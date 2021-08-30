using System;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Extensions;
using Iis.DbLayer.MaterialEnum;
using Iis.DbLayer.Repositories.Helpers;
using Iis.DbLayer.MaterialDictionaries;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Data;
using Iis.Domain.Materials;
using IIS.Repository;
using Iis.Utility;

namespace Iis.DbLayer.Repositories
{
    public class MaterialRepository : RepositoryBase<OntologyContext>, IMaterialRepository
    {
        private static readonly string NoneLinkTypeValue = MaterialNodeLinkType.None.ToString();
        private readonly MaterialIncludeEnum[] _includeAll = new MaterialIncludeEnum[]
        {
            MaterialIncludeEnum.WithChildren,
            MaterialIncludeEnum.WithFeatures
        };

        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IElasticManager _elasticManager;
        private readonly IMapper _mapper;
        private readonly IOntologyNodesData _ontologyData;
        public IReadOnlyCollection<string> MaterialIndexes => new[] { "Materials" };

        public MaterialRepository(IMLResponseRepository mLResponseRepository,
            IElasticManager elasticManager,
            IMapper mapper,
            IOntologyNodesData ontologyData)
        {
            _mLResponseRepository = mLResponseRepository;
            _elasticManager = elasticManager;
            _mapper = mapper;
            _ontologyData = ontologyData;
        }

        public Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQuery(includes)
                    .SingleOrDefaultAsync(e => e.Id == id);
        }

        public Task<MaterialEntity[]> GetByIdsAsync(ISet<Guid> ids, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQuery(includes)
                    .Where(e => ids.Contains(e.Id))
                    .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                            .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(
            Func<MaterialEntity, bool> filter, 
            int limit,
            params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes).Where(filter).AsQueryable().ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                .Take(limit)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllForRelatedNodeListAsync(IEnumerable<Guid> nodeIdList)
        {
            var materialIdList = await GetOnlyMaterialsForNodeIdListQuery(nodeIdList)
                                .Select(e => e.Id)
                                .ToArrayAsync();

            var materialResult = await GetAllAsync(materialIdList, 0, 0);

            return materialResult.Entities;
        }

        public Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null)
        {
            return GetAllWithPredicateAsync(limit, offset, sortColumnName: sortColumnName, sortOrder: sortOrder);
        }

