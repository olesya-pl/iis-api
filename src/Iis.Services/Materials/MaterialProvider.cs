using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;
using Iis.DbLayer.Repositories;
using Iis.Domain;
using Iis.Domain.MachineLearning;
using Iis.Domain.Materials;
using Iis.Domain.Users;
using Iis.Elastic.Entities;
using Iis.Interfaces.Common;
using Iis.Interfaces.Constants;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Materials.Distribution;
using IIS.Repository;
using IIS.Repository.Factories;
using IIS.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Materials;
using Iis.RabbitMq.Channels;
using MaterialSign = Iis.Domain.Materials.MaterialSign;
using NextAssignedMessage = Iis.Messages.Materials.MaterialNextAssignedMessage;
using Iis.Interfaces.SecurityLevels;

namespace IIS.Services.Materials
{
    public class MaterialProvider<TUnitOfWork> : BaseService<TUnitOfWork>, IMaterialProvider where TUnitOfWork : IIISUnitOfWork
    {
        private const string WildCart = "*";
        private static readonly IReadOnlyCollection<Material> EmptyMaterialCollection = Array.Empty<Material>();
        private static readonly OutputCollection<Material> EmptyMaterialCollectionInstance = new OutputCollection<Material>(Array.Empty<Material>());
        private static readonly IReadOnlyCollection<string> RelationTypeNameList = new List<string>
        {
            "parent", "bePartOf"
        };
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyNodesData _ontologyData;
        private readonly IMaterialElasticService _materialElasticService;
        private readonly IMapper _mapper;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IMaterialSignRepository _materialSignRepository;
        private readonly IImageVectorizer _imageVectorizer;
        private readonly MaterialDocumentMapper _materialDocumentMapper;
        private readonly IConnection _connection;
        private readonly MaterialNextAssignedPublisherConfig _nextAssignedConfig;
        private readonly ILogger<IMaterialProvider> _logger;
        private readonly ISecurityLevelChecker _securityLevelChecker;

        public MaterialProvider(
            IOntologyService ontologyService,
            IOntologyNodesData ontologyData,
            IMaterialElasticService materialElasticService,
            IMLResponseRepository mLResponseRepository,
            IMaterialSignRepository materialSignRepository,
            IMapper mapper,
            IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory,
            IImageVectorizer imageVectorizer,
            MaterialDocumentMapper materialDocumentMapper,
            IConnection connection,
            ISecurityLevelChecker securityLevelChecker,
            IOptions<MaterialNextAssignedPublisherConfig> nextAssignedConfigOption,
            ILogger<IMaterialProvider> logger) : base(unitOfWorkFactory)
        {
            _ontologyService = ontologyService;
            _ontologyData = ontologyData;
            _materialElasticService = materialElasticService;
            _mLResponseRepository = mLResponseRepository;
            _materialSignRepository = materialSignRepository;
            _mapper = mapper;
            _imageVectorizer = imageVectorizer;
            _materialDocumentMapper = materialDocumentMapper;
            _connection = connection;
            _nextAssignedConfig = nextAssignedConfigOption.Value;
            _logger = logger;
            _securityLevelChecker = securityLevelChecker;
        }

        public async Task<MaterialsDto> GetMaterialsAsync(
            Guid userId,
            string filterQuery,
            RelationsState? materialRelationsState,
            IReadOnlyCollection<Property> filteredItems,
            IReadOnlyCollection<string> cherryPickedItems,
            DateRange createdDateRange,
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
                Sorting = sorting,
                CreatedDateRange = new DateRange(createdDateRange.From, createdDateRange.To)
            };

            var searchResult = await _materialElasticService.SearchMaterialsByConfiguredFieldsAsync(userId, searchParams, materialRelationsState, ct);

            var materials = searchResult.Items.Values
                .Select(p => MaterialDocument.FromJObject(p.SearchResult))
                .Select(_materialDocumentMapper.Map)
                .ToArray();

            return MaterialsDto.Create(materials, searchResult.Count, searchResult.Items, searchResult.Aggregations);
        }

