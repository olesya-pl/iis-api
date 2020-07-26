using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.DbLayer.Extensions;
using Iis.DbLayer.MaterialEnum;
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
        private readonly MaterialIncludeEnum[] _includeAll = new MaterialIncludeEnum[]
        {
            MaterialIncludeEnum.WithChildren,
            MaterialIncludeEnum.WithFeatures
        };

        //private readonly OntologyContext _context;
        private readonly IMLResponseRepository _mLResponseRepository;
        private readonly IElasticManager _elasticManager;
        private readonly IElasticConfiguration _elasticConfiguration;
        private readonly IMapper _mapper;
        private readonly IOntologySchema ontologySchema;

        public string[] MaterialIndexes { get; }

        public MaterialRepository(OntologyContext context,
            IMLResponseRepository mLResponseRepository,
            IElasticManager elasticManager,
            IElasticConfiguration elasticConfiguration,
            IMapper mapper, IOntologySchema ontologySchema)
        {
            //_context = context;
            _mLResponseRepository = mLResponseRepository;
            _elasticManager = elasticManager;
            _elasticConfiguration = elasticConfiguration;
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

        public async Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, CancellationToken token = default)
        {

            var material = await GetMaterialsQuery(MaterialIncludeEnum.WithChildren, MaterialIncludeEnum.WithFeatures)
            .SingleOrDefaultAsync(p => p.Id == materialId);

            var materialDocument = _mapper.Map<MaterialDocument>(material);

            materialDocument.Children = material.Children.Select(p => _mapper.Map<MaterialDocument>(p)).ToArray();

            materialDocument.NodeIds = material.MaterialInfos
                .SelectMany(p => p.MaterialFeatures)
                .Select(p => p.NodeId)
                .ToArray();

            await PopulateMLResponses(materialId, materialDocument);

            return await _elasticManager.PutDocumentAsync(MaterialIndexes.FirstOrDefault(),
                materialId.ToString("N"),
                JsonConvert.SerializeObject(materialDocument),
                token);


        }

        public async Task<SearchByConfiguredFieldsResult> SearchMaterials(IElasticNodeFilter filter, CancellationToken cancellationToken = default)
        {
            var materialFields = _elasticConfiguration.GetMaterialsIncludedFields(MaterialIndexes);
            var searchParams = new IisElasticSearchParams
            {
                BaseIndexNames = MaterialIndexes.ToList(),
                Query = string.IsNullOrEmpty(filter.Suggestion) ? "ParentId:NULL" : $"{filter.Suggestion} AND ParentId:NULL",
                From = filter.Offset,
                Size = filter.Limit,
                SearchFields = materialFields
            };
            var searchResult = await _elasticManager.Search(searchParams, cancellationToken);
            return new SearchByConfiguredFieldsResult
            {
                Count = searchResult.Count,
                Items = searchResult.Items
                    .ToDictionary(k => new Guid(k.Identifier),
                    v => new SearchByConfiguredFieldsResultItem { Highlight = v.Higlight, SearchResult = v.SearchResult })
            };
        }

        public void AddMaterialEntity(MaterialEntity materialEntity)
        {
            Context.Materials.Add(materialEntity);
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
                .Where(e => (!typeIdList.Any() || typeIdList.Contains(e.Node.NodeTypeId)) && e.Relation.SourceNodeId == nodeId)
                .AsNoTracking()
                .Select(e => e.Node.Id)
                .ToList();
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

        private async Task PopulateMLResponses(Guid materialId, MaterialDocument materialDocument)
        {
            var mlResponses = await _mLResponseRepository.GetAllForMaterialAsync(materialId);
            if (mlResponses.Any())
            {
                var mlResponsesContainer = new JObject();
                materialDocument.MLResponses = mlResponsesContainer;
                var mlHandlers = mlResponses.GroupBy(_ => _.MLHandlerName).Select(_ => _.Key).ToArray();
                foreach (var mlHandler in mlHandlers)
                {
                    var responses = mlResponses.Where(_ => _.MLHandlerName == mlHandler).ToArray();
                    for (var i = 0; i < responses.Count(); i++)
                    {
                        var propertyName = $"{mlHandler}-{i + 1}";

                        mlResponsesContainer.Add(new JProperty(propertyName.ToLowerCamelCase().RemoveWhiteSpace(),
                            responses[i].OriginalResponse));
                    }
                }
            }
        }


        private async Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllWithPredicateAsync(int limit = 0, int offset = 0, Expression<Func<MaterialEntity, bool>> predicate = null, string sortColumnName = null, string sortOrder = null)
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
    }
}