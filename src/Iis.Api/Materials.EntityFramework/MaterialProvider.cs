using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Files;
using IIS.Core.Ontology;
using Iis.DataModel;
using Iis.Domain.Materials;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Iis.Domain;
using Iis.DataModel.Cache;
using AutoMapper;
using Iis.DataModel.Materials;
using Iis.Interfaces.Materials;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider : IMaterialProvider
    {
        private readonly OntologyContext _context;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyCache _cache;
        private readonly IMapper _mapper;

        public MaterialProvider(OntologyContext context, IOntologyService ontologyService, IOntologyCache cache, IMapper mapper)
        {
            _context = context;
            _ontologyService = ontologyService;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Material>> GetMaterialsAsync(int limit, int offset,
            Guid? parentId = null, IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null)
        {
            IEnumerable<MaterialEntity> materials;
            await _context.Semaphore.WaitAsync();
            try
            {
                IQueryable<MaterialEntity> materialsQ;
                if (nodeIds == null)
                {
                    materialsQ = GetMaterialQuery();
                    if (parentId != null)
                        materialsQ = materialsQ.Where(e => e.ParentId == parentId);
                    if (types != null)
                        materialsQ = materialsQ.Where(e => types.Contains(e.Type));
                    materialsQ = materialsQ.Skip(offset).Take(limit);
                }
                else
                {
                    var nodeIdsArr = nodeIds.ToArray();
                    materialsQ = GetMaterialQuery()
                        .Where(m => m.MaterialInfos.Any(i => i.MaterialFeatures.Any(f => nodeIdsArr.Contains(f.NodeId))))
                        .OrderByDescending(m => m.CreatedDate)
                        .Skip(offset)
                        .Take(limit)
                    ;
                }

                materials = await materialsQ
                    .ToArrayAsync();
            }
            finally
            {
                _context.Semaphore.Release();
            }
            var result = new List<Material>();
            foreach (var material in materials)
                result.Add(await MapAsync(material));
            return result;
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

        private IQueryable<MaterialEntity> GetMaterialQuery()
        {
            return _context.Materials
                    .AsNoTracking()
                    .Include(m => m.Importance)
                    .Include(m => m.Reliability)
                    .Include(m => m.Relevance)
                    .Include(m => m.Completeness)
                    .Include(m => m.SourceReliability)
                    .Include(m => m.MaterialInfos)
                    .ThenInclude(m => m.MaterialFeatures)
                    .AsNoTracking();
        }

        // Todo: think about enumerable.Select(MapAsync) trouble
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

            foreach (var info in material.MaterialInfos)
                result.Infos.Add(await MapAsync(info));
            return result;
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
            if (json.ContainsKey("receivingDate")) result.ReceivingDate = (DateTime)json["receivingDate"];
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
            result.Node = await _ontologyService.LoadNodesAsync(feature.Id, null);
            return result;
        }
    }
}