        public Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<Guid> materialIdList, int limit, int offset, string sortColumnName = null, string sortOrder = null)
        {
            return GetAllWithPredicateAsync(limit, offset, e => materialIdList.Contains(e.Id), sortColumnName, sortOrder);
        }

        public Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<string> types, int limit, int offset, string sortColumnName = null, string sortOrder = null)
        {
            return GetAllWithPredicateAsync(limit, offset, e => types.Contains(e.Type), sortColumnName, sortOrder);
        }

        public async Task<IReadOnlyCollection<Guid>> GetAllUnassignedIdsAsync(
            int limit,
            int offset,
            string sortColumnName = null,
            string sortOrder = null,
            CancellationToken cancellationToken = default)
        {
            var materialQuery = Context.Materials
                .AsNoTracking()
                .Where(_ => _.AssigneeId == null)
                .ApplySorting(sortColumnName, sortOrder);
            if (limit != default)
            {
                materialQuery = materialQuery
                                    .Skip(offset)
                                    .Take(limit);
            }

            return await materialQuery
                .Select(_ => _.Id)
                .ToArrayAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId)
        {
            return await GetMaterialsQuery()
                            .OnlyParent()
                            .Where(p => p.AssigneeId == assigneeId)
                            .ToArrayAsync();
        }

        public async Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken ct = default)
        {
            const int batchSize = 5000;

            var materialsCount = await GetMaterialsQuery(_includeAll)
                .CountAsync();

            if (materialsCount == 0) return new List<ElasticBulkResponse>();

            var responses = new List<ElasticBulkResponse>(materialsCount);

            for (var i = 0; i < (materialsCount / batchSize) + 1; i++)
            {
                ct.ThrowIfCancellationRequested();

                var materialEntities = await GetMaterialsQuery(MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)
                    .OrderBy(p => p.Id)
                    .Skip(i * batchSize)
                    .Take(batchSize)
                    .ToArrayAsync();

                var mlResponsesList = await Context.MLResponses
                                    .AsNoTracking()
                                    .ToArrayAsync();

                var mlResponseDictionary = mlResponsesList
                    .GroupBy(p => p.MaterialId)
                    .ToDictionary(k => k.Key, p => p.ToArray());

                var materialDocuments = materialEntities
                    .Select(p => MapEntityToDocument(p))
                    .Select(p =>
                    {
                        var materialIdList = p.Children
                                                .Select(e => e.Id)
                                                .Union(new[] { p.Id })
                                                .ToArray();

                        var (mlResponses, mlResponsesCount) = GetResponseJsonWithCounter(p.Id, mlResponseDictionary);

                        p.MLResponses = mlResponses;

                        p.ProcessedMlHandlersCount = mlResponsesCount;

                        p.ImageVectors = GetImageVectorList(materialIdList, mlResponseDictionary);

                        return p;
                    });

                var json = ConvertToJson(materialDocuments);
                var response = await _elasticManager.PutDocumentsAsync(MaterialIndexes.FirstOrDefault(), json, false, ct);
                responses.AddRange(response);
            }

            return responses;
        }

        public async Task<List<ElasticBulkResponse>> PutCreatedMaterialsToElasticSearchAsync(IReadOnlyCollection<Guid> materialIds,
            bool waitForIndexing = false,
            CancellationToken token = default)
        {
            var materials = await GetMaterialsQuery(MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)
            .Where(p => materialIds.Contains(p.Id))
            .ToListAsync();

            var materialDocuments = materials.Select(p => MapEntityToDocument(p));
            var json = ConvertToJson(materialDocuments);

            return await _elasticManager.PutDocumentsAsync(MaterialIndexes.FirstOrDefault(), json, waitForIndexing, token);
        }

        public string ConvertToJson(IEnumerable<MaterialDocument> materialDocuments)
        {
            var sb = new StringBuilder();
            foreach (var materialDocument in materialDocuments)
            {
                sb.AppendLine($"{{ \"index\":{{ \"_id\": \"{materialDocument.Id:N}\" }} }}\n{JsonConvert.SerializeObject(materialDocument)}");
            }
            return sb.ToString();
        }

        public async Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, CancellationToken ct = default, bool waitForIndexing = false)
        {
            var material = await GetMaterialsQuery(MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)
                .SingleOrDefaultAsync(p => p.Id == materialId);

            var materialDocument = MapEntityToDocument(material);

            var materialIdList = material.Children
                                .Select(e => e.Id)
                                .Union(new[] { material.Id })
                                .ToArray();

            var responseList = await _mLResponseRepository.GetAllForMaterialListAsync(materialIdList);

            var responseDictionary = responseList
                                        .GroupBy(e => e.MaterialId)
                                        .ToDictionary(group => group.Key, group => group.ToArray());



            var (mlResponses, mlResponsesCount) = GetResponseJsonWithCounter(materialDocument.Id, responseDictionary);

            materialDocument.MLResponses = mlResponses;

            materialDocument.ProcessedMlHandlersCount = mlResponsesCount;

            materialDocument.ImageVectors = GetImageVectorList(materialIdList, responseDictionary);

            return await _elasticManager.PutDocumentAsync(MaterialIndexes.FirstOrDefault(),
                materialId.ToString("N"),
                JsonConvert.SerializeObject(materialDocument),
                waitForIndexing,
                ct);
        }

        public void AddMaterialEntity(MaterialEntity materialEntity)
        {
            Context.Materials.Add(materialEntity);
        }

        public void AddMaterialInfos(IEnumerable<MaterialInfoEntity> materialEntity)
        {
            Context.MaterialInfos.AddRange(materialEntity);
        }

        public void AddMaterialFeatures(IEnumerable<MaterialFeatureEntity> materialFeatureEntities)
        {
            Context.MaterialFeatures.AddRange(materialFeatureEntities);
        }

        public void EditMaterial(MaterialEntity materialEntity)
        {
            materialEntity.UpdatedAt = DateTime.UtcNow;
            Context.Materials.Update(materialEntity);
        }

        public Task<List<Guid>> GetNodeIsWithMaterialsAsync(IReadOnlyCollection<Guid> nodeIdCollection)
        {
            return Context.Materials
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialFeature.NodeId)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MaterialEntity>> GetMaterialCollectionByNodeIdAsync(IReadOnlyCollection<Guid> nodeIdCollection, params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material)
                .ToArrayAsync();
        }

        public async Task<IReadOnlyCollection<Guid>> GetMaterialIdCollectionByNodeIdCollectionAsync(IReadOnlyCollection<Guid> nodeIdCollection)
        {
            return await Context.Materials
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material.Id)
                .ToArrayAsync();
        }

        public Task<List<MaterialsCountByType>> GetParentMaterialByNodeIdQueryAsync(IReadOnlyCollection<Guid> nodeIdCollection)
        {
            return Context.Materials
                .JoinMaterialFeaturesAsNoTracking(Context)
                .Where(m => nodeIdCollection.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material).Where(_ => _.ParentId == null)
                .GroupBy(p => p.Type)
                .Select(group => new MaterialsCountByType
                {
                    Count = group.Count(),
                    Type = group.Key
                })
                .ToListAsync();
        }

        public void AddFeatureIdList(Guid materialId, IEnumerable<Guid> featureIdList)
        {
            foreach (var featureId in featureIdList)
            {
                Context.MaterialFeatures.Add(new MaterialFeatureEntity
                {
                    NodeId = featureId,
                    MaterialInfo = new MaterialInfoEntity
                    {
                        MaterialId = materialId
                    }
                });
            }
        }

        public async Task<IEnumerable<Guid>> GetChildIdListForMaterialAsync(Guid materialId)
        {
            return await GetMaterialsQuery(MaterialIncludeEnum.WithChildren)
                    .Where(e => e.ParentId == materialId)
                    .Select(e => e.Id)
                    .ToArrayAsync();
        }

        public Task<bool> CheckMaterialExistsAndHasContent(Guid materialId)
        {
            return GetMaterialsQuery()
                        .AnyAsync(e => e.Id == materialId && !string.IsNullOrWhiteSpace(e.Content));
        }

        public async Task RemoveMaterialsAndRelatedData(IReadOnlyCollection<Guid> fileIdList)
        {
            var removeFileIdList = fileIdList
                .Select(e => $"'{e.ToString("N")}'")
                .ToArray();

            using (var transaction = await Context.Database.BeginTransactionAsync())
            {
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"LocationHistory\" where \"MaterialId\" is not null");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"MaterialFeatures\"");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"MaterialInfos\"");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"Materials\"");
                Context.Database.ExecuteSqlRaw("DELETE FROM public.\"MLResponses\"");

                if (removeFileIdList.Any())
                {
                    Context.Database.ExecuteSqlRaw("DELETE FROM public.\"Files\" WHERE \"Id\"::text in ({0})", string.Join(" , ", fileIdList));
                }

                await transaction.CommitAsync();
            }
        }
        public Task<Guid?> GetParentIdByChildIdAsync(Guid materialId)
        {
            return Context.Materials
                .Where(e => e.Id.Equals(materialId))
                .Select(e => e.ParentId).FirstOrDefaultAsync();
        }

        private MaterialDocument MapEntityToDocument(MaterialEntity material)
        {
            var materialDocument = _mapper.Map<MaterialDocument>(material);

            materialDocument.Content = RemoveImagesFromContent(materialDocument.Content);

            materialDocument.Children = material.Children.Select(p => _mapper.Map<MaterialDocument>(p)).ToArray();

            var featureCollection = material.MaterialInfos
                .SelectMany(p => p.MaterialFeatures)
                .ToArray();
            materialDocument.NodeIds = featureCollection
                .Where(e => e.NodeLinkType == MaterialNodeLinkType.None)
                .Select(p => p.NodeId)
                .ToArray();

            materialDocument.NodesCount = materialDocument.NodeIds.Count();

            var nodeDictionary = MaterialDocumentHelper.MapFeatureCollectionToNodeDictionary(featureCollection, _ontologyData);

            var nodeFromSingsDictionary = MaterialDocumentHelper.GetObjectsLinkedBySign(nodeDictionary, _ontologyData);

            nodeDictionary.TryAddRange(nodeFromSingsDictionary);

            materialDocument.RelatedObjectCollection = MaterialDocumentHelper.MapObjectOfStudyCollection(nodeDictionary);

            materialDocument.RelatedEventCollection = MaterialDocumentHelper.MapEventCollection(nodeDictionary);

            materialDocument.RelatedSignCollection = MaterialDocumentHelper.MapSingCollection(nodeDictionary);

            materialDocument.ObjectsOfStudyCount = materialDocument.RelatedObjectCollection.Count(e => e.RelationType == NoneLinkTypeValue);

            return materialDocument;
        }

        private (JObject responseJObject, int responsesCount) GetResponseJsonWithCounter(Guid materialId, Dictionary<Guid, MLResponseEntity[]> responseDictionary)
        {
            if (responseDictionary.TryGetValue(materialId, out MLResponseEntity[] responseList))
            {
                return (ConvertMLResponsesToJson(responseList), responseList.Length);
            }

            return (new JObject(), 0);
        }

        private ImageVector[] GetImageVectorList(IReadOnlyCollection<Guid> materialIdList, Dictionary<Guid, MLResponseEntity[]> responseDictionary)
        {
            var result = new List<ImageVector>();

            foreach (var materialId in materialIdList)
            {
                if (responseDictionary.TryGetValue(materialId, out MLResponseEntity[] responseList))
                {
                    var imageVectorList = GetLatestImageVectorList(responseList, MlHandlerCodeList.ImageVector)
                                        .Select(e => new ImageVector(e))
                                        .ToArray();
                    result.AddRange(imageVectorList);
                }
            }
            return result.ToArray();
        }

        private async Task<(JObject mlResponses, int mlResponsesCount, ImageVector[] imageVector)> GetMLResponseData(Guid materialId)
        {
            var mlResponses = await _mLResponseRepository.GetAllForMaterialAsync(materialId);

            var imageVectorList = GetLatestImageVectorList(mlResponses, MlHandlerCodeList.ImageVector)
                                    .Select(e => new ImageVector(e))
                                    .ToArray();

            return (ConvertMLResponsesToJson(mlResponses), mlResponses.Count, imageVectorList);
        }

        private static JObject ConvertMLResponsesToJson(IReadOnlyCollection<MLResponseEntity> mlResponses)
        {
            var mlResponsesContainer = new JObject();
            if (mlResponses.Any())
            {
                var mlHandlers = mlResponses.GroupBy(_ => _.HandlerName).ToArray();
                foreach (var mlHandler in mlHandlers)
                {
                    string propertyName = GetMlHandlerName(mlHandler);
                    mlResponsesContainer.Add(new JProperty(propertyName,
                            mlHandler.Select(p => p.OriginalResponse).ToArray()));
                }
            }
            return mlResponsesContainer;
        }

        private static string GetMlHandlerName(IGrouping<string, MLResponseEntity> mlHandler)
        {
            var code = mlHandler.FirstOrDefault()?.HandlerCode;
            var propertyName = string.IsNullOrEmpty(code)
                ? mlHandler.Key.ToLowerCamelCase().RemoveWhiteSpace()
                : code;
            return propertyName;
        }

        private string RemoveImagesFromContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return null;

            return Regex.Replace(content, @"\(data:image.+\)", string.Empty, RegexOptions.Compiled);
        }

        private async Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllWithPredicateAsync(
            int limit = 0,
            int offset = 0,
            Expression<Func<MaterialEntity, bool>> predicate = null,
            string sortColumnName = null,
            string sortOrder = null)
        {
            var materialQuery = predicate is null
                                ? GetMaterialsQuery(_includeAll)
                                    .OnlyParent()
                                : (IQueryable<MaterialEntity>)
                                    GetMaterialsQuery(_includeAll)
                                    .OnlyParent()
                                    .Where(predicate);

            var materialCountQuery = materialQuery;

            if (limit == 0)
            {
                materialQuery = materialQuery
                                    .ApplySorting(sortColumnName, sortOrder);
            }
            else
            {
                materialQuery = materialQuery
                                    .ApplySorting(sortColumnName, sortOrder)
                                    .Skip(offset)
                                    .Take(limit);
            }

            var materials = await materialQuery
                                    .ToArrayAsync();

            var materialCount = await materialCountQuery.CountAsync();


            return (materials, materialCount);
        }

        private IQueryable<MaterialEntity> GetSimplifiedMaterialsQuery()
        {
            return Context.Materials
                    .Include(m => m.File)
                    .Include(m => m.Importance)
                    .Include(m => m.Reliability)
                    .Include(m => m.Relevance)
                    .Include(m => m.Completeness)
                    .Include(m => m.SourceReliability)
                    .Include(m => m.ProcessedStatus)
                    .Include(m => m.SessionPriority)
                    .Include(m => m.Assignee)
                    .Include(m => m.Editor)
                    .AsNoTracking();
        }

        private IQueryable<MaterialEntity> GetOnlyMaterialsForNodeIdListQuery(IEnumerable<Guid> nodeIdList)
        {
            return Context.Materials
                        .Join(Context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                            (Material, MaterialInfo) => new { Material, MaterialInfo })
                        .Join(Context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                            (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                        .Where(m => nodeIdList.Contains(m.MaterialFeature.NodeId))
                        .Select(m => m.MaterialInfoJoined.Material);
        }

        private IQueryable<MaterialEntity> GetMaterialsQuery(params MaterialIncludeEnum[] includes)
        {
            if (!includes.Any()) return GetSimplifiedMaterialsQuery();

            includes = includes.Distinct()
                                .ToArray();

            var resultQuery = GetSimplifiedMaterialsQuery();

            foreach (var include in includes)
            {
                resultQuery = include switch
                {
                    MaterialIncludeEnum.WithFeatures => resultQuery.WithFeatures(),
                    MaterialIncludeEnum.WithNodes => resultQuery.WithNodes(),
                    MaterialIncludeEnum.WithChildren => resultQuery.WithChildren(),
                    MaterialIncludeEnum.WithFiles => resultQuery.WithFiles(),
                    MaterialIncludeEnum.OnlyParent => resultQuery.OnlyParent(),
                    _ => resultQuery
                };
            }

            return resultQuery;
        }

        private static IReadOnlyCollection<decimal[]> GetLatestImageVectorList(IReadOnlyCollection<MLResponseEntity> mlResponsesByEntity, string handlerCode)
        {
            var response = mlResponsesByEntity
                                    .OrderByDescending(e => e.ProcessingDate)
                                    .FirstOrDefault(e => e.HandlerCode == handlerCode);

            return FaceAPIResponseParser.GetFaceVectorList(response?.OriginalResponse);
        }

        public async Task<IEnumerable<MaterialEntity>> GetCellSatWithChannel(int limit)
        {
            return await GetMaterialsQuery()
                .Where(m => (m.Source.StartsWith("sat.") || m.Source.StartsWith("cell.")) && m.Channel != null)
                .Take(limit)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetCellSatWithoutChannel(int limit)
        {
            return await GetMaterialsQuery()
                .Where(m => (m.Source.StartsWith("sat.") || m.Source.StartsWith("cell.")) && m.Channel == null)
                .Take(limit)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialEntity>> GetNotCellSat(int limit)
        {
            return await GetMaterialsQuery()
                .Where(m => !(m.Source.StartsWith("sat.") || m.Source.StartsWith("cell.")))
                .Take(limit)
                .ToArrayAsync();
        }

        public async Task<IEnumerable<MaterialChannelMappingEntity>> GetChannelMappingsAsync()
        {
            return await Context.MaterialChannelMappings.ToArrayAsync();
        }
    }
}