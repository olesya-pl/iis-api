using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using AutoMapper;

using Iis.Utility;
using Iis.Roles;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.DbLayer.MaterialEnum;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;
using MaterialSign = Iis.Domain.Materials.MaterialSign;
using Newtonsoft.Json;
using IIS.Repository;
using IIS.Repository.Factories;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider<TUnitOfWork> : BaseService<TUnitOfWork>, IMaterialProvider where TUnitOfWork : IIISUnitOfWork
    {
        private static readonly JsonSerializerSettings _materialDocSerializeSettings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None
        };
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private readonly IElasticService _elasticService;
        private readonly IMapper _mapper;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IMaterialSignRepository _materialSignRepository;

        public MaterialProvider(IOntologyService ontologyService,
            IOntologySchema ontologySchema,
            IElasticService elasticService,
            IMLResponseRepository mLResponseRepository,
            IMaterialSignRepository materialSignRepository,
            IMapper mapper,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory) : base(unitOfWorkFactory)
        {
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _elasticService = elasticService;

            _mLResponseRepository = mLResponseRepository;
            _materialSignRepository = materialSignRepository;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<Material> Materials, int Count, Dictionary<Guid, SearchByConfiguredFieldsResultItem> Highlights)>
            GetMaterialsAsync(int limit, int offset, string filterQuery,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null,
            string sortColumnName = null, string sortOrder = null)
        {
            IEnumerable<Task<Material>> mappingTasks;
            IEnumerable<Material> materials;
            (IEnumerable<MaterialEntity> Materials, int TotalCount) materialResult;

            if (!string.IsNullOrWhiteSpace(filterQuery))
            {
                var searchResult = await _elasticService.SearchMaterialsByConfiguredFieldsAsync(
                    new ElasticFilter { Limit = limit, Offset = offset, Suggestion = filterQuery });

                var materialTasks = searchResult.Items.Values
                    .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                    .Select(p => MapMaterialDocumentAsync(p));

                materials = await Task.WhenAll(materialTasks);

                return (materials, searchResult.Count, searchResult.Items);
            }

            if (types != null)
            {
                materialResult = await RunWithoutCommitAsync(async (unitOfWork) =>
                    await unitOfWork.MaterialRepository.GetAllAsync(types, limit, offset));
            }
            else
            {
                materialResult = await RunWithoutCommitAsync(async (unitOfWork) =>
                    await unitOfWork.MaterialRepository.GetAllAsync(limit, offset));
            }

            mappingTasks = materialResult.Materials
                                .Select(async entity => await MapAsync(entity));

            materials = await Task.WhenAll(mappingTasks);

            materials = await UpdateProcessedMLHandlersCount(materials);

            return (materials, materialResult.TotalCount, new Dictionary<Guid, SearchByConfiguredFieldsResultItem>());
        }

        private async Task<Material> MapMaterialDocumentAsync(MaterialDocument p)
        {
            var res = _mapper.Map<Material>(p);
            
            res.Children = p.Children.Select(c => _mapper.Map<Material>(c)).ToList();
            
            var nodes = await Task.WhenAll(p.NodeIds.Select(x => _ontologyService.LoadNodesAsync(x, null)));
            
            res.Events = nodes.Where(x => IsEvent(x)).Select(x => EventToJObject(x));
            res.Features = nodes.Where(x => IsObjectSign(x)).Select(x => NodeToJObject(x));
            res.ObjectsOfStudy = await GetObjectOfStudyListForMaterial(nodes.ToList());
            
            return res;
        }

        public async Task<Material> GetMaterialAsync(Guid id)
        {
            var entity = await RunWithoutCommitAsync(async (unitOfWork) =>
                await unitOfWork.MaterialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures));

            return await MapAsync(entity);
        }

        public Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync()
        {
            return RunWithoutCommitAsync(async (unitOfWork) =>
                await unitOfWork.MaterialRepository.GetAllAsync());
        }

        public IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName)
        {
            return _materialSignRepository.GetAllByTypeName(typeName);
        }

        public MaterialSign GetMaterialSign(Guid id)
        {
            var entity = _materialSignRepository.GetById(id);

            return _mapper.Map<MaterialSign>(entity);
        }

        public MaterialSign GetMaterialSign(string signValue)
        {
            var entity = _materialSignRepository.GetByValue(signValue);

            if (entity is null) return null;

            return _mapper.Map<MaterialSign>(entity);
        }

        public async Task<Material> MapAsync(MaterialEntity material)
        {
            if (material == null) return null;

            var result = _mapper.Map<Material>(material);

            result.Infos.AddRange(await MapInfos(material));

            result.Children.AddRange(await MapChildren(material));

            result.Assignee = _mapper.Map<User>(material.Assignee);

            var nodes = result.Infos
                                .SelectMany(p => p.Features.Select(x => x.Node))
                                .ToList();

            result.Events = nodes.Where(x => IsEvent(x)).Select(x => EventToJObject(x));

            result.Features = nodes.Where(x => IsObjectSign(x)).Select(x => NodeToJObject(x));

            result.ObjectsOfStudy = await GetObjectOfStudyListForMaterial(nodes);

            return result;
        }

        public async Task<List<MLResponse>> GetMLProcessingResultsAsync(Guid materialId)
        {
            var materialIdList = new List<Guid> { materialId };

            var childList = await RunWithoutCommitAsync(uow => uow.MaterialRepository.GetChildIdListForMaterialAsync(materialId));

            materialIdList.AddRange(childList);

            var entities = await _mLResponseRepository.GetAllForMaterialListAsync(materialIdList);

            return _mapper.Map<List<MLResponse>>(entities);
        }

        public async Task<(List<Material> Materials, int Count)> GetMaterialsByAssigneeIdAsync(Guid assigneeId)
        {
            var entities = await RunWithoutCommitAsync(async (unitOfWork) =>
                await unitOfWork.MaterialRepository.GetAllByAssigneeIdAsync(assigneeId));

            var materials = _mapper.Map<List<Material>>(entities);

            return (materials, materials.Count());
        }

        public Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId)
        {
            var nodeIdList = RunWithoutCommit((unitOfWork) =>
                unitOfWork.MaterialRepository.GetFeatureIdListThatRelatesToObjectId(nodeId));

            nodeIdList.Add(nodeId);
            return RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.MaterialRepository.GetParentMaterialByNodeIdQueryAsync(nodeIdList));
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId)
        {
            IEnumerable<Task<Material>> mappingTasks;
            IEnumerable<Material> materials;

            var materialsByNode = GetMaterialByNodeIdQuery(nodeId);
            //TODO: we need to add logic that provides list of NodeId
            //var result = _materialRepository.GetAllForRelatedNodeListAsync(nodeIdList).GetAwaiter().GetResult();

            mappingTasks = materialsByNode
                                 .Select(async e => await MapAsync(await RunWithoutCommitAsync(async (unitOfWork) =>
                                     await unitOfWork.MaterialRepository.GetByIdAsync(e.Id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures))));

            materials = await Task.WhenAll(mappingTasks);

            return (materials, materials.Count());
        }

        private bool IsEvent(Node node)
        {
            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsEvent;
        }

        private bool IsObjectOfStudy(Node node)
        {
            if (node is null) return false;

            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsObjectOfStudy;
        }

        private bool IsObjectSign(Node node)
        {
            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsObjectSign;
        }

        private JObject NodeToJObject(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            foreach (var attribute in node.GetChildAttributes())
            {
                result.Add(new JProperty(attribute.dotName, attribute.attribute.Value));
            }

            return result;
        }

        private JObject EventToJObject(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            var attributies = node.GetChildAttributes().Where(a => a.dotName == "name" || a.dotName == "description");

            foreach (var attribute in attributies)
            {
                result.Add(new JProperty(attribute.dotName, attribute.attribute.Value));
            }

            return result;
        }

        private async Task<IEnumerable<Material>> UpdateProcessedMLHandlersCount(IEnumerable<Material> materials)
        {
            var materialIds = Array.AsReadOnly(materials.Select(p => p.Id).ToArray());

            var mlResults = await _mLResponseRepository.GetAllForMaterialsAsync(materialIds);

            materials.Join(
                mlResults,
                m => m.Id,
                ml => ml.MaterialId,
                (material, result) =>
                {
                    material.ProcessedMlHandlersCount = result.Count;
                    return material;
                }).ToList();

            return materials;
        }

        private List<MaterialEntity> GetMaterialByNodeIdQuery(Guid nodeId)
        {
            var nodeIdList = RunWithoutCommit((unitOfWork) =>
                unitOfWork.MaterialRepository.GetFeatureIdListThatRelatesToObjectId(nodeId));

            nodeIdList.Add(nodeId);

            return RunWithoutCommit((unitOfWork) => unitOfWork.MaterialRepository.GetMaterialByNodeIdQuery(nodeIdList));
        }


        private async Task<JObject> GetObjectOfStudyListForMaterial(List<Node> nodeList)
        {
            var result = new JObject();

            if (!nodeList.Any()) return result;

            var directIdList = nodeList
                                .Where(x => IsObjectOfStudy(x))
                                .Select(x => x.Id)
                                .ToList();

            var featureIdList = nodeList
                                    .Where(x => IsObjectSign(x))
                                    .Select(x => x.Id)
                                    .ToList();

            var featureList = await _ontologyService.GetNodeIdListByFeatureIdListAsync(featureIdList);

            featureList = featureList.Except(directIdList).ToList();

            result.Add(featureList.Select(i => new JProperty(i.ToString("N"), EntityMaterialRelation.Feature)));
            result.Add(directIdList.Select(i => new JProperty(i.ToString("N"), EntityMaterialRelation.Direct)));

            return result;
        }

        private async Task<Material[]> MapChildren(MaterialEntity material)
        {
            var mapChildrenTasks = new List<Task<Material>>();
            foreach (var child in material.Children ?? new List<MaterialEntity>())
            {
                mapChildrenTasks.Add(MapAsync(child));
            }
            return await Task.WhenAll(mapChildrenTasks);
        }

        private async Task<MaterialInfo[]> MapInfos(MaterialEntity material)
        {
            var mapInfoTasks = new List<Task<MaterialInfo>>();
            foreach (var info in material.MaterialInfos ?? new List<MaterialInfoEntity>())
            {
                mapInfoTasks.Add(MapAsync(info));
            }
            return await Task.WhenAll(mapInfoTasks);
        }

        private async Task<MaterialInfo> MapAsync(MaterialInfoEntity info)
        {
            var result = new MaterialInfo(info.Id, JObject.Parse(info.Data), info.Source, info.SourceType, info.SourceVersion);
            foreach (var feature in info.MaterialFeatures)
                result.Features.Add(await MapAsync(feature));
            return result;
        }

        private async Task<MaterialFeature> MapAsync(MaterialFeatureEntity feature)
        {
            var result = new MaterialFeature(feature.Id, feature.Relation, feature.Value);
            result.Node = await _ontologyService.LoadNodesAsync(feature.NodeId, null);
            return result;
        }
    }
}
