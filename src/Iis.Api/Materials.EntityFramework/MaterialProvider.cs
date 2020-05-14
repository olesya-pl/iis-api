using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using AutoMapper;

using Iis.Utility;
using Iis.Domain;
using Iis.Domain.Materials;
using Iis.Domain.MachineLearning;
using Iis.DataModel;
using Iis.DataModel.Cache;
using Iis.DataModel.Materials;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider : IMaterialProvider
    {
        private readonly OntologyContext _context;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyCache _cache;
        private readonly IMapper _mapper;
        private readonly IElasticService _elasticService;

        public MaterialProvider(OntologyContext context,
            IOntologyService ontologyService,
            IElasticService elasticService,
            IOntologyCache cache,
            IMapper mapper)
        {
            _context = context;
            _ontologyService = ontologyService;
            _elasticService = elasticService;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<(
            IEnumerable<Material> Materials,
            int Count,
            Dictionary<Guid, SearchByConfiguredFieldsResultItem> Highlights)> GetMaterialsAsync(int limit, int offset, string filterQuery,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null)
        {
            await _context.Semaphore.WaitAsync();

            try
            {
                IQueryable<MaterialEntity> materialsQuery = GetParentMaterialsQuery(GetMaterialQuery());
                IQueryable<MaterialEntity> materialsCountQuery;
                IEnumerable<Task<Material>> mappingTasks;
                IEnumerable<Material> materials;
                if(!string.IsNullOrWhiteSpace(filterQuery))
                {
                    if (!_elasticService.UseElastic)
                    {
                        return (new List<Material>(), 0, new Dictionary<Guid, SearchByConfiguredFieldsResultItem>());
                    }

                    var searchResult = await _elasticService.SearchByConfiguredFieldsAsync(
                        _elasticService.MaterialIndexes,
                        new ElasticFilter { Limit = limit, Offset = offset, Suggestion = filterQuery});

                    var foundIds = searchResult.Items.Keys.ToList();
                    mappingTasks =  (await materialsQuery
                                        .Where(e => foundIds.Contains(e.Id))
                                        .ToArrayAsync())
                                            .Select(async entity => await MapAsync(entity));

                    materials = await Task.WhenAll(mappingTasks);

                    return (materials, searchResult.Count, searchResult.Items);
                }

                if (nodeIds == null)
                {
                    if (types != null) materialsQuery = materialsQuery.Where(e => types.Contains(e.Type));
                }
                else
                {
                    var nodeIdsArr = nodeIds.ToArray();

                    materialsQuery = materialsQuery
                        .Where(m => m.MaterialInfos.Any(i => i.MaterialFeatures.Any(f => nodeIdsArr.Contains(f.NodeId))))
                        .OrderByDescending(m => m.CreatedDate);
                }

                materialsCountQuery = materialsQuery;

                materialsQuery = materialsQuery
                                    .Skip(offset)
                                    .Take(limit);

                mappingTasks = (await materialsQuery.ToArrayAsync())
                                    .Select(async entity => await MapAsync(entity));

                materials = await Task.WhenAll(mappingTasks);

                var materialsCount = await materialsCountQuery.CountAsync();

                return (materials, materialsCount, new Dictionary<Guid, SearchByConfiguredFieldsResultItem>());
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<MaterialEntity> GetMaterialEntityAsync(Guid id)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                return await GetMaterialQuery().SingleOrDefaultAsync(m => m.Id == id);
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<Material> GetMaterialAsync(Guid id)
        {
            var material = await GetMaterialEntityAsync(id);
            return await MapAsync(material);
        }

        public async Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync()
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                return await GetMaterialQuery().ToArrayAsync();
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName)
        {
            return _cache.MaterialSigns
                .Where(ms => ms.MaterialSignType.Name == typeName)
                .OrderBy(ms => ms.OrderNumber)
                .ToList();
        }

        public MaterialSign GetMaterialSign(Guid id)
        {
            var materialSignEntity = _cache.GetMaterialSign(id);
            return _mapper.Map<MaterialSign>(materialSignEntity);
        }

        public async Task<MaterialEntity> UpdateMaterial(IMaterialUpdateInput input)
        {
            var material = await GetMaterialAsync(input.Id);
            if (input.Title != null) material.Title = input.Title;
            if (input.ImportanceId != null) material.Importance = GetMaterialSign((Guid)input.ImportanceId);
            if (input.ReliabilityId != null) material.Reliability = GetMaterialSign((Guid)input.ReliabilityId);
            if (input.RelevanceId != null) material.Relevance = GetMaterialSign((Guid)input.RelevanceId);
            if (input.CompletenessId != null) material.Completeness = GetMaterialSign((Guid)input.CompletenessId);
            if (input.SourceReliabilityId != null) material.SourceReliability = GetMaterialSign((Guid)input.SourceReliabilityId);
            if (input.Objects != null) material.LoadData.Objects = new List<string>(input.Objects);
            if (input.Tags != null) material.LoadData.Tags = new List<string>(input.Tags);
            if (input.States != null) material.LoadData.States = new List<string>(input.States);
            if (input.IsImportantSession != null) material.IsImportantSession = input.IsImportantSession;
            return _mapper.Map<MaterialEntity>(material);
        }

        public async Task<Material> MapAsync(MaterialEntity material)
        {
            if (material == null) return null;

            var result = _mapper.Map<Material>(material);

            result.Infos.AddRange(await MapInfos(material));

            result.Children.AddRange(await MapChildren(material));

            return result;
        }

        public async Task<List<MlProcessingResult>> GetMlProcessingResultsAsync(Guid materialId)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                return await _context.MLResponses
                                .Where(p => p.MaterialId == materialId)
                                .AsNoTracking()
                                .Select(p => _mapper.Map<MlProcessingResult>(p))
                                .ToListAsync();
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<JObject> GetMaterialDocumentAsync(Guid materialId)
        {
            var materialTask = GetMaterialAsync(materialId);

            var mLResponsesTask = GetMlProcessingResultsAsync(materialId);

            await Task.WhenAll(materialTask, mLResponsesTask);

            var material = await materialTask;
            var mLResponses = await mLResponsesTask;

            var jDocument = new JObject(
                new JProperty(nameof(Material.Source).ToLower(), material.Source),
                new JProperty(nameof(Material.Type).ToLower(), material.Type),
                new JProperty(nameof(Material.Importance).ToLower(), material.Importance?.Title),
                new JProperty(nameof(Material.Reliability).ToLower(), material.Reliability?.Title),
                new JProperty(nameof(Material.Relevance).ToLower(), material.Relevance?.Title),
                new JProperty(nameof(Material.Completeness).ToLower(), material.Completeness?.Title),
                new JProperty(nameof(Material.SourceReliability).ToLower(), material.SourceReliability?.Title),
                new JProperty(nameof(Material.Id).ToLower(), material.Id.ToString("N"))
            );

            if (!string.IsNullOrWhiteSpace(material.Title))
            {
                jDocument.Add(nameof(material.Title), material.Title);
            }

            if (!(material.Data is null) && material.Data.HasValues)
            {
                var materialData = new JObject();
                material.Data
                    .Select(token => new JProperty(token.Value<string>("Type"), token.Value<string>("Text")))
                    .Select(property =>
                    {
                        materialData.Add(property);
                        return property;
                    })
                    .ToList();
                jDocument.Add(new JProperty(nameof(Material.Data), materialData));
            }

            if (!(material.LoadData is null))
            {
                jDocument.Add(new JProperty(nameof(Material.LoadData), JObject.Parse(material.LoadData.ToJson())));
            }

            if(mLResponses.Any())
            {
                var mlResponses = new JObject();
                jDocument.Add(new JProperty(nameof(mlResponses), mlResponses));
                foreach (var response in mLResponses)
                {
                    var handlerName = response.MlHandlerName.ToLowerCamelCase().RemoveWhiteSpace();

                    var propertyName = $"{handlerName}-{response.Id.ToString("N")}";

                    mlResponses.Add(new JProperty(propertyName, response.ResponseText));
                }
            }


            return jDocument;
        }
        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsByNodeIdQuery(Guid nodeId)
        {
            IEnumerable<Task<Material>> mappingTasks;
            IEnumerable<Material> materials;

            IQueryable<MaterialEntity> materialsByNode = GetMaterialByNodeIdQuery(nodeId);

            mappingTasks = (await materialsByNode
                                 .ToArrayAsync())
                                 .Select(async e => await MapAsync(await GetMaterialEntityAsync(e.Id)));

            materials = await Task.WhenAll(mappingTasks);

            return (materials, materials.Count());
        }

        private IQueryable<MaterialEntity> GetMaterialByNodeIdQuery(Guid nodeId)
        {
            return _context.Materials
                            .Join(_context.MaterialInfos, m => m.Id, mi => mi.MaterialId,
                                (Material, MaterialInfo) => new { Material, MaterialInfo })
                            .Join(_context.MaterialFeatures, m => m.MaterialInfo.Id, mf => mf.MaterialInfoId,
                                (MaterialInfoJoined, MaterialFeature) => new { MaterialInfoJoined, MaterialFeature })
                            .Where(m => m.MaterialFeature.NodeId == nodeId)
                            .Select(m => m.MaterialInfoJoined.Material);
        }

        private IQueryable<MaterialEntity> GetParentMaterialsQuery(IQueryable<MaterialEntity> materialQuery)
        {
            return materialQuery
                    .Where(p => p.ParentId == null);
        }

        private IQueryable<MaterialEntity> GetMaterialQuery()
        {
            return _context.Materials
                    .AsNoTracking()
                    .Include(m => m.Importance)
                    .Include(m => m.Reliability)
                    .Include(m => m.Relevance)
                    .Include(m => m.Completeness)
                    .Include(m => m.SourceReliability)
                    .Include(m => m.Children)
                    .Include(m => m.MaterialInfos)
                    .ThenInclude(m => m.MaterialFeatures)
                    .AsNoTracking();
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

        public Task<List<MaterialsCountByType>> CountMaterialsByTypeAndNodeAsync(Guid nodeId)
        {
            return GetParentMaterialsQuery(GetMaterialByNodeIdQuery(nodeId))
                .GroupBy(p => p.Type)
                .Select(group => new MaterialsCountByType
                {
                    Count = group.Count(),
                    Type = group.Key
                })
                .ToListAsync();
        }
    }
}
