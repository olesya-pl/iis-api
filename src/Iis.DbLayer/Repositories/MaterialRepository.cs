using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Extensions;
using Iis.DbLayer.MaterialEnum;
using Iis.DbLayer.Repositories.Helpers;
using Iis.Interfaces.Elastic;
using AutoMapper;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;
using Iis.Utility;
using Iis.Domain.Elastic;
using Iis.Domain.Materials;
using Iis.Interfaces.Ontology.Schema;
using IIS.Repository;

namespace Iis.DbLayer.Repositories
{
    internal class MaterialRepository : RepositoryBase<OntologyContext>, IMaterialRepository
    {
        private const string ImageVectorMlHandlerCode = "imageVector";

        private readonly MaterialIncludeEnum[] _includeAll = new MaterialIncludeEnum[]
        {
            MaterialIncludeEnum.WithChildren,
            MaterialIncludeEnum.WithFeatures
        };

        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IElasticManager _elasticManager;
        private readonly IMapper _mapper;
        private readonly IOntologySchema ontologySchema;

        public string[] MaterialIndexes { get; }

        public MaterialRepository(IMLResponseRepository mLResponseRepository,
            IElasticManager elasticManager,
            IMapper mapper, IOntologySchema ontologySchema)
        {
            _mLResponseRepository = mLResponseRepository;
            _elasticManager = elasticManager;
            _mapper = mapper;
            this.ontologySchema = ontologySchema;

            MaterialIndexes = new[] { "Materials" };
        }

        public Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes)
        {
            return GetMaterialsQuery(includes)
                    .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes)
        {
            return await GetMaterialsQuery(includes)
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

        public async Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId)
        {
            return await GetMaterialsQuery()
                            .OnlyParent()
                            .Where(p => p.AssigneeId == assigneeId)
                            .ToArrayAsync();
        }

