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
    public class MaterialService : IMaterialService
    {
        private readonly OntologyContext _context;
        private readonly IFileService _fileService;
        private readonly IOntologyService _ontologyService;

        public MaterialService(OntologyContext context, IFileService fileService, IOntologyService ontologyService)
        {
            _context = context;
            _fileService = fileService;
            _ontologyService = ontologyService;
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
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Materials.Material>> GetMaterialsAsync()
        {
            var materials = _context.Materials
                .Include(m => m.Children)
                .Include(m => m.Infos)
                    .ThenInclude(m => m.Features);
            var result = new List<Materials.Material>();
            foreach (var material in materials)
                result.Add(await MapAsync(material));
            return result;
        }

        public async Task<Materials.Material> GetMaterialAsync(Guid id)
        {
            var material = _context.Materials
                .Include(m => m.Children)
                .Include(m => m.Infos)
                    .ThenInclude(m => m.Features)
                .SingleOrDefault(m => m.Id == id);
            if (material == null) return null;
            return await MapAsync(material);
        }

        // Todo: think about enumerable.Select(MapAsync) trouble
        private async Task<Materials.Material> MapAsync(Material material)
        {
            var result = new Materials.Material(material.Id, JObject.Parse(material.Data), material.Type, material.Source);
            if (material.FileId.HasValue)
                result.File = await _fileService.GetFileAsync(material.FileId.Value);
            if (material.Children != null)
                foreach (var child in material.Children)
                    result.Children.Add(await GetMaterialAsync(child.Id)); // todo: lazy load recursion with graphql resolvers
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
