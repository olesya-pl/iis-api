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

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider : IMaterialProvider
    {
        private readonly OntologyContext _context;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyCache _cache;

        public MaterialProvider(OntologyContext context, IOntologyService ontologyService, IOntologyCache cache)
        {
            _context = context;
            _ontologyService = ontologyService;
            _cache = cache;
        }

        public async Task<IEnumerable<Material>> GetMaterialsAsync(int limit, int offset,
            Guid? parentId = null, IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null)
        {
            IEnumerable<Iis.DataModel.Materials.MaterialEntity> materials;
            await _context.Semaphore.WaitAsync();
            try
            {
                IQueryable<Iis.DataModel.Materials.MaterialEntity> materialsQ;
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

        public async Task<Material> GetMaterialAsync(Guid id)
        {
            Iis.DataModel.Materials.MaterialEntity material;
            await _context.Semaphore.WaitAsync();
            try
            {
                material = GetMaterialQuery().SingleOrDefault(m => m.Id == id);
            }
            finally
            {
                _context.Semaphore.Release();
            }
            if (material == null) return null;
            return await MapAsync(material);
        }

        public IReadOnlyCollection<Iis.DataModel.Materials.MaterialSignEntity> MaterialSigns => _cache.MaterialSigns;

        public Iis.DataModel.Materials.MaterialSignEntity GetMaterialSign(Guid id)
        {
            return _cache.GetMaterialSign(id);
        }

        private IQueryable<Iis.DataModel.Materials.MaterialEntity> GetMaterialQuery()
        {
            return _context.Materials
                    .Include(m => m.Importance)
                    .Include(m => m.Reliability)
                    .Include(m => m.Relevance)
                    .Include(m => m.Completeness)
                    .Include(m => m.SourceReliability)
                    .Include(m => m.MaterialInfos)
                    .ThenInclude(m => m.MaterialFeatures);
        }

        // Todo: think about enumerable.Select(MapAsync) trouble
        private async Task<Material> MapAsync(Iis.DataModel.Materials.MaterialEntity material)
        {
            var result = new Material(material.Id,
                JObject.Parse(material.Metadata),
                material.Data == null ? null : JArray.Parse(material.Data),
                material.Type, material.Source);
            if (material.FileId.HasValue)
                result.File = new FileInfo(material.FileId.Value);

            result.Importance = MapSign(material.Importance);
            result.Reliability = MapSign(material.Reliability);
            result.Relevance = MapSign(material.Relevance);
            result.Completness = MapSign(material.Completeness);
            result.SourceReliability = MapSign(material.SourceReliability);

            foreach (var info in material.MaterialInfos)
                result.Infos.Add(await MapAsync(info));
            return result;
        }

        private MaterialSign MapSign(Iis.DataModel.Materials.MaterialSignEntity sign)
        {
            return sign == null ? null : new MaterialSign
            {
                Id = sign.Id,
                MaterialSignTypeId = sign.MaterialSignTypeId,
                ShortTitle = sign.ShortTitle,
                Title = sign.Title,
                OrderNumber = sign.OrderNumber
            };
        }

        private async Task<MaterialInfo> MapAsync(Iis.DataModel.Materials.MaterialInfoEntity info)
        {
            var result = new MaterialInfo(info.Id, JObject.Parse(info.Data), info.Source, info.SourceType, info.SourceVersion);
            foreach (var feature in info.MaterialFeatures)
                result.Features.Add(await MapAsync(feature));
            return result;
        }

        private async Task<MaterialFeature> MapAsync(Iis.DataModel.Materials.MaterialFeatureEntity feature)
        {
            var result = new MaterialFeature(feature.Id, feature.Relation, feature.Value);
            result.Node = await _ontologyService.LoadNodesAsync(feature.Id, null);
            return result;
        }
    }
}
