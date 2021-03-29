using AutoMapper;
using Iis.Api.Ontology;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;
using Iis.DbLayer.Repositories;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Domain;
using Iis.Domain.MachineLearning;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using Iis.Utility;
using IIS.Repository;
using IIS.Repository.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MaterialSign = Iis.Domain.Materials.MaterialSign;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider<TUnitOfWork> : BaseService<TUnitOfWork>, IMaterialProvider where TUnitOfWork : IIISUnitOfWork
    {
        private const string WildCart = "*";
        private static readonly JsonSerializerSettings _materialDocSerializeSettings = new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None
        };
        private static readonly IEnumerable<Material> EmptyMaterialCollection = Array.Empty<Material>();
        private static readonly IReadOnlyCollection<string> RelationTypeNameList = new List<string>
        {
            "parent", "bePartOf"
        };
        private readonly IOntologyService _ontologyService;
        private readonly IOntologySchema _ontologySchema;
        private readonly IOntologyNodesData _ontologyData;
        private readonly IMaterialElasticService _materialElasticService;
        private readonly IMapper _mapper;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IMaterialSignRepository _materialSignRepository;
        private readonly string _imageVectorizerUrl;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly NodeToJObjectMapper _nodeToJObjectMapper;

        public MaterialProvider(IOntologyService ontologyService,
            IOntologySchema ontologySchema,
            IOntologyNodesData ontologyData,
            IMaterialElasticService materialElasticService,
            IMLResponseRepository mLResponseRepository,
            IMaterialSignRepository materialSignRepository,
            IMapper mapper,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            NodeToJObjectMapper nodeToJObjectMapper) : base(unitOfWorkFactory)
        {
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _ontologyData = ontologyData;
            _materialElasticService = materialElasticService;
            _mLResponseRepository = mLResponseRepository;
            _materialSignRepository = materialSignRepository;
            _mapper = mapper;
            _imageVectorizerUrl = configuration.GetValue<string>("imageVectorizerUrl");
            _httpClientFactory = httpClientFactory;
            _nodeToJObjectMapper = nodeToJObjectMapper;
        }

        public async Task<MaterialsDto> GetMaterialsAsync(
            Guid userId,
            string filterQuery,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default)
        {
            var searchParams = new SearchParams
            {
                Suggestion = string.IsNullOrWhiteSpace(filterQuery) || filterQuery == WildCart ? null : filterQuery,
                Page = page,
                Sorting = sorting
            };

            var searchResult = await _materialElasticService.SearchMaterialsByConfiguredFieldsAsync(userId, searchParams);

            var materials = searchResult.Items.Values
                .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                .Select(MapMaterialDocument)
                .ToArray();

            return MaterialsDto.Create(materials, searchResult.Count, searchResult.Items, searchResult.Aggregations);
        }

        private Material MapMaterialDocument(MaterialDocument document)
        {
            var material = _mapper.Map<Material>(document);

            material.Children = document.Children
                                            .Select(_mapper.Map<Material>)
                                            .ToList();

            var nodes = document.NodeIds
                                    .Select(_ontologyService.GetNode)
                                    .ToArray();

            material.Events = nodes
                                .Where(IsEvent)
                                .Select(_nodeToJObjectMapper.EventToJObject);

            material.Features = nodes
                                .Where(IsObjectSign)
                                .Select(_nodeToJObjectMapper.NodeToJObject);

            material.ObjectsOfStudy = GetObjectOfStudyListForMaterial(nodes);

            return material;
        }

        public async Task<Material> GetMaterialAsync(Guid id, User user)
        {
            var entity = await RunWithoutCommitAsync(async (unitOfWork) =>
                await unitOfWork.MaterialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures));

            if (entity is null || !entity.CanBeAccessedBy(user.AccessLevel))
            {
                throw new ArgumentException($"Cannot find material with id {id}");
            }

            var mapped = Map(entity);
            mapped.CanBeEdited = entity.CanBeEdited(user.Id);
            return mapped;
        }

        public async Task<Material[]> GetMaterialsByIdsAsync(ISet<Guid> ids, User user)
        {
            var entities = await RunWithoutCommitAsync(async (unitOfWork) =>
                await unitOfWork.MaterialRepository.GetByIdsAsync(ids, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures));

            return entities.Where(p => p.CanBeAccessedBy(user.AccessLevel))
                .Select(entity => {
                    var mapped = Map(entity);
                    mapped.CanBeEdited = entity.CanBeEdited(user.Id);
                    return mapped;
                })
                .ToArray();         
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

        private Material Map(MaterialEntity material)
        {
            if (material == null) return null;

            var result = _mapper.Map<Material>(material);

            result.Infos.AddRange(MapInfos(material));

            result.Children.AddRange(MapChildren(material));
                        result.Assignee = _mapper.Map<User>(material.Assignee);

            var nodes = result.Infos
                                .SelectMany(p => p.Features.Select(x => x.Node))
                                .ToList();

            result.Events = nodes
                                .Where(IsEvent)
                                .Select(_nodeToJObjectMapper.EventToJObject);

            result.Features = nodes
                                .Where(IsObjectSign)
                                .Select(_nodeToJObjectMapper.NodeToJObject);

            result.ObjectsOfStudy = GetObjectOfStudyListForMaterial(nodes);

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
                                 .Select(async e => Map(await RunWithoutCommitAsync((unitOfWork) =>
                                     unitOfWork.MaterialRepository.GetByIdAsync(e.Id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures))));

            materials = await Task.WhenAll(mappingTasks);

            return (materials, materials.Count());
        }

        public async Task<Dictionary<Guid, int>> CountMaterialsByNodeIds(HashSet<Guid> nodeIds)
        {
            var nodeFeatureRelationsList = await RunWithoutCommitAsync(async (unitOfWork) =>
                await unitOfWork.MaterialRepository.GetFeatureIdListThatRelatesToObjectIdsAsync(nodeIds));

            var nodeIdsForCountQuery = new List<Guid>(nodeIds);
            nodeIdsForCountQuery.AddRange(nodeFeatureRelationsList.Select(p => p.FeatureId));

            var nodesWithMaterials = await RunWithoutCommitAsync(
                    async (unitOfWork) =>
                    await unitOfWork.MaterialRepository.GetNodeIsWithMaterials(nodeIdsForCountQuery));

            var parentNodesMap = PrepareNodesMap(nodeIds, nodeFeatureRelationsList);

            return PrepareResult(nodesWithMaterials, parentNodesMap);
        }

        private static Dictionary<Guid, int> PrepareResult(List<Guid> nodesWithMaterials, Dictionary<Guid, List<Guid>> parentNodesMap)
        {
            var res = new Dictionary<Guid, int>();

            foreach (var nodeWithMaterial in nodesWithMaterials)
            {
                var nodesToIncrement = parentNodesMap[nodeWithMaterial];
                foreach (var nodeToIncrement in nodesToIncrement)
                {
                    if (res.ContainsKey(nodeToIncrement))
                    {
                        res[nodeToIncrement]++;
                    }
                    else
                    {
                        res.Add(nodeToIncrement, 1);
                    }
                }
            }

            return res;
        }

        private static Dictionary<Guid, List<Guid>> PrepareNodesMap(HashSet<Guid> nodeIds, List<ObjectFeatureRelation> nodeFeatureRelationsList)
        {
            var parentNodesMap = nodeIds.ToDictionary(k => k, v => new List<Guid> { v });

            foreach (var relation in nodeFeatureRelationsList)
            {
                if (parentNodesMap.ContainsKey(relation.FeatureId))
                {
                    parentNodesMap[relation.FeatureId].Add(relation.ObjectId);
                }
                else
                {
                    parentNodesMap.Add(relation.FeatureId, new List<Guid> { relation.ObjectId });
                }
            }

            return parentNodesMap;
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsLikeThisAsync(
            Guid userId,
            Guid materialId, 
            PaginationParams page,
            SortingParams sorting)
        {
            var isEligible = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.MaterialRepository.CheckMaterialExistsAndHasContent(materialId));

            if (!isEligible) return (EmptyMaterialCollection, 0);

            var searchParams = new SearchParams
            {
                Suggestion = materialId.ToString("N"),
                Page = page,
                Sorting = sorting
            };

            var searchResult = await _materialElasticService.SearchMoreLikeThisAsync(userId, searchParams);

            var materials = searchResult.Items.Values
                    .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                    .Select(MapMaterialDocument);

            return (materials, searchResult.Count);
        }

        public async Task<MaterialsDto> GetMaterialsByImageAsync(Guid userId, PaginationParams page, string fileName, byte[] content)
        {
            decimal[] imageVector;
            try
            {
                imageVector = await VectorizeImage(content, fileName);

                if (imageVector is null) throw new Exception("Image vector is empty.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to vectorize image", e);
            }
            var searchResult = await _materialElasticService.SearchByImageVector(userId, imageVector, page);

            var materials = searchResult.Items.Values
                    .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                    .Select(MapMaterialDocument)
                    .ToList();

            return MaterialsDto.Create(materials, searchResult.Count, searchResult.Items, searchResult.Aggregations);
        }

        public async Task<decimal[]> VectorizeImage(byte[] fileContent, string fileName)
        {
            using var form = new MultipartFormDataContent();
            using var content = new ByteArrayContent(fileContent);

            form.Add(content, "file", fileName);
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.PostAsync(_imageVectorizerUrl, form);
            response.EnsureSuccessStatusCode();
            var contentJson = await response.Content.ReadAsStringAsync();
            return FaceAPIResponseParser.GetEncoding(contentJson);
        }

        public async Task<MaterialsDto> GetMaterialsCommonForEntitiesAsync(Guid userId,
            IEnumerable<Guid> nodeIdList,
            bool includeDescendants,
            string suggestion,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default)
        {
            var materialEntityList = new List<MaterialEntity>();

            foreach (var nodeId in nodeIdList)
            {
                var objectIdList = new List<Guid> { nodeId };

                if (includeDescendants) objectIdList.AddRange(GetDescendantsByGivenRelationTypeNameList(objectIdList, RelationTypeNameList));

                var materialEntities = await RunWithoutCommitAsync((unitOfWork) => unitOfWork.MaterialRepository.GetMaterialByNodeIdQueryAsync(objectIdList));

                materialEntities = materialEntities.GroupBy(e => e.Id).Select(gr => gr.FirstOrDefault()).ToList();

                materialEntityList.AddRange(materialEntities);
            }

            if (!materialEntityList.Any()) return MaterialsDto.Empty;

            var materialEntitiesIdList = materialEntityList
                .GroupBy(e => e.Id)
                .Where(gr => gr.Count() == nodeIdList.Count())
                .Select(gr => gr.Select(e => e.Id).FirstOrDefault())
                .ToArray();

            if (!materialEntitiesIdList.Any()) return MaterialsDto.Empty;

            var searchParams = new SearchParams { Suggestion = suggestion, Page = page, Sorting = sorting };

            var searchResult = await _materialElasticService.SearchMaterialsAsync(userId, searchParams, materialEntitiesIdList, ct);

            var materials = searchResult.Items.Values
                .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                .Select(MapMaterialDocument)
                .ToList();

            return MaterialsDto.Create(materials, searchResult.Count, searchResult.Items, searchResult.Aggregations);
        }

        private IReadOnlyCollection<Guid> GetDescendantsByGivenRelationTypeNameList(IReadOnlyCollection<Guid> entityIdList, IReadOnlyCollection<string> relationTypeNameList)
        {
            var result = new List<Guid>();

            var tempValues = entityIdList.ToList();

            while (tempValues.Any())
            {
                var relationList = _ontologyData.GetIncomingRelations(tempValues, relationTypeNameList);

                tempValues = relationList
                            .Select(e => e.SourceNodeId)
                            .ToList();

                result.AddRange(tempValues);
            }
            return result.AsReadOnly();
        }
        private bool IsEvent(Node node)
        {
            if (node is null) return false;

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
            if (node is null) return false;

            var nodeType = _ontologySchema.GetNodeTypeById(node.Type.Id);

            return nodeType.IsObjectSign;
        }

        private async Task<IReadOnlyCollection<Material>> UpdateProcessedMLHandlersCountAsync(IReadOnlyCollection<Material> materials)
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
                }).ToArray();

            return materials;
        }

        private List<MaterialEntity> GetMaterialByNodeIdQuery(Guid nodeId)
        {
            var nodeIdList = RunWithoutCommit((unitOfWork) =>
                unitOfWork.MaterialRepository.GetFeatureIdListThatRelatesToObjectId(nodeId));

            nodeIdList.Add(nodeId);

            return RunWithoutCommit((unitOfWork) => unitOfWork.MaterialRepository.GetMaterialByNodeIdQuery(nodeIdList));
        }

        private JObject GetObjectOfStudyListForMaterial(IEnumerable<Node> nodeList)
        {
            var result = new JObject();

            if (!nodeList.Any()) return result;

            var directIdList = nodeList
                                .Where(x => IsObjectOfStudy(x))
                                .Select(x => x.Id)
                                .ToArray();

            var featureIdList = nodeList
                                    .Where(x => IsObjectSign(x))
                                    .Select(x => x.Id)
                                    .ToArray();

            var featureList = _ontologyService.GetNodeIdListByFeatureIdList(featureIdList);

            featureList = featureList.Except(directIdList).ToArray();

            result.Add(featureList.Select(i => new JProperty(i.ToString("N"), EntityMaterialRelation.Feature)));
            result.Add(directIdList.Select(i => new JProperty(i.ToString("N"), EntityMaterialRelation.Direct)));

            return result;
        }

        private IReadOnlyCollection<Material> MapChildren(MaterialEntity material)
        {
            if (material.Children == null)
            {
                return Array.Empty<Material>();
            }
            return material.Children.Select(child => Map(child)).ToArray();
        }

        private IReadOnlyCollection<MaterialInfo> MapInfos(MaterialEntity material)
        {
            var mapInfoTasks = new List<MaterialInfo>();
            foreach (var info in material.MaterialInfos ?? new List<MaterialInfoEntity>())
            {
                mapInfoTasks.Add(Map(info));
            }
            return mapInfoTasks;
        }

        private MaterialInfo Map(MaterialInfoEntity info)
        {
            var result = new MaterialInfo(info.Id, JObject.Parse(info.Data), info.Source, info.SourceType, info.SourceVersion);
            foreach (var feature in info.MaterialFeatures)
                result.Features.Add(Map(feature));
            return result;
        }

        private MaterialFeature Map(MaterialFeatureEntity feature)
        {
            var result = new MaterialFeature(feature.Id, feature.Relation, feature.Value);
            result.Node = _ontologyService.GetNode(feature.NodeId);
            return result;
        }

        public async Task<bool> MaterialExists(Guid id)
        {
            var entity = await RunWithoutCommitAsync((unitOfWork) =>
                unitOfWork.MaterialRepository.GetByIdAsync(id));

            return entity != null;
        }
    }
}
