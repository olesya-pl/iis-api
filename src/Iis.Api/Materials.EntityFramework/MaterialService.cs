using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;

using IIS.Core.Files;
using IIS.Core.Materials.EntityFramework.FeatureProcessors;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;
using IIS.Repository;
using IIS.Repository.Factories;
using MaterialLoadData = Iis.Domain.Materials.MaterialLoadData;
using IIS.Repository.UnitOfWork;
using System.Threading;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialService<TUnitOfWork> : BaseService<TUnitOfWork>, IMaterialService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IMaterialEventProducer _eventProducer;
        private readonly IMaterialProvider _materialProvider;
        private readonly IEnumerable<IMaterialProcessor> _materialProcessors;
        private readonly IMLResponseRepository _mLResponseRepository;

        public MaterialService(IFileService fileService,
            IMapper mapper,
            IMaterialEventProducer eventProducer,
            IMaterialProvider materialProvider,
            IEnumerable<IMaterialProcessor> materialProcessors,
            IMLResponseRepository mLResponseRepository,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _fileService = fileService;
            _mapper = mapper;
            _eventProducer = eventProducer;
            _materialProvider = materialProvider;
            _materialProcessors = materialProcessors;
            _mLResponseRepository = mLResponseRepository;
        }

        public async Task SaveAsync(Material material)
        {
            if (material.File != null) // if material has attached file
            {
                var file = await _fileService.GetFileAsync(material.File.Id); // explicit file check in case of no FK to file service
                if (file == null) throw new ArgumentException($"File with guid {material.File.Id} was not found");
                if (!file.IsTemporary) throw new ArgumentException($"File with guid {material.File.Id} is already used");

                await _fileService.MarkFilePermanentAsync(file.Id);

                // todo: implement correct file type - material type compatibility checking
                if (material.Type == "cell.voice" && !file.ContentType.StartsWith("audio/"))
                    throw new ArgumentException($"\"{material.Type}\" material expects audio file to be attached. Got \"{file.ContentType}\"");
            }

            if (material.ParentId.HasValue && _materialProvider.GetMaterialAsync(material.ParentId.Value) == null)
                throw new ArgumentException($"Material with guid {material.ParentId.Value} does not exist");

            var materialEntity = _mapper.Map<MaterialEntity>(material);

            Run(unitOfWork => { unitOfWork.MaterialRepository.AddMaterialEntity(materialEntity); });

            foreach (var child in material.Children)
            {
                child.ParentId = material.Id;
                await SaveAsync(child);
            }
            var materialInfoEntities = material.Infos.Select(info => Map(info, material.Id)).ToList();
            var materialFeatures = new List<MaterialFeatureEntity>();
            foreach (var featureId in GetNodeIdentitiesFromFeatures(material.Metadata))
            {
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

            await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.PutMaterialToElasticSearchAsync(material.Id));

            if (material.ParentId == null)
            {
                _eventProducer.SendAvailableForOperatorEvent(materialEntity.Id);
            }
            _eventProducer.SendMaterialEvent(new MaterialEventMessage { Id = materialEntity.Id, Source = materialEntity.Source, Type = materialEntity.Type });

            // todo: put message to rabbit instead of calling another service directly
            if (material.Metadata.SelectToken("Features.Nodes") != null)
            {
                foreach (var processor in _materialProcessors)
                    await processor.ExtractInfoAsync(material);
            }
            // end
            // todo: multiple queues for different material types
            if (material.File != null && material.Type == "cell.voice")
                _eventProducer.SendMaterialAddedEventAsync(
                    new MaterialAddedEvent { FileId = material.File.Id, MaterialId = material.Id });
        }


        public async Task<MLResponse> SaveMlHandlerResponseAsync(MLResponse response)

        {
            var responseEntity = _mapper.Map<MLResponseEntity>(response);

            responseEntity = await _mLResponseRepository.SaveAsync(responseEntity);

            await RunWithoutCommitAsync(async unitOfWork =>
                await unitOfWork.MaterialRepository.PutMaterialToElasticSearchAsync(responseEntity.MaterialId));

            return _mapper.Map<MLResponse>(responseEntity);
        }

        public async Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input)
        {
            var material = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.MaterialRepository.GetByIdAsync(input.Id));

            if (!string.IsNullOrWhiteSpace(input.Title)) material.Title = input.Title;
            if (input.ImportanceId.HasValue) material.ImportanceSignId = input.ImportanceId.Value;
            if (input.ReliabilityId.HasValue) material.ReliabilitySignId = input.ReliabilityId.Value;
            if (input.RelevanceId.HasValue) material.RelevanceSignId = input.RelevanceId.Value;
            if (input.CompletenessId.HasValue) material.CompletenessSignId = input.CompletenessId.Value;
            if (input.SourceReliabilityId.HasValue) material.SourceReliabilitySignId = input.SourceReliabilityId.Value;
            if (input.ProcessedStatusId.HasValue) material.ProcessedStatusSignId = input.ProcessedStatusId.Value;
            if (input.SessionPriorityId.HasValue) material.SessionPriorityId = input.SessionPriorityId.Value;
            if (input.AssigneeId.HasValue) material.AssigneeId = input.AssigneeId;
            if (input.Content != null) material.Content = input.Content;

            var loadData = MaterialLoadData.MapLoadData(material.LoadData);

            if (input.Objects != null) loadData.Objects = new List<string>(input.Objects);
            if (input.Tags != null) loadData.Tags = new List<string>(input.Tags);
            if (input.States != null) loadData.States = new List<string>(input.States);

            material.LoadData = loadData.ToJson();

            await UpdateMaterialAsync(material);

            return await _materialProvider.GetMaterialAsync(input.Id);
        }

        public async Task UpdateMaterialAsync(MaterialEntity material)
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

        public Task<int> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken)
        {
            return RunWithoutCommitAsync(async (unitOfWork)
                => await unitOfWork.MaterialRepository.PutAllMaterialsToElasticSearchAsync(cancellationToken));
        }
    }
}
