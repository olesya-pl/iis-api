using AutoMapper;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.MachineLearning;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using Iis.Utility;
using IIS.Repository;
using IIS.Repository.Factories;
using IIS.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Materials;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MaterialSign = Iis.Domain.Materials.MaterialSign;

namespace IIS.Services.Materials
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
        private readonly IImageVectorizer _imageVectorizer;
        private readonly NodeToJObjectMapper _nodeToJObjectMapper;

        public MaterialProvider(IOntologyService ontologyService,
            IOntologySchema ontologySchema,
            IOntologyNodesData ontologyData,
            IMaterialElasticService materialElasticService,
            IMLResponseRepository mLResponseRepository,
            IMaterialSignRepository materialSignRepository,
            IMapper mapper,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IImageVectorizer imageVectorizer,
            NodeToJObjectMapper nodeToJObjectMapper) : base(unitOfWorkFactory)
        {
            _ontologyService = ontologyService;
            _ontologySchema = ontologySchema;
            _ontologyData = ontologyData;
            _materialElasticService = materialElasticService;
            _mLResponseRepository = mLResponseRepository;
            _materialSignRepository = materialSignRepository;
            _mapper = mapper;
            _imageVectorizer = imageVectorizer;
            _nodeToJObjectMapper = nodeToJObjectMapper;
        }

        public async Task<MaterialsDto> GetMaterialsAsync(
            Guid userId,
            string filterQuery,
            IReadOnlyCollection<Property> filteredItems,
            IReadOnlyCollection<string> cherryPickedItems,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default)
        {
            if (_materialElasticService.ShouldReturnNoEntities(filterQuery)) return MaterialsDto.Empty;

            var searchParams = new SearchParams
            {
                Suggestion = string.IsNullOrWhiteSpace(filterQuery) || filterQuery == WildCart ? null : filterQuery,
                FilteredItems = filteredItems,
                CherryPickedItems = cherryPickedItems.Select(p => new CherryPickedItem(p)).ToList(),
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
            var entity = await RunWithoutCommitAsync(uow => uow.MaterialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures));

            if (entity is null || !entity.CanBeAccessedBy(user.AccessLevel))
            {
                throw new ArgumentException($"{FrontEndErrorCodes.NotFound}:Матеріал не знайдено");
            }

            var mapped = Map(entity);

            mapped.CanBeEdited = entity.CanBeEdited(user.Id);

            return mapped;
        }

        public async Task<Material> GetMaterialAsync(Guid id)
        {
            var entity = await RunWithoutCommitAsync(uow => uow.MaterialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures));

            if (entity is null)
            {
                throw new ArgumentException($"{FrontEndErrorCodes.NotFound}:Матеріал не знайдено");
            }
            return Map(entity);
        }

        public async Task<Material[]> GetMaterialsByIdsAsync(ISet<Guid> ids, User user)
        {
            var entities = await RunWithoutCommitAsync(uow =>
                uow.MaterialRepository.GetByIdsAsync(ids, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures));

            return entities.Where(p => p.CanBeAccessedBy(user.AccessLevel))
                .Select(entity =>
                {
                    var mapped = Map(entity);
                    mapped.CanBeEdited = entity.CanBeEdited(user.Id);
                    return mapped;
                })
                .ToArray();
        }

        public Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync()
        {
            return RunWithoutCommitAsync(uow => uow.MaterialRepository.GetAllAsync());
        }

        public async Task<IReadOnlyCollection<Guid>> GetMaterialsIdsAsync(int limit)
        {
            var materials = await RunWithoutCommitAsync(uow =>
                uow.MaterialRepository.GetAllAsync(limit, MaterialIncludeEnum.OnlyParent));

            var materialIds = materials.Select((material) => material.Id).ToArray();

            return materialIds;
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

            result.Editor = _mapper.Map<User>(material.Editor);

            var nodes = result.Infos
                                .SelectMany(p => p.Features.Where(e => e.NodeLinkType == MaterialNodeLinkType.None).Select(e => e.Node))
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
            var entities = await RunWithoutCommitAsync(uow =>
                uow.MaterialRepository.GetAllByAssigneeIdAsync(assigneeId));

            var materials = _mapper.Map<List<Material>>(entities);

            return (materials, materials.Count());
        }

        public Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId)
        {
            var nodeIdList = new List<Guid> { nodeId };

            var featureIdCollection = _ontologyService.GetObjectFeatureRelationCollection(nodeIdList)
                                        .Select(e => e.FeatureId)
                                        .ToArray();

            nodeIdList.AddRange(featureIdCollection);

            return RunWithoutCommitAsync(uow => uow.MaterialRepository.GetParentMaterialByNodeIdQueryAsync(nodeIdList));
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeId(Guid nodeId)
        {
            var materialsByNode = await GetMaterialCollectionByNodeIdAsync(nodeId, false);
            var materials = materialsByNode.Select(p => Map(p));
            return (materials, materials.Count());
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdAndRelatedEntities(Guid nodeId)
        {
            var materialsByNode = await GetMaterialCollectionByNodeIdAsync(nodeId, true);
            var materials = materialsByNode.Select(p => Map(p));
            return (materials, materials.Count());
        }

        public async Task<Dictionary<Guid, int>> CountMaterialsByNodeIds(HashSet<Guid> nodeIds)
        {
            var nodeFeatureRelationsList = _ontologyService.GetObjectFeatureRelationCollection(nodeIds);

            var nodeIdsForCountQuery = new List<Guid>(nodeIds);
            nodeIdsForCountQuery.AddRange(nodeFeatureRelationsList.Select(p => p.FeatureId));

            var nodesWithMaterials = await RunWithoutCommitAsync(
                    uow => uow.MaterialRepository.GetNodeIsWithMaterialsAsync(nodeIdsForCountQuery));

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

        private static Dictionary<Guid, List<Guid>> PrepareNodesMap(HashSet<Guid> nodeIds, IReadOnlyCollection<ObjectFeatureRelation> nodeFeatureRelationsList)
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
            var isEligible = await RunWithoutCommitAsync(uow => uow.MaterialRepository.CheckMaterialExistsAndHasContent(materialId));

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
            IReadOnlyCollection<decimal[]> imageVectorList;
            try
            {
                imageVectorList = await _imageVectorizer.VectorizeImage(content, fileName);

                if (!imageVectorList.Any()) throw new Exception("No image vectors have found.");
            }
            catch (Exception e)
            {
                throw new Exception("Failed to vectorize image", e);
            }
            var searchResult = await _materialElasticService.SearchByImageVector(userId, imageVectorList, page);

            var materials = searchResult.Items.Values
                    .Select(p => JsonConvert.DeserializeObject<MaterialDocument>(p.SearchResult.ToString(), _materialDocSerializeSettings))
                    .Select(MapMaterialDocument)
                    .ToList();

            return MaterialsDto.Create(materials, searchResult.Count, searchResult.Items, searchResult.Aggregations);
        }

        public async Task<MaterialsDto> GetMaterialsCommonForEntitiesAsync(Guid userId,
            IEnumerable<Guid> nodeIdList,
            bool includeDescendants,
            string suggestion,
            PaginationParams page,
            SortingParams sorting,
            CancellationToken ct = default)
        {
            var materialEntityIdCollection = new List<Guid>();

            foreach (var nodeId in nodeIdList)
            {
                var objectIdList = new List<Guid> { nodeId };

                if (includeDescendants) objectIdList.AddRange(GetDescendantsByGivenRelationTypeNameList(objectIdList, RelationTypeNameList));

                var queryResult = await RunWithoutCommitAsync((unitOfWork) => unitOfWork.MaterialRepository.GetMaterialIdCollectionByNodeIdCollectionAsync(objectIdList));

                queryResult = queryResult.Distinct().ToArray();

                materialEntityIdCollection.AddRange(queryResult);
            }

            if (!materialEntityIdCollection.Any()) return MaterialsDto.Empty;

            var materialEntitiesIdList = materialEntityIdCollection
                .GroupBy(e => e)
                .Where(gr => gr.Count() == nodeIdList.Count())
                .Select(gr => gr.FirstOrDefault())
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

        private Task<IReadOnlyCollection<MaterialEntity>> GetMaterialCollectionByNodeIdAsync(Guid nodeId, bool includeRelatedEntities)
        {
            var nodeIdList = new List<Guid> { nodeId };

            if (includeRelatedEntities)
            {
                var featureIdCollection = _ontologyService.GetObjectFeatureRelationCollection(nodeIdList)
                                            .Select(e => e.FeatureId)
                                            .ToArray();

                nodeIdList.AddRange(featureIdCollection);
            }

            return RunWithoutCommitAsync(uow => uow.MaterialRepository.GetMaterialCollectionByNodeIdAsync(nodeIdList, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures, MaterialIncludeEnum.WithFiles));
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
            var result = _mapper.Map<MaterialFeature>(feature);
            result.Node = _ontologyService.GetNode(feature.NodeId);
            return result;
        }

        public async Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoriesAsync(Guid materialId)
        {
            var entity = await RunWithoutCommitAsync(uow => uow.MaterialRepository.GetByIdAsync(materialId, MaterialIncludeEnum.WithFeatures));

            var infoList = MapInfos(entity);

            var nodes = infoList
                            .SelectMany(p => p.Features.Select(x => x.Node))
                            .ToArray();

            var featureIdList = nodes
                                .Where(IsObjectSign)
                                .Select(e => e.Id)
                                .ToArray();

            var result = new List<LocationHistoryDto>(featureIdList.Length + 1);

            var featureLocationListTasks = featureIdList
                .Select(id => RunWithoutCommitAsync(uow => uow.LocationHistoryRepository.GetLatestLocationHistoryEntityAsync(id)));

            var locationEntityList = await Task.WhenAll(featureLocationListTasks);

            var dtoList = locationEntityList.Select(e => _mapper.Map<LocationHistoryDto>(e));

            result.AddRange(dtoList);

            locationEntityList = await RunWithoutCommitAsync(uow => uow.LocationHistoryRepository.GetLocationHistoryEntityListByMaterialIdAsync(entity.Id));

            dtoList = locationEntityList.Select(e => _mapper.Map<LocationHistoryDto>(e));

            result.AddRange(dtoList);

            return result.Where(x => x != null).ToList();
        }

        public async Task<bool> MaterialExists(Guid id)
        {
            var entity = await RunWithoutCommitAsync((unitOfWork) =>
                unitOfWork.MaterialRepository.GetByIdAsync(id));

            return entity != null;
        }

        public Task<IReadOnlyCollection<Guid>> GetAllUnassignedIdsAsync(PaginationParams page, SortingParams sorting = default, CancellationToken cancellationToken = default)
        {
            var (offset, limit) = page.ToEFPage();

            return RunWithoutCommitAsync((unitOfWork) =>
                   unitOfWork.MaterialRepository.GetAllUnassignedIdsAsync(limit, offset, sorting?.ColumnName, sorting?.Order, cancellationToken));
        }

        public Task<IReadOnlyList<string>> GetCellSatChannelsAsync() =>
            RunWithoutCommitAsync((unitOfWork) =>
                   unitOfWork.MaterialRepository.GetCellSatChannelsAsync());
        public Task<IReadOnlyList<MaterialEntity>> GetCellSatWithChannelAsync(int limit, string channel) =>
            RunWithoutCommitAsync((unitOfWork) =>
                   unitOfWork.MaterialRepository.GetCellSatWithChannelAsync(limit, channel));

        public Task<IReadOnlyList<MaterialEntity>> GetCellSatWithoutChannelAsync(int limit) =>
            RunWithoutCommitAsync((unitOfWork) =>
                   unitOfWork.MaterialRepository.GetCellSatWithoutChannelAsync(limit));

        public Task<IReadOnlyList<MaterialEntity>> GetNotCellSatAsync(int limit) =>
            RunWithoutCommitAsync((unitOfWork) =>
                   unitOfWork.MaterialRepository.GetNotCellSatAsync(limit));

        public Task<IReadOnlyList<MaterialChannelMappingEntity>> GetChannelMappingsAsync() =>
            RunWithoutCommitAsync((unitOfWork) =>
                   unitOfWork.MaterialRepository.GetChannelMappingsAsync());
    }
}