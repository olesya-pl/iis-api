using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json.Linq;
using IIS.Core.Materials.EntityFramework.FeatureProcessors;
using Iis.DataModel.Materials;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Materials;
using IIS.Repository;
using IIS.Repository.Factories;
using MaterialLoadData = Iis.Domain.Materials.MaterialLoadData;
using System.Threading;
using Iis.DbLayer.MaterialEnum;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Interfaces;
using Iis.Interfaces.Ontology.Data;
using MediatR;
using Iis.Events.Materials;
using Iis.Services.Contracts.Dtos;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialService<TUnitOfWork> : BaseService<TUnitOfWork>, IMaterialService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IMaterialEventProducer _eventProducer;
        private readonly IMaterialProvider _materialProvider;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IOntologyNodesData _nodesData;
        private readonly IChangeHistoryService _changeHistoryService;
        private readonly IMaterialSignRepository _materialSignRepository;
        private readonly IMediator _mediatr;

        public MaterialService(IFileService fileService,
            IMapper mapper,
            IMaterialEventProducer eventProducer,
            IMaterialProvider materialProvider,
            IMLResponseRepository mLResponseRepository,
            IOntologyNodesData nodesData,
            IChangeHistoryService changeHistoryService,
            IMediator mediator,
            IMaterialSignRepository materialSignRepository,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _fileService = fileService;
            _mapper = mapper;
            _eventProducer = eventProducer;
            _materialProvider = materialProvider;
            _mLResponseRepository = mLResponseRepository;
            _nodesData = nodesData;
            _changeHistoryService = changeHistoryService;
            _materialSignRepository = materialSignRepository;
            _mediatr = mediator;
        }

        public async Task SaveAsync(Material material)
        {
            await MakeFilePermanent(material);
            await ValidateMaterialParent(material);

            var materialEntity = _mapper.Map<MaterialEntity>(material);

            Run(unitOfWork => { unitOfWork.MaterialRepository.AddMaterialEntity(materialEntity); });
            var changeRequestId = Guid.NewGuid();
            await SaveMaterialChangeHistory(material, changeRequestId);
            await SaveMaterialChildren(material, changeRequestId);
            SaveMaterialInfoEntitites(material);

            await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.PutMaterialToElasticSearchAsync(material.Id));

            if (material.IsParentMaterial())
            {
                _eventProducer.SendAvailableForOperatorEvent(materialEntity.Id);
            }

            var message = new MaterialEventMessage { Id = materialEntity.Id, Source = materialEntity.Source, Type = materialEntity.Type };

            _eventProducer.SendMaterialEvent(message);

            _eventProducer.SendMaterialFeatureEvent(message);

            // todo: multiple queues for different material types
            if (material.File != null && material.Type == "cell.voice")
                _eventProducer.SendMaterialAddedEventAsync(
                    new MaterialAddedEvent { FileId = material.File.Id, MaterialId = material.Id });

            await _mediatr.Publish(new MaterialCreatedEvent());
        }

        private Task SaveMaterialChangeHistory(Material material, Guid changeRequestId)
        {
            var changeItems = new List<ChangeHistoryDto>();
            changeItems.Add(new ChangeHistoryDto 
            {
                Date = DateTime.UtcNow,
                NewValue = material.Id.ToString(),
                PropertyName = nameof(material.Id),
                RequestId = changeRequestId,
                TargetId = material.Id,                
            });
            changeItems.Add(new ChangeHistoryDto
            {
                Date = DateTime.UtcNow,
                NewValue = material.Source,
                PropertyName = nameof(material.Source),
                RequestId = changeRequestId,
                TargetId = material.Id,
            });
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
                await SaveAsync(child);
                await SaveMaterialChangeHistory(child, changeRequestId);
            }
        }

        private void SaveMaterialInfoEntitites(Material material)
        {
            var materialInfoEntities = material.Infos.Select(info => Map(info, material.Id)).ToList();
            var materialFeatures = new List<MaterialFeatureEntity>();

            foreach (var featureId in GetNodeIdentitiesFromFeatures(material.Metadata))
            {
                if (string.Equals(material.Source, "osint.data.file", StringComparison.Ordinal))
                {
                    var node = _nodesData.GetNode(featureId);
                    if (node == null)
                    {
                        continue;
                    }
                }

                materialFeatures.Add(new MaterialFeatureEntity
                {
                    NodeId = featureId,
                    MaterialInfo = new MaterialInfoEntity
                    {
                        MaterialId = material.Id
                    }
                });
            }

            Run(unitOfWork =>
            {
                unitOfWork.MaterialRepository.AddMaterialInfos(materialInfoEntities);
                unitOfWork.MaterialRepository.AddMaterialFeatures(materialFeatures);
            });
        }

        public async Task<MLResponse> SaveMlHandlerResponseAsync(MLResponse response)

        {
            var responseEntity = _mapper.Map<MLResponseEntity>(response);

            responseEntity = await _mLResponseRepository.SaveAsync(responseEntity);

            await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.PutMaterialToElasticSearchAsync(responseEntity.MaterialId));

            return _mapper.Map<MLResponse>(responseEntity);
        }

        public async Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input, Guid userId, string username)
        {
            var material = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.MaterialRepository.GetByIdAsync(input.Id, new[] { MaterialIncludeEnum.WithChildren }));

            if (material.CanBeEdited(userId))
            {
                var changeRequestId = Guid.NewGuid();
                var changesList = new List<ChangeHistoryDto>();
                
                if (!string.IsNullOrWhiteSpace(input.Title)) material.Title = input.Title;
                if (input.ImportanceId.HasValue)
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = _materialSignRepository.GetById(input.ImportanceId.Value).Title,
                        OldValue = material.ImportanceSignId.HasValue
                            ? _materialSignRepository.GetById(material.ImportanceSignId.Value).Title
                            : string.Empty,
                        PropertyName = nameof(material.Importance),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.ImportanceSignId = input.ImportanceId.Value;
                    material.Importance = null;
                    
                }
                if (input.ReliabilityId.HasValue)
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = _materialSignRepository.GetById(input.ReliabilityId.Value).Title,
                        OldValue = material.ReliabilitySignId.HasValue
                            ? _materialSignRepository.GetById(material.ReliabilitySignId.Value).Title
                            : string.Empty,
                        PropertyName = nameof(material.Reliability),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.ReliabilitySignId = input.ReliabilityId.Value;
                    material.Reliability = null;
                }
                if (input.RelevanceId.HasValue)
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = _materialSignRepository.GetById(input.RelevanceId.Value).Title,
                        OldValue = material.RelevanceSignId.HasValue
                            ? _materialSignRepository.GetById(material.RelevanceSignId.Value).Title
                            : string.Empty,
                        PropertyName = nameof(material.Relevance),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.RelevanceSignId = input.RelevanceId.Value;
                    material.Relevance = null;
                }
                if (input.CompletenessId.HasValue)
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = _materialSignRepository.GetById(input.CompletenessId.Value).Title,
                        OldValue = material.CompletenessSignId.HasValue
                            ? _materialSignRepository.GetById(material.CompletenessSignId.Value).Title
                            : string.Empty,
                        PropertyName = nameof(material.Completeness),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.CompletenessSignId = input.CompletenessId.Value;
                    material.Completeness = null;
                }
                if (input.SourceReliabilityId.HasValue)
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = _materialSignRepository.GetById(input.SourceReliabilityId.Value).Title,
                        OldValue = material.SourceReliabilitySignId.HasValue
                            ? _materialSignRepository.GetById(material.SourceReliabilitySignId.Value).Title
                            : string.Empty,
                        PropertyName = nameof(material.SourceReliability),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.SourceReliabilitySignId = input.SourceReliabilityId.Value;
                    material.SourceReliability = null;
                }
                if (input.ProcessedStatusId.HasValue)
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = _materialSignRepository.GetById(input.ProcessedStatusId.Value).Title,
                        OldValue = material.ProcessedStatusSignId.HasValue
                            ? _materialSignRepository.GetById(material.ProcessedStatusSignId.Value).Title
                            : string.Empty,
                        PropertyName = nameof(material.ProcessedStatus),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.ProcessedStatusSignId = input.ProcessedStatusId.Value;
                    material.ProcessedStatus = null;
                }
                if (input.SessionPriorityId.HasValue)
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = _materialSignRepository.GetById(input.SessionPriorityId.Value).Title,
                        OldValue = material.SessionPriorityId.HasValue
                            ? _materialSignRepository.GetById(material.SessionPriorityId.Value).Title
                            : string.Empty,
                        PropertyName = nameof(material.SessionPriority),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.SessionPriorityId = input.SessionPriorityId.Value;
                    material.SessionPriority = null;
                }
                if (input.AssigneeId.HasValue) 
                {
                    changesList.Add(new ChangeHistoryDto
                    {
                        Date = DateTime.UtcNow,
                        NewValue = input.AssigneeId.Value.ToString(),
                        OldValue = material.AssigneeId.ToString(),
                        PropertyName = nameof(material.Assignee),
                        RequestId = changeRequestId,
                        TargetId = material.Id,
                        UserName = username
                    });
                    material.AssigneeId = input.AssigneeId;
                }
                if (input.Content != null) 
                {
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

                await _changeHistoryService.SaveMaterialChanges(changesList);
                await UpdateMaterialAsync(material);
                QueueMaterialChildrenForMl(material);                
            }
            return await _materialProvider.GetMaterialAsync(input.Id, userId);
        }

        private void QueueMaterialChildrenForMl(MaterialEntity material)
        {
            foreach (var child in material.Children)
            {
                _eventProducer.SendMaterialEvent(new MaterialEventMessage { Id = child.Id, Source = child.Source, Type = child.Type });
            }
        }

        private async Task UpdateMaterialAsync(MaterialEntity material)
        {
            Run((unitOfWork) => { unitOfWork.MaterialRepository.EditMaterial(material); });

            await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.PutMaterialToElasticSearchAsync(material.Id));

            _eventProducer.SendMaterialEvent(new MaterialEventMessage { Id = material.Id, Source = material.Source, Type = material.Type });
        }

        public async Task AssignMaterialOperatorAsync(Guid materialId, Guid assigneeId)
        {
            var material = await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.GetByIdAsync(materialId));
            if (material == null)
            {
                return;
            }
            material.AssigneeId = assigneeId;
            Run(unitOfWork => unitOfWork.MaterialRepository.EditMaterial(material));
            await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.PutMaterialToElasticSearchAsync(material.Id));
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
            await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.PutMaterialToElasticSearchAsync(material.Id));
        }

        private IEnumerable<Guid> GetNodeIdentitiesFromFeatures(JObject metadata)
        {
            var result = new List<Guid>();

            var features = metadata.SelectToken(FeatureFields.FeaturesSection);

            if (features is null) return result;

            foreach (JObject feature in features)
            {
                var featureId = feature.GetValue(FeatureFields.featureId)?.Value<string>();

                if (string.IsNullOrWhiteSpace(featureId)) continue;

                if (!Guid.TryParse(featureId, out Guid featureGuid)) continue;

                if (featureGuid.Equals(Guid.Empty)) continue;

                result.Add(featureGuid);
            }

            return result;
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

        public Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken)
        {
            return RunWithoutCommitAsync(async (unitOfWork)
                => await unitOfWork.MaterialRepository.PutAllMaterialsToElasticSearchAsync(cancellationToken));
        }
    }
}
