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
        private static readonly IReadOnlyCollection<string> RelationTypeNameList = new List<string>
        {
            "parent"
        };
        private static readonly (IEnumerable<Material> Materials, int Count) EmptMaterialResult = (Materials: Array.Empty<Material>(), 0);
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
            IElasticService elasticService,
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

        public async Task<(IEnumerable<Material> Materials, int Count, Dictionary<Guid, SearchResultItem> Highlights)>
            GetMaterialsAsync(int limit, int offset, string filterQuery,
            IEnumerable<string> types = null, string sortColumnName = null, string sortOrder = null)
        {
            IEnumerable<Task<Material>> mappingTasks;
            IEnumerable<Material> materials;
            (IEnumerable<MaterialEntity> Materials, int TotalCount) materialResult;

            if (!string.IsNullOrWhiteSpace(filterQuery) && filterQuery != WildCart)
            {
                var searchResult = await _materialElasticService.SearchMaterialsByConfiguredFieldsAsync(
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
                    await unitOfWork.MaterialRepository.GetAllAsync(types, limit, offset, sortColumnName, sortOrder));
            }
            else
            {
                materialResult = await RunWithoutCommitAsync(async (unitOfWork) =>
                    await unitOfWork.MaterialRepository.GetAllAsync(limit, offset, sortColumnName, sortOrder));
            }

            mappingTasks = materialResult.Materials
                                .Select(async entity => await MapAsync(entity));

            materials = await Task.WhenAll(mappingTasks);

            materials = await UpdateProcessedMLHandlersCount(materials);

            return (materials, materialResult.TotalCount, new Dictionary<Guid, SearchResultItem>());
        }

        private async Task<Material> MapMaterialDocumentAsync(MaterialDocument p)
        {
            var res = _mapper.Map<Material>(p);

            res.Children = p.Children.Select(c => _mapper.Map<Material>(c)).ToList();

            var nodes = p.NodeIds.Select(x => _ontologyService.LoadNodes(x));

            res.Events = nodes.Where(x => IsEvent(x)).Select(x => _nodeToJObjectMapper.EventToJObject(x));
            res.Features = nodes.Where(x => IsObjectSign(x)).Select(x => _nodeToJObjectMapper.NodeToJObject(x));
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

            result.Infos.AddRange(MapInfos(material));

            result.Children.AddRange(await MapChildren(material));

            result.Assignee = _mapper.Map<User>(material.Assignee);

            var nodes = result.Infos
                                .SelectMany(p => p.Features.Select(x => x.Node))
                                .ToList();

            result.Events = nodes
                .Where(x => IsEvent(x))
                .Select(x => _nodeToJObjectMapper.EventToJObject(x))
                .ToList();

            result.Features = nodes
                .Where(x => IsObjectSign(x))
                .Select(x => _nodeToJObjectMapper.NodeToJObject(x))
                .ToList();

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

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsLikeThisAsync(Guid materialId, int limit, int offset)
        {
            var isEligible = await RunWithoutCommitAsync(async (unitOfWork) => await unitOfWork.MaterialRepository.CheckMaterialExistsAndHasContent(materialId));

            if(!isEligible) return (new List<Material>(), 0);

            var searchParams = new SearchParams
            {
                Offset = offset,
                Limit = limit,
                Suggestion = materialId.ToString("N")
            };

            var searchResult = await _materialElasticService.SearchMoreLikeThisAsync(searchParams);

            var materialTasks = searchResult.Items.Values
                    .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                    .Select(p => MapMaterialDocumentAsync(p));

            var materials = await Task.WhenAll(materialTasks);

            return (materials, searchResult.Count);
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByImageAsync(int pageSize, int offset, string fileName, byte[] content)
        {
            decimal[] imageVector;
            try
            {
                imageVector = await VectorizeImage(content, fileName);

                if(imageVector is null) throw new Exception("Image vector is empty.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to vectorize image", e);
            }
            var searchResult = await _materialElasticService.SearchByImageVector(imageVector, offset, pageSize, CancellationToken.None);

            var materialTasks = searchResult.Items.Values
                    .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                    .Select(p => MapMaterialDocumentAsync(p));

            var materials = await Task.WhenAll(materialTasks);

            return (materials, searchResult.Count);
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

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsCommonForEntitiesAsync(IEnumerable<Guid> nodeIdList, bool includeDescendants, string suggestion, int limit = 0, int offset = 0, CancellationToken ct = default)
        {
            var materialEntityList = new List<MaterialEntity>();

            foreach (var nodeId in nodeIdList)
            {
                var objectIdList = new List<Guid>{ nodeId };

                if(includeDescendants) objectIdList.AddRange(GetDescendantsByGivenRelationTypeNameList(objectIdList, RelationTypeNameList));

                var materialEntities = await RunWithoutCommitAsync((unitOfWork) => unitOfWork.MaterialRepository.GetMaterialByNodeIdQueryAsync(objectIdList));

                materialEntities = materialEntities.GroupBy(e => e.Id).Select(gr => gr.FirstOrDefault()).ToList();

                materialEntityList.AddRange(materialEntities);
            }

            if(!materialEntityList.Any()) return EmptMaterialResult;

            var materialEntitiesIdList = materialEntityList
                .GroupBy(e => e.Id)
                .Where(gr => gr.Count() == nodeIdList.Count())
                .Select(gr => gr.Select(e => e.Id).FirstOrDefault());

            var searchParams = new SearchParams{Offset = offset, Limit = limit, Suggestion = suggestion};

            var searchResult = await _materialElasticService.SearchMaterialsAsync(searchParams, materialEntitiesIdList);

            var materialTasks = searchResult.Items.Values
                .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                .Select(p => MapMaterialDocumentAsync(p));

            var materials = await Task.WhenAll(materialTasks);

            return (materials, searchResult.Count);
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
            if(node is null) return false;

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

            var featureList = _ontologyService.GetNodeIdListByFeatureIdListAsync(featureIdList);

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
            result.Node = _ontologyService.LoadNodes(feature.NodeId);
            return result;
        }
    }
}