        public async Task<Material> GetMaterialAsync(Guid id)
        {
            var entity = await RunWithoutCommitAsync(uow => uow.MaterialRepository.GetByIdAsync(id, MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures));

            if (entity is null)
            {
                throw new ArgumentException($"{FrontEndErrorCodes.NotFound}:Матеріал не знайдено");
            }
            return _materialDocumentMapper.Map(entity);
        }

        public async Task<Material> GetMaterialAsync(Guid id, User user)
        {
            var entity = await _materialElasticService.GetMaterialById(user.Id, id);
            if (entity is null)
            {
                throw new ArgumentException($"{FrontEndErrorCodes.NotFound}:Матеріал не знайдено");
            }
            return _materialDocumentMapper.Map(entity, user);
        }

        public async Task<Material[]> GetMaterialsByIdsAsync(ISet<Guid> ids, User user)
        {
            var documentCollection = await _materialElasticService.GetMaterialCollectionByIdCollectionAsync(ids.ToArray(), user.Id, CancellationToken.None);

            if (!documentCollection.Any()) return EmptyMaterialCollection.ToArray();

            return documentCollection
                    .Select(_ => _materialDocumentMapper.Map(_))
                    .Select(material =>
                    {
                        var materialSecurityLevelIndexes = material.SecurityLevels.Select(_ => _.UniqueIndex).ToList();
                        var isAllowed = _securityLevelChecker.AccessGranted(user.SecurityLevelsIndexes,
                            materialSecurityLevelIndexes);

                        if (isAllowed) material.AccessAllowed = true;
                        
                        CheckIsAllowedRelatedEntityCollectionsForUser(material.RelatedEventCollection, user);
                        CheckIsAllowedRelatedEntityCollectionsForUser(material.RelatedObjectCollection, user);
                        CheckIsAllowedRelatedEntityCollectionsForUser(material.RelatedSignCollection, user);

                        return material;
                    })
                    .ToArray();
        }
        
        private void CheckIsAllowedRelatedEntityCollectionsForUser(IEnumerable<Iis.Domain.Materials.RelatedObject> relatedEntityCollection, User user)
        {
            foreach (var entity in relatedEntityCollection)
            {
                entity.AccessAllowed = IsAllowedEntityForUser(entity.Id, user);

                if (entity.AccessAllowed) continue;
                
                entity.Id = Guid.Empty;
                entity.Title = string.Empty;
                entity.NodeType = string.Empty;
                entity.RelationType = string.Empty;
                entity.RelationCreatingType = string.Empty;
            }
        }

