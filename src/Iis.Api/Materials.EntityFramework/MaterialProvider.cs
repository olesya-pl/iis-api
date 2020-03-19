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

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialProvider : IMaterialProvider
    {
        private readonly OntologyContext _context;
        private readonly IOntologyService _ontologyService;

        public MaterialProvider(OntologyContext context, IOntologyService ontologyService)
        {
            _context = context;
            _ontologyService = ontologyService;
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
                    materialsQ = _context.Materials
                        .Include(m => m.MaterialInfos)
                        .ThenInclude(m => m.MaterialFeatures);
                    if (parentId != null)
                        materialsQ = materialsQ.Where(e => e.ParentId == parentId);
                    if (types != null)
                        materialsQ = materialsQ.Where(e => types.Contains(e.Type));
                    materialsQ = materialsQ.Skip(offset).Take(limit);
                }
                else
                {
                    var nodeIdsArr = nodeIds.ToArray();
                    materialsQ = _context.Materials
                        .Include(m => m.MaterialInfos)
                        .ThenInclude(m => m.MaterialFeatures)
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
                material = _context.Materials
                    .Include(m => m.MaterialInfos)
                    .ThenInclude(m => m.MaterialFeatures)
                    .SingleOrDefault(m => m.Id == id);
            }
            finally
            {
                _context.Semaphore.Release();
            }
            if (material == null) return null;
            return await MapAsync(material);
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
            foreach (var info in material.MaterialInfos)
                result.Infos.Add(await MapAsync(info));
            return result;
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
