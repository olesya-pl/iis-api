using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Files;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

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

        public async Task<IEnumerable<Materials.Material>> GetMaterialsAsync(int limit, int offset,
            Guid? parentId = null, IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null)
        {
            IEnumerable<Material> materials;
            await _context.Semaphore.WaitAsync();
            try
            {
                IQueryable<Material> materialsQ;
                if (nodeIds == null)
                {
                    materialsQ = _context.Materials
                        .Include(m => m.Infos)
                        .ThenInclude(m => m.Features);
                    if (parentId != null)
                        materialsQ = materialsQ.Where(e => e.ParentId == parentId);
                    if (types != null)
                        materialsQ = materialsQ.Where(e => types.Contains(e.Type));
                    materialsQ = materialsQ.Skip(offset).Take(limit);
                }
                else
                {
                    var nodeIdsArr = nodeIds.ToArray();
                    var idsQ = _context.MaterialFeatures
                        .Include(e => e.Info)
                        .ThenInclude(e => e.Material)
                        .Where(e => nodeIdsArr.Contains(e.NodeId));
                    if (types != null)
                        idsQ = idsQ.Where(e => types.Contains(e.Info.Material.Type));
                    var idsDist = idsQ.Select(e => e.Info.Material.Id)
                        .Distinct();
                    var materialsIds = idsDist.Skip(offset).Take(limit);
                    materialsQ = _context.Materials
                        .Include(m => m.Infos)
                        .ThenInclude(m => m.Features)
                        .Where(m => materialsIds.Contains(m.Id));
                }

                materials = await materialsQ.ToArrayAsync();
            }
            finally
            {
                _context.Semaphore.Release();
            }
            var result = new List<Materials.Material>();
            foreach (var material in materials)
                result.Add(await MapAsync(material));
            return result;
        }

        public async Task<Materials.Material> GetMaterialAsync(Guid id)
        {
            Material material;
            await _context.Semaphore.WaitAsync();
            try
            {
                material = _context.Materials
                    .Include(m => m.Infos)
                    .ThenInclude(m => m.Features)
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
        private async Task<Materials.Material> MapAsync(Material material)
        {
            var result = new Materials.Material(material.Id,
                JObject.Parse(material.Metadata),
                material.Data == null ? null : JArray.Parse(material.Data),
                material.Type, material.Source);
            if (material.FileId.HasValue)
                result.File = new FileInfo(material.FileId.Value);
            foreach (var info in material.Infos)
                result.Infos.Add(await MapAsync(info));
            return result;
        }

        private async Task<Materials.MaterialInfo> MapAsync(MaterialInfo info)
        {
            var result = new Materials.MaterialInfo(info.Id, JObject.Parse(info.Data), info.Source, info.SourceType, info.SourceVersion);
            foreach (var feature in info.Features)
                result.Features.Add(await MapAsync(feature));
            return result;
        }

        private async Task<Materials.MaterialFeature> MapAsync(MaterialFeature feature)
        {
            var result = new Materials.MaterialFeature(feature.Id, feature.Relation, feature.Value);
            result.Node = await _ontologyService.LoadNodesAsync(feature.Id, null);
            return result;
        }
    }
}
