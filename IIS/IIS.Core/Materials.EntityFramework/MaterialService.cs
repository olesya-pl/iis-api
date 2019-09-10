using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Files;
using IIS.Core.GSM.Producer;
using IIS.Core.Materials.EntityFramework.Workers;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialService : IMaterialService
    {
        private readonly OntologyContext _context;
        private readonly IFileService _fileService;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyProvider _ontologyProvider;
        private readonly IMaterialEventProducer _eventProducer;

        public MaterialService(OntologyContext context, IFileService fileService, IOntologyService ontologyService,
            IOntologyProvider ontologyProvider, IMaterialEventProducer eventProducer)
        {
            _context = context;
            _fileService = fileService;
            _ontologyService = ontologyService;
            _ontologyProvider = ontologyProvider;
            _eventProducer = eventProducer;
        }

        public async Task SaveAsync(Materials.Material material)
        {
            await SaveAsync(material, null);
        }

        public async Task SaveAsync(Materials.Material material, Guid? parentId)
        {
            if (material.File != null) // if material has attached file
            {
                var file = await _fileService.GetFileAsync(material.File.Id); // explicit file check in case of no FK to file service
                if (file == null) throw new ArgumentException($"File with guid {material.File.Id} was not found");
                if (!file.IsTemporary) throw new ArgumentException($"File with guid {material.File.Id} is already used");
                await _fileService.MarkFilePermanentAsync(file.Id);
            }
            if (parentId.HasValue && GetMaterialAsync(parentId.Value) == null)
                throw new ArgumentException($"Material with guid {parentId.Value} does not exist");
            _context.Add(Map(material, parentId));
            foreach (var child in material.Children)
                await SaveAsync(child, material.Id);
            foreach (var info in material.Infos)
                _context.Add(Map(info, material.Id));
            // todo: put message to rabbit instead of calling another service directly
            await new MetadataExtractor(_context, this, _ontologyService, _ontologyProvider)
                .ExtractInfo(material);
            // end
            await _context.SaveChangesAsync();
            _eventProducer.SendMaterialAddedEventAsync(new MaterialAddedEvent { Id = material.Id });
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
                    materialsQ = materialsQ.Skip(offset).Take(limit);
                }
                else
                {
                    var nodeIdsArr = nodeIds.ToArray();
                    var idsQ = _context.MaterialFeatures
                        .Include(e => e.Info)
                        .ThenInclude(e => e.Material)
                        .Where(e => nodeIdsArr.Contains(e.NodeId))
                        .Select(e => e.Info.Material.Id)
                        .Distinct();
                    var materialsIds = idsQ.Skip(offset).Take(limit);
                    materialsQ = _context.Materials
                        .Include(m => m.Infos)
                        .ThenInclude(m => m.Features)
                        .Where(m => materialsIds.Contains(m.Id));
                }

                if (types == null)
                    materialsQ = materialsQ.Where(e => types.Contains(e.Type));

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

        public async Task SaveAsync(Guid materialId, Materials.MaterialInfo materialInfo)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                var mi = new MaterialInfo
                {
                    Id = materialInfo.Id, Data = materialInfo.Data?.ToString(), MaterialId = materialId,
                    Source = materialInfo.Source, SourceType = materialInfo.SourceType,
                    SourceVersion = materialInfo.SourceVersion
                };
                _context.Add(mi);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.Semaphore.Release();
            }
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

        private Material Map(Materials.Material material, Guid? parentId = null)
        {
            return new Material
            {
                Id = material.Id,
                FileId = material.File?.Id,
                ParentId = parentId,
                Metadata = material.Metadata.ToString(),
                Data = material.Data.ToString(),
                Type = material.Type,
                Source = material.Source,
            };
        }

        private MaterialInfo Map(Materials.MaterialInfo info, Guid materialId)
        {
            return new MaterialInfo
            {
                Id = info.Id,
                MaterialId = materialId,
                Data = info.Data.ToString(),
                Source = info.Source,
                SourceType = info.SourceType,
                SourceVersion = info.SourceVersion,
            };
        }
    }
}