        private bool IsAllowedEntityForUser(Guid id, User user)
        {
            return _securityLevelChecker.AccessGranted(user.SecurityLevelsIndexes,
                _ontologyService.GetNode(id).OriginalNode.GetSecurityLevelIndexes());
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

        public MaterialSign GetMaterialSign(string signValue)
        {
            var entity = _materialSignRepository.GetByValue(signValue);

            if (entity is null) return null;

            return _mapper.Map<MaterialSign>(entity);
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

            return (materials, materials.Count);
        }

        public async Task<IReadOnlyCollection<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId, Guid userId, CancellationToken cancellationToken = default)
        {
            var response = await _materialElasticService.CountMaterialsByTypeAndNodeAsync(nodeId, userId, cancellationToken);
            var result = response
                .Select(_ => new MaterialsCountByType
                {
                    Type = _.Key,
                    Count = _.Value
                })
                .ToArray();

            return result;
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeId(Guid nodeId)
        {
            var materialsByNode = await GetMaterialCollectionByNodeIdAsync(nodeId, false);
            var materials = materialsByNode.Select(p => _materialDocumentMapper.Map(p));
            return (materials, materials.Count());
        }

        public async Task<OutputCollection<Material>> GetMaterialsByNodeIdAsync(Guid nodeId, Guid userId, CancellationToken cancellationToken)
        {
            var documentCollection = await _materialElasticService.GetMaterialCollectionRelatedToNodeAsync(nodeId, userId, cancellationToken);

            if (!documentCollection.Any()) return EmptyMaterialCollectionInstance;

            var materialCollection = documentCollection
                                    .Select(_ => _materialDocumentMapper.Map(_))
                                    .ToArray();
            
            

            return new OutputCollection<Material>(materialCollection);
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdAndRelatedEntities(Guid nodeId)
        {
            var materialsByNode = await GetMaterialCollectionByNodeIdAsync(nodeId, true);
            var materials = materialsByNode.Select(p => _materialDocumentMapper.Map(p));
            return (materials, materials.Count());
        }

        public async Task<Dictionary<Guid, int>> CountMaterialsByNodeIdSetAsync(ISet<Guid> nodeIdSet, Guid userId, CancellationToken cancellationToken)
        {
            var materialCountTaskList = nodeIdSet
                .Select(_ => _materialElasticService.CountMaterialCollectionRelatedToNodeAsync(_, userId, cancellationToken));

            var materialCountResultList = await Task.WhenAll(materialCountTaskList);

            return materialCountResultList.ToDictionary(_ => _.Key, _ => _.Count);
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
                    .Select(p => MaterialDocument.FromJObject(p.SearchResult))
                    .Select(_materialDocumentMapper.Map);

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
                    .Select(p => MaterialDocument.FromJObject(p.SearchResult))
                    .Select(_materialDocumentMapper.Map)
                    .ToList();

            return MaterialsDto.Create(materials, searchResult.Count, searchResult.Items, searchResult.Aggregations);
        }

        public async Task<MaterialsDto> GetMaterialsCommonForEntitiesAsync(Guid userId,
            IEnumerable<Guid> nodeIdList,
            bool includeDescendants,
            string suggestion,
            DateRange createdDateRange,
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
                .Select(p => MaterialDocument.FromJObject(p.SearchResult))
                .Select(_materialDocumentMapper.Map)
                .ToList();

            return MaterialsDto.Create(materials, searchResult.Count, searchResult.Items, searchResult.Aggregations);
        }

        public async Task<Material> GetNextAssignedMaterialForUserAsync(User user, CancellationToken cancellationToken = default)
        {
            using (var rpcChannel = MessageChannels.CreateRPCPublisher<NextAssignedMessage>(_connection, _nextAssignedConfig.TargetChannel, _logger))
            {
                var message = new NextAssignedMessage { UserId = user.Id };

                var response = await rpcChannel.SendAsync(message, cancellationToken);

                if (!response.MaterialId.HasValue) return null;

                return await GetMaterialAsync(response.MaterialId.Value, user);
            };
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

        public async Task<IReadOnlyCollection<LocationHistoryDto>> GetLocationHistoriesAsync(Guid materialId)
        {
            var entity = await RunWithoutCommitAsync(uow => uow.MaterialRepository.GetByIdAsync(materialId, MaterialIncludeEnum.WithFeatures));

            var infoList = _materialDocumentMapper.MapInfos(entity);

            var nodes = infoList
                            .SelectMany(p => p.Features.Select(x => x.Node))
                            .ToArray();

            var featureIdList = nodes
                                .Where(_ => _.OriginalNode.NodeType.IsObjectSign)
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

        public async Task<bool> MaterialExists(Guid materialId)
        {
            var entity = await RunWithoutCommitAsync((unitOfWork) =>
                unitOfWork.MaterialRepository.GetByIdAsync(materialId));

            return entity != null;
        }

        public Task<IReadOnlyList<MaterialDistributionItem>> GetMaterialsForDistributionAsync(
            UserDistributionItem user,
            Expression<Func<MaterialEntity, bool>> filter)
        {
            return RunWithoutCommitAsync((unitOfWork) =>
                   unitOfWork.MaterialRepository.GetMaterialsForDistribution(user, filter));
        }

        public Task<IReadOnlyList<ResCallerReceiverDto>> GetCallInfoAsync(IReadOnlyList<Guid> nodeIds, CancellationToken cancellationToken = default) =>
            RunWithoutCommitAsync((unitOfWork) =>
                unitOfWork.MaterialRepository.GetCallInfoAsync(nodeIds, cancellationToken));
    }
}