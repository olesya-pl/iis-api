using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain.Materials;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Iis.Domain;
using Iis.DataModel.Cache;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.Interfaces.Materials;
using Iis.Interfaces.Elastic;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider : IMaterialProvider
    {
        private readonly OntologyContext _context;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyCache _cache;
        private readonly IMapper _mapper;
        private readonly IElasticService _elasticService;

        public MaterialProvider(OntologyContext context, IOntologyService ontologyService, IElasticService elasticService, IOntologyCache cache, IMapper mapper)
        {
            _context = context;
            _ontologyService = ontologyService;
            _elasticService = elasticService;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsAsync(int limit, int offset, string filterQuery,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null)
        {
            await _context.Semaphore.WaitAsync();
            
            try
            {
                IQueryable<MaterialEntity> materialsQuery = GetParentMaterialsQuery();
                IQueryable<MaterialEntity> materialsCountQuery;
                IEnumerable<Task<Material>> mappingTasks;
                IEnumerable<Material> materials;
                if(!string.IsNullOrWhiteSpace(filterQuery))
                {
                    if (!_elasticService.UseElastic)
                    {
                        return (new List<Material>(), 0);
                    }

                    var searchResult = await _elasticService.SearchByAllFieldsAsync(
                        _elasticService.MaterialIndexes, 
                        new ElasticFilter { Limit = limit, Offset = offset, Suggestion = filterQuery});
                    
                    mappingTasks =  (await materialsQuery
                                        .Where(e => searchResult.ids.Contains(e.Id))
                                        .ToArrayAsync())
                                            .Select(async entity => await MapAsync(entity));

                    materials = await Task.WhenAll(mappingTasks);

                    return (materials, searchResult.count);
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

                return (materials, materialsCount);
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
            return _mapper.Map<MaterialEntity>(material);
        }

        private IQueryable<MaterialEntity> GetParentMaterialsQuery()
        {
            return GetMaterialQuery()
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

        public async Task<Material> MapAsync(MaterialEntity material)
        {
            if (material == null) return null;

            var result = new Material(material.Id,
                JObject.Parse(material.Metadata),
                material.Data == null ? null : JArray.Parse(material.Data),
                material.Type, material.Source);
            if (material.FileId.HasValue)
                result.File = new FileInfo(material.FileId.Value);

            result.Title = material.Title;
            result.ParentId = material.ParentId;
            result.LoadData = string.IsNullOrEmpty(material.LoadData) ?
                new MaterialLoadData() :
                MapLoadData(material.LoadData);

            result.Importance = MapSign(material.Importance);
            result.Reliability = MapSign(material.Reliability);
            result.Relevance = MapSign(material.Relevance);
            result.Completeness = MapSign(material.Completeness);
            result.SourceReliability = MapSign(material.SourceReliability);

            result.Infos.AddRange(await MapInfos(material));
            result.Children.AddRange(await MapChildren(material));
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

        private MaterialSign MapSign(MaterialSignEntity sign)
        {
            return _mapper.Map<MaterialSign>(sign);
        }

        private MaterialLoadData MapLoadData(string loadData)
        {
            var result = new MaterialLoadData();
            var json = JObject.Parse(loadData);

            if (json.ContainsKey("from")) result.From = (string)json["from"];
            if (json.ContainsKey("code")) result.Code = (string)json["code"];
            if (json.ContainsKey("coordinates")) result.Coordinates = (string)json["coordinates"];
            if (json.ContainsKey("loadedBy")) result.LoadedBy = (string)json["loadedBy"];
            if (json.ContainsKey("receivingDate")) result.ReceivingDate = (DateTime?)json["receivingDate"];
            if (json.ContainsKey("objects")) result.Objects = json["objects"].Value<JArray>().ToObject<List<string>>();
            if (json.ContainsKey("tags")) result.Tags = json["tags"].Value<JArray>().ToObject<List<string>>();
            if (json.ContainsKey("states")) result.States = json["states"].Value<JArray>().ToObject<List<string>>();

            return result;
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
