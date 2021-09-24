using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using MediatR;
using Iis.Utility;
using Iis.Messages.Materials;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DbLayer.Repositories;
using Iis.DbLayer.MaterialEnum;
using Iis.DbLayer.MaterialDictionaries;
using Iis.DataModel.Materials;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Materials;
using IIS.Repository;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using MaterialLoadData = Iis.Domain.Materials.MaterialLoadData;
using Iis.Interfaces.Common;
using Microsoft.Extensions.Logging;
using Iis.Domain.Users;
using IIS.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Materials.Distribution;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialService<TUnitOfWork> : BaseService<TUnitOfWork>, IMaterialService where TUnitOfWork : IIISUnitOfWork
    {
        private const string AssigneeNameSeparator = "; ";

        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IMaterialEventProducer _eventProducer;
        private readonly IMaterialProvider _materialProvider;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly IMaterialSignRepository _materialSignRepository;
        private readonly IUserService _userService;
        private readonly ICommonData _commonData;
        private readonly ILogger<MaterialService<TUnitOfWork>> _logger;
        private readonly IMaterialElasticService _materialElasticService;

        public MaterialService(IFileService fileService,
            IMapper mapper,
            IMaterialEventProducer eventProducer,
            IMaterialProvider materialProvider,
            IMLResponseRepository mLResponseRepository,
            IChangeHistoryService changeHistoryService,
            IUserService userService,
            IMediator mediator,
            IMaterialSignRepository materialSignRepository,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            ICommonData commonData,
            ILogger<MaterialService<TUnitOfWork>> logger,
            IMaterialElasticService materialElasticService) : base(unitOfWorkFactory)
        {
            _fileService = fileService;
            _mapper = mapper;
            _eventProducer = eventProducer;
            _materialProvider = materialProvider;
            _mLResponseRepository = mLResponseRepository;
            _changeHistoryService = changeHistoryService;
            _userService = userService;
            _materialSignRepository = materialSignRepository;
            _commonData = commonData;
            _logger = logger;
            _materialElasticService = materialElasticService;
        }

        public async Task SaveAsync(Material material, Guid? changeRequestId = null)
        {
            await MakeFilePermanent(material);
            await ValidateMaterialParent(material);

            var materialEntity = _mapper.Map<MaterialEntity>(material);

            await RunAsync(unitOfWork => { unitOfWork.MaterialRepository.AddMaterialEntity(materialEntity); });

            var requestId = changeRequestId.GetValueOrDefault(Guid.NewGuid());
            await SaveMaterialChangeHistory(material, requestId);
            await SaveMaterialChildren(material, requestId);
            await SaveMaterialInfoEntities(material);

            _eventProducer.PublishMaterialCreatedMessage(CreatedMessage(material));
        }

        private MaterialCreatedMessage CreatedMessage(Material material)
        {
            return new MaterialCreatedMessage
            {
                MaterialId = material.Id,
                FileId = material.FileId,
                ParentId = material.ParentId,
                CreatedDate = material.CreatedDate,
                Type = material.Type,
                Source = material.Source,
                Channel = material.Channel
            };
        }

        private Task SaveMaterialChangeHistory(Material material, Guid changeRequestId)
        {
            var timeSpan = DateTime.UtcNow;
            var changeItems = new List<ChangeHistoryDto>
            {
                new ChangeHistoryDto
                {
                    Date = timeSpan,
                    NewValue = material.Id.ToString(),
                    PropertyName = nameof(material.Id),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                },
                new ChangeHistoryDto
                {
                    Date = timeSpan,
                    NewValue = material.Source,
                    PropertyName = nameof(material.Source),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                },
                new ChangeHistoryDto
                {
                    Date = timeSpan,
                    NewValue = material.Channel,
                    PropertyName = nameof(material.Channel),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                },
                new ChangeHistoryDto
                {
                    Date = timeSpan,
                    NewValue = material.LoadData.LoadedBy,
                    PropertyName = nameof(material.LoadData.LoadedBy),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                },
                new ChangeHistoryDto
                {
                    Date = timeSpan,
                    NewValue = material.AccessLevel.ToString("D"),
                    PropertyName = nameof(material.AccessLevel),
                    RequestId = changeRequestId,
                    TargetId = material.Id,
                }
            };
            return _changeHistoryService.SaveMaterialChanges(changeItems);
        }

        private async Task MakeFilePermanent(Material material)
        {
            if (material.HasAttachedFile())
            {
                var file = await _fileService.GetFileAsync(material.File.Id);
                if (file == null) throw new ArgumentException($"File with guid {material.File.Id} was not found");
                if (!file.IsTemporary) throw new ArgumentException($"File with guid {material.File.Id} is already used");

                await _fileService.MarkFilePermanentAsync(file.Id);

                // todo: implement correct file type - material type compatibility checking
                if (material.Type == "cell.voice" && !file.ContentType.StartsWith("audio/"))
                    throw new ArgumentException($"\"{material.Type}\" material expects audio file to be attached. Got \"{file.ContentType}\"");
            }
        }

        private async Task ValidateMaterialParent(Material material)
        {
            if (material.ParentId.HasValue && (await _materialProvider.MaterialExists(material.ParentId.Value)) == false)
                throw new ArgumentException($"Material with guid {material.ParentId.Value} does not exist");
        }

        private async Task SaveMaterialChildren(Material material, Guid changeRequestId)
        {
            foreach (var child in material.Children)
            {
                child.ParentId = material.Id;
                await SaveAsync(child, changeRequestId);
            }
        }

        private Task SaveMaterialInfoEntities(Material material)
        {
            var materialInfoEntities = material.Infos.Select(info => Map(info, material.Id)).ToList();
            return RunAsync(unitOfWork =>
            {
                unitOfWork.MaterialRepository.AddMaterialInfos(materialInfoEntities);
            });
        }

        public async Task<MLResponse> SaveMlHandlerResponseAsync(MLResponse response)
        {
            var responseEntity = _mapper.Map<MLResponseEntity>(response);

            responseEntity = await _mLResponseRepository.SaveAsync(responseEntity);

            await _materialElasticService.PutMaterialToElasticSearchAsync(responseEntity.MaterialId);

            if (responseEntity.HandlerCode == MlHandlerCodeList.ImageVector)
            {
                var parentId = await RunWithoutCommitAsync(uow => uow.MaterialRepository.GetParentIdByChildIdAsync(responseEntity.MaterialId));
                if (parentId.HasValue)
                    await _materialElasticService.PutMaterialToElasticSearchAsync(parentId.Value);
            }

            return _mapper.Map<MLResponse>(responseEntity);
        }

        public async Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input, User user)
        {
            var material = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.MaterialRepository.GetByIdAsync(input.Id, new[] { MaterialIncludeEnum.WithChildren }));
            var username = user.UserName;
            if (material.CanBeEdited(user.Id))
            {
                var changeRequestId = Guid.NewGuid();
                var changesList = new List<ChangeHistoryDto>();

                if (!string.IsNullOrWhiteSpace(input.Title)) material.Title = input.Title;

                input.ImportanceId.DoIfHasValue(p =>
                {
                    CreateChangeHistory(material.Id,
                        material.ImportanceSignId,
                        nameof(material.Importance),
                        p, username, changeRequestId, changesList);
                    material.Importance = null;
                    material.ImportanceSignId = p;
                });

                input.ReliabilityId.DoIfHasValue(p =>
                {
                    CreateChangeHistory(material.Id,
                        material.ReliabilitySignId,
                        nameof(material.Reliability),
                        p, username, changeRequestId, changesList);
                    material.Reliability = null;
                    material.ReliabilitySignId = p;
                });

                input.RelevanceId.DoIfHasValue(p =>
                {
                    CreateChangeHistory(material.Id,
                        material.RelevanceSignId,
                        nameof(material.Relevance),
                        p, username, changeRequestId, changesList);
                    material.Relevance = null;
                    material.RelevanceSignId = p;
                });

                input.CompletenessId.DoIfHasValue(p =>
                {
                    CreateChangeHistory(material.Id,
                        material.CompletenessSignId,
                        nameof(material.Completeness),
                        p, username, changeRequestId, changesList);
                    material.Completeness = null;
                    material.CompletenessSignId = p;
                });

                input.SourceReliabilityId.DoIfHasValue(p =>
                {
                    CreateChangeHistory(material.Id,
                        material.SourceReliabilitySignId,
                        nameof(material.SourceReliability),
                        p, username, changeRequestId, changesList);
                    material.SourceReliability = null;
                    material.SourceReliabilitySignId = p;
                });

                input.ProcessedStatusId.DoIfHasValue(p =>
                {
                    CreateChangeHistory(material.Id,
                        material.ProcessedStatusSignId,
                        nameof(material.ProcessedStatus),
                        p, username, changeRequestId, changesList);
                    material.ProcessedStatus = null;
                    material.ProcessedStatusSignId = p;
                });

                input.SessionPriorityId.DoIfHasValue(p =>
                {
                    CreateChangeHistory(material.Id,
                        material.SessionPriorityId,
                        nameof(material.SessionPriority),
                        p, username, changeRequestId, changesList);
                    material.SessionPriority = null;
                    material.SessionPriorityId = p;
                });

                var eventReassignmentNeeded = false;
                if (input.Content != null && !string.Equals(material.Content, input.Content, StringComparison.Ordinal))
                {
                    eventReassignmentNeeded = true;
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = input.Content,
                        OldValue = material.Content,
                        PropertyName = nameof(material.Content),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.Content = input.Content;
                }

                var loadData = MaterialLoadData.MapLoadData(material.LoadData);

                var loadDataStringified = loadData.ToJson();
                if (!string.Equals(loadDataStringified, material.LoadData, StringComparison.Ordinal))
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = loadDataStringified,
                        OldValue = material.LoadData,
                        PropertyName = nameof(material.LoadData),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.Content = input.Content;
                    if (input.Objects != null) loadData.Objects = new List<string>(input.Objects);
                    if (input.Tags != null) loadData.Tags = new List<string>(input.Tags);
                    if (input.States != null) loadData.States = new List<string>(input.States);
                    material.LoadData = loadDataStringified;
                }

                Run((unitOfWork) => { unitOfWork.MaterialRepository.EditMaterial(material); });

                if (input.AssigneeIds != null)
                {
                    var change = await AssignMaterialOperatorsAsync(material, input.AssigneeIds.ToHashSet(), user, changeRequestId);
                    if (change != default)
                        changesList.Add(change);
                }

                var fillElasticTask = _materialElasticService.PutMaterialToElasticSearchAsync(material.Id, waitForIndexing: true);

                var addHistoryTask = _changeHistoryService.SaveMaterialChanges(changesList);

                await Task.WhenAll(new[] { fillElasticTask, addHistoryTask });

                if (eventReassignmentNeeded)
                {
                    SendMaterialUpdatedMessage(material);
                }

                if (MaterialShouldBeQueuedForMachineLearning(material))
                {
                    QueueMaterialForMachineLearning(material);
                }
            }
            return await _materialProvider.GetMaterialAsync(input.Id, user);
        }

        private void SendMaterialUpdatedMessage(MaterialEntity material)
        {
            _eventProducer.SendMaterialSavedToElastic(new List<Guid>() { material.Id });
        }

        private void CreateChangeHistory(Guid targetId,
            Guid? destinationId,
            string destinationName,
            Guid value,
            string userName,
            Guid requestId,
            List<ChangeHistoryDto> changesList)
        {
            if (destinationId.HasValue && destinationId.Value == value)
            {
                return;
            }

            changesList.Add(new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = _materialSignRepository.GetById(value).Title,
                OldValue = destinationId.HasValue
                                ? _materialSignRepository.GetById(destinationId.Value).Title
                                : string.Empty,
                PropertyName = destinationName,
                UserName = userName,
                RequestId = requestId,
                TargetId = targetId
            });
        }

        private void QueueMaterialForMachineLearning(MaterialEntity material)
        {
            _eventProducer.SendMaterialEvent(new MaterialProcessingEventMessage { Id = material.Id, Source = material.Source, Type = material.Type });

            foreach (var child in material.Children)
            {
                _eventProducer.SendMaterialEvent(new MaterialProcessingEventMessage { Id = child.Id, Source = child.Source, Type = child.Type });
            }
        }

        private bool MaterialShouldBeQueuedForMachineLearning(MaterialEntity material)
        {
            return material.ProcessedStatusSignId.HasValue && material.ProcessedStatusSignId == MaterialEntity.ProcessingStatusProcessedSignId;
        }

        public Task AssignMaterialOperatorAsync(ISet<Guid> materialIds, ISet<Guid> assigneeIds, User user)
        {
            var tasks = materialIds.Select(_ => AssignMaterialOperatorsAsync(_, assigneeIds, user));

            return Task.WhenAll(tasks);
        }

        public Task SaveDistributionResult(DistributionResult distributionResult)
        {
            return RunAsync(_ => _.MaterialRepository.SaveDistributionResult(distributionResult));
        }

        public async Task AssignMaterialOperatorsAsync(Guid materialId, ISet<Guid> assigneeIds, User user)
        {
            var material = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdAsync(materialId));
            if (material == null
                || !material.CanBeAccessedBy(user.AccessLevel))
                return;

            var existingMaterialAssigneeIds = material.MaterialAssignees.Select(_ => _.AssigneeId);
            IReadOnlyCollection<Guid> addedAssigneeIds = assigneeIds.Except(existingMaterialAssigneeIds).ToArray();
            if (addedAssigneeIds.Count == 0)
                return;

            var addedAssignees = await RunWithoutCommitAsync(_ => _.UserRepository.GetUsersAsync(user => addedAssigneeIds.Contains(user.Id)));
            var oldAssigneeNames = material.MaterialAssignees.Select(_ => _.Assignee.Username).ToArray();
            string oldValue = GetAssigneeChangeValue(oldAssigneeNames);
            var addedAssigneeNames = addedAssignees.Select(_ => _.Username);
            string newValue = GetAssigneeChangeValue(oldAssigneeNames.Concat(addedAssigneeNames));

            var addedEntities = addedAssignees.Select(_ => MaterialAssigneeEntity.CreateFrom(material.Id, _.Id));

            await RunAsync(_ => _.MaterialRepository.AddMaterialAssignees(addedEntities));

            var change = new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = newValue,
                OldValue = oldValue,
                PropertyName = nameof(material.MaterialAssignees),
                RequestId = Guid.NewGuid(),
                TargetId = material.Id,
                UserName = user.UserName
            };

            var putToElasticTask = _materialElasticService.PutMaterialToElasticSearchAsync(material.Id);
            var addHistoryTask = _changeHistoryService.SaveMaterialChanges(change.AsReadOnlyCollection());

            await Task.WhenAll(putToElasticTask, addHistoryTask);
        }

        public async Task<bool> AssignMaterialEditorAsync(Guid materialId, User user)
        {
            var accessLevel = user.AccessLevel;
            var material = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdAsync(materialId));
            if (material == null
                || material.EditorId != null
                || !material.CanBeAccessedBy(accessLevel))
                return false;

            material.EditorId = user.Id;

            Run(_ => _.MaterialRepository.EditMaterial(material));
            await _materialElasticService.PutMaterialToElasticSearchAsync(material.Id);

            return true;
        }

        public async Task<bool> UnassignMaterialEditorAsync(Guid materialId, User user)
        {
            var accessLevel = user.AccessLevel;
            var material = await RunWithoutCommitAsync(_ => _.MaterialRepository.GetByIdAsync(materialId));
            if (material == null
                || material.EditorId != user.Id
                || !material.CanBeAccessedBy(accessLevel))
                return false;

            material.EditorId = null;

            Run(_ => _.MaterialRepository.EditMaterial(material));
            await _materialElasticService.PutMaterialToElasticSearchAsync(material.Id);

            return true;
        }

        public async Task SetMachineLearningHadnlersCount(Guid materialId, int handlersCount)
        {
            var material = await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.GetByIdAsync(materialId));

            if (material == null)
            {
                throw new ArgumentNullException($"Material with given id not found");
            }

            material.MlHandlersCount += handlersCount;
            Run(unitOfWork => unitOfWork.MaterialRepository.EditMaterial(material));
            await _materialElasticService.PutMaterialToElasticSearchAsync(material.Id);
        }

        private MaterialInfoEntity Map(MaterialInfo info, Guid materialId)
        {
            return new MaterialInfoEntity
            {
                Id = info.Id,
                MaterialId = materialId,
                Data = info.Data.ToString(),
                Source = info.Source,
                SourceType = info.SourceType,
                SourceVersion = info.SourceVersion,
            };
        }

        public async Task<Material> ChangeMaterialAccessLevel(Guid materialId, int accessLevel, User user)
        {
            var accessLevelValidationResult = IsValidAccessLevel(accessLevel);

            if (!accessLevelValidationResult.IsValid) throw new ArgumentException("Wrong Access level value");

            if (!IsUserAuthorizedForChangeAccessLevel(user) || !_userService.IsAccessLevelAllowedForUser(user.AccessLevel, accessLevelValidationResult.Value))
            {
                throw new InvalidOperationException($"Unable to change AccessLevel by user {user.UserName}");
            }

            var material = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.MaterialRepository.GetByIdAsync(materialId));

            var changeHistory = new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = accessLevelValidationResult.Value.ToString(),
                OldValue = material.AccessLevel.ToString(),
                PropertyName = nameof(material.AccessLevel),
                RequestId = Guid.NewGuid(),
                TargetId = material.Id,
                UserName = user.UserName
            };

            material.AccessLevel = accessLevelValidationResult.Value;

            Run(uow => uow.MaterialRepository.EditMaterial(material));

            var elasticTask = _materialElasticService.PutMaterialToElasticSearchAsync(material.Id, waitForIndexing: true);

            var changeHistoryTask = _changeHistoryService.SaveMaterialChanges(new[] { changeHistory });

            var getMaterialTask = _materialProvider.GetMaterialAsync(materialId, user);

            await Task.WhenAll(new[] { elasticTask, changeHistoryTask, getMaterialTask });

            return getMaterialTask.Result;
        }

        public async Task RemoveMaterials()
        {
            _logger.LogDebug("Start RemoveMaterials");
            var fileIds = await RunWithoutCommitAsync(uow => uow.FileRepository.GetMaterialFileIds());
            _logger.LogDebug("Found {Count} files to remove", fileIds.Count);
            await RunAsync(uow => uow.MaterialRepository.RemoveMaterialsAndRelatedData(fileIds));
            var removeFiles = _fileService.RemoveFiles(fileIds);
            _logger.LogDebug("Removed {Count} files", removeFiles);
        }

        private (bool IsValid, int Value) IsValidAccessLevel(int accessLevel)
        {
            var isValid = _commonData.AccessLevels.IndexIsValid(accessLevel);

            return (IsValid: isValid, Value: isValid ? accessLevel : 0);
        }
        private bool IsUserAuthorizedForChangeAccessLevel(User user)
        {
            return user.IsGranted(AccessKind.Material, AccessOperation.AccessLevelUpdate, AccessCategory.Entity);
        }

        private async Task<ChangeHistoryDto> AssignMaterialOperatorsAsync(
            MaterialEntity material,
            ISet<Guid> assigneeIds,
            User user,
            Guid changeRequestId)
        {
            var existingAssignees = await RunWithoutCommitAsync(_ => _.UserRepository.GetUsersAsync(user => assigneeIds.Contains(user.Id)));
            var existingAssigneeDictionary = existingAssignees.ToDictionary(_ => _.Id);
            var existingMaterialAssigneeIds = material.MaterialAssignees.Select(_ => _.AssigneeId);
            var (added, removed) = existingAssigneeDictionary.Keys.GetChanges(existingMaterialAssigneeIds);
            if (added.Count == 0
                && removed.Count == 0)
                return default;

            string oldValue = GetAssigneeChangeValue(material.MaterialAssignees.Select(_ => _.Assignee.Username));
            string newValue = GetAssigneeChangeValue(existingAssignees.Select(_ => _.Username));

            await RunAsync(unitOfWork =>
            {
                if (added.Count > 0)
                {
                    var addedEntities = added.Select(_ => MaterialAssigneeEntity.CreateFrom(material.Id, _));
                    unitOfWork.MaterialRepository.AddMaterialAssignees(addedEntities);
                }
                if (removed.Count > 0)
                {
                    var removedEntities = removed.Select(_ => MaterialAssigneeEntity.CreateFrom(material.Id, _));
                    unitOfWork.MaterialRepository.RemoveMaterialAssignees(removedEntities);
                }
            });

            return new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = newValue,
                OldValue = oldValue,
                PropertyName = nameof(material.MaterialAssignees),
                RequestId = changeRequestId,
                TargetId = material.Id,
                UserName = user.UserName
            };
        }

        private string GetAssigneeChangeValue(IEnumerable<string> userNames) => string.Join(AssigneeNameSeparator, userNames);
    }
}