        public async Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken token = default)
        {
            const int batchSize = 50000;

            var materialsCount = await GetMaterialsQuery(MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)
                .CountAsync();

            var responses = new List<ElasticBulkResponse>(materialsCount);
            for (var i = 0; i < (materialsCount / batchSize) + 1; i++)
            {
                token.ThrowIfCancellationRequested();

                var materialEntities = await GetMaterialsQuery(MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)
                    .OrderBy(p => p.Id)
                    .Skip(i*batchSize)
                    .Take(batchSize)
                    .ToListAsync();

                var mlResponsesList = await Context.MLResponses.AsNoTracking()
                    .ToListAsync();
                var mlResponses = mlResponsesList
                    .GroupBy(p => p.MaterialId)
                    .ToDictionary(k => k.Key, p => p.ToList());
                var materialDocuments = materialEntities
                    .Select(p => MapEntityToDocument(p))
                    .Select(p =>
                    {
                        if (!mlResponses.ContainsKey(p.Id))
                        {
                            return p;
                        }
                        var mlResponsesByEntity = mlResponses[p.Id];

                        p.MLResponses = MapMlResponseEntities(mlResponsesByEntity);

                        p.ProcessedMlHandlersCount = mlResponsesByEntity.Count();

                        decimal[] imageVector = ExtractLatestImageVector(mlResponsesByEntity);

                        if (imageVector != null)
                        {
                            p.ImageVector = imageVector;
                        }
                        return p;
                    })
                    .Aggregate("", (acc, p) => acc += $"{{ \"index\":{{ \"_id\": \"{p.Id:N}\" }} }}\n{JsonConvert.SerializeObject(p)}\n");

                var response = await _elasticManager.PutDocumentsAsync(MaterialIndexes.FirstOrDefault(), materialDocuments, token);
                responses.AddRange(response);
            }

            return responses;
        }


        public async Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, CancellationToken token = default)
        {
            var material = await GetMaterialsQuery(MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)
            .SingleOrDefaultAsync(p => p.Id == materialId);

            var materialDocument = MapEntityToDocument(material);

            var (mlResponses, imageVector) = await PopulateMLResponses(materialId);

            materialDocument.MLResponses = mlResponses;

            materialDocument.ProcessedMlHandlersCount = mlResponses.Count;

            if (imageVector != null)
            {
                materialDocument.ImageVector = imageVector;
            }
            return await _elasticManager.PutDocumentAsync(MaterialIndexes.FirstOrDefault(),
                materialId.ToString("N"),
                JsonConvert.SerializeObject(materialDocument),
                token);
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
            Context.Materials.Update(materialEntity);
        }

        public List<Guid> GetFeatureIdListThatRelatesToObjectId(Guid nodeId)
        {
            var type = ontologySchema.GetEntityTypeByName("ObjectSign");

            var typeIdList = new List<Guid>();

            if (type != null)
            {
                typeIdList = type.IncomingRelations
                    .Select(p => p.SourceTypeId)
                    .ToList();
            }
            return Context.Nodes
                .Join(Context.Relations, n => n.Id, r => r.TargetNodeId, (node, relation) => new { Node = node, Relation = relation })
                .Where(e => (!typeIdList.Any() || typeIdList.Contains(e.Node.NodeTypeId)) && e.Relation.SourceNodeId == nodeId && !e.Relation.Node.IsArchived)
                .AsNoTracking()
                .Select(e => e.Node.Id)
                .ToList();
        }

        public async Task<List<ObjectFeatureRelation>> GetFeatureIdListThatRelatesToObjectIdsAsync(IReadOnlyCollection<Guid> nodeIds)
        {
            var type = ontologySchema.GetEntityTypeByName("ObjectSign");

            var typeIdList = new List<Guid>();

            if (type != null)
            {
                typeIdList = type.IncomingRelations
                    .Select(p => p.SourceTypeId)
                    .ToList();
            }
            return await Context.Nodes
                .Join(Context.Relations, n => n.Id, r => r.TargetNodeId, (node, relation) => new { Node = node, Relation = relation })
                .Where(e => (!typeIdList.Any() 
                    || typeIdList.Contains(e.Node.NodeTypeId)) 
                    && nodeIds.Contains(e.Relation.SourceNodeId)  
                    && !e.Relation.Node.IsArchived)
                .AsNoTracking()
                .Select(e => new ObjectFeatureRelation { ObjectId = e.Relation.SourceNodeId, FeatureId = e.Node.Id })
                .ToListAsync();
        }

        public List<MaterialEntity> GetMaterialByNodeIdQuery(IList<Guid> nodeIds)
        {
            return Context.Materials
                .Join(Context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                    (Material, MaterialInfo) => new { Material, MaterialInfo })
                .Join(Context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                    (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                .Where(m => nodeIds.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialInfoJoined.Material).ToList();
        }
        public async Task<List<Guid>> GetNodeIsWithMaterials(IList<Guid> nodeIds)
        {
            return await Context.Materials
                .Join(Context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                    (Material, MaterialInfo) => new { Material, MaterialInfo })
                .Join(Context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                    (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                .Where(m => nodeIds.Contains(m.MaterialFeature.NodeId))
                .Select(m => m.MaterialFeature.NodeId)
                .ToListAsync();
        }
        public Task<List<MaterialEntity>> GetMaterialByNodeIdQueryAsync(IEnumerable<Guid> nodeIds)
        {
            return Context.Materials
                .Join(Context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                    (Material, MaterialInfo) => new { Material, MaterialInfo })
                .Join(Context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                    (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                .Where(m => nodeIds.Contains(m.MaterialFeature.NodeId))
                .AsNoTracking()
                .Select(m => m.MaterialInfoJoined.Material)
                .ToListAsync();
        }
        public Task<List<MaterialsCountByType>> GetParentMaterialByNodeIdQueryAsync(IList<Guid> nodeIds)
        {
            return Context.Materials
                .Join(Context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                    (Material, MaterialInfo) => new { Material, MaterialInfo })
                .Join(Context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                    (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                .Where(m => nodeIds.Contains(m.MaterialFeature.NodeId))
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
        private MaterialDocument MapEntityToDocument(MaterialEntity material)
        {
            var materialDocument = _mapper.Map<MaterialDocument>(material);

            materialDocument.Content = RemoveImagesFromContent(materialDocument.Content);

            materialDocument.Children = material.Children.Select(p => _mapper.Map<MaterialDocument>(p)).ToArray();

            materialDocument.NodeIds = material.MaterialInfos
                .SelectMany(p => p.MaterialFeatures)
                .Select(p => p.NodeId)
                .ToArray();

            materialDocument.NodesCount = materialDocument.NodeIds.Count();

            return materialDocument;
        }

        private async Task<(JObject mlResponses, decimal[] imageVector)> PopulateMLResponses(Guid materialId)
        {
            var mlResponses = await _mLResponseRepository.GetAllForMaterialAsync(materialId);
            decimal[] imageVector = ExtractLatestImageVector(mlResponses);
            return (MapMlResponseEntities(mlResponses), imageVector);
        }

        private static JObject MapMlResponseEntities(IEnumerable<MLResponseEntity> mlResponses)
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
                    _ => resultQuery
                };
            }

            return resultQuery;
        }

        private static decimal[] ExtractLatestImageVector(IReadOnlyCollection<MLResponseEntity> mlResponsesByEntity)
        {
            var response = mlResponsesByEntity
                                    .OrderByDescending(e => e.ProcessingDate)
                                    .FirstOrDefault(e => e.HandlerCode == ImageVectorMlHandlerCode)?
                                    .OriginalResponse;

            return FaceAPIResponseParser.GetEncoding(response);
       }
    }
}