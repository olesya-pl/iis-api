using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Files;
using IIS.Core.GSM.Producer;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.Extensions.DependencyInjection;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialService : IMaterialService
    {
        private readonly OntologyContext _context;
        private readonly IFileService _fileService;
        private readonly IMaterialEventProducer _eventProducer;
        private readonly IMaterialProvider _materialProvider;
        private readonly IEnumerable<IMaterialProcessor> _materialProcessors;

        public MaterialService(OntologyContext context, IFileService fileService, IMaterialEventProducer eventProducer,
            IMaterialProvider materialProvider, IEnumerable<IMaterialProcessor> materialProcessors)
        {
            _context = context;
            _fileService = fileService;
            _eventProducer = eventProducer;
            _materialProvider = materialProvider;
            _materialProcessors = materialProcessors;
        }


        public async Task SaveAsync(Materials.Material material)
        {
            await SaveAsync(material, null);
        }

        public async Task SaveAsync(Materials.Material material, Guid? parentId)
        {
            await SaveAsync(material, parentId, null);
        }

        public async Task SaveAsync(Materials.Material material, Guid? parentId, IEnumerable<IIS.Core.GraphQL.Materials.Node> nodes)
        {
            if (material.File != null) // if material has attached file
            {
                var file = await _fileService.GetFileAsync(material.File.Id); // explicit file check in case of no FK to file service
                if (file == null) throw new ArgumentException($"File with guid {material.File.Id} was not found");
                if (!file.IsTemporary) throw new ArgumentException($"File with guid {material.File.Id} is already used");

                await _fileService.MarkFilePermanentAsync(file.Id);

                // todo: implement correct file type - material type compatibility checking
                if (material.Type == "cell.voice" && !file.ContentType.StartsWith("audio/"))
                    throw new ArgumentException($"\"{material.Type}\" material expects audio file to be attached. Got \"{file.ContentType}\"");
            }

            if (parentId.HasValue && _materialProvider.GetMaterialAsync(parentId.Value) == null)
                throw new ArgumentException($"Material with guid {parentId.Value} does not exist");

            _context.Add(Map(material, parentId));

            foreach (var child in material.Children)
                await SaveAsync(child, material.Id);

            foreach (var info in material.Infos)
                _context.Add(Map(info, material.Id));

            await _context.SaveChangesAsync();

            // todo: put message to rabbit instead of calling another service directly

            if (material.Metadata.SelectToken("Features.Nodes") != null)
            {
                foreach (var processor in _materialProcessors)
                    await processor.ExtractInfoAsync(material);
            }
            // end
            // todo: multiple queues for different material types
            if (material.File != null && material.Type == "cell.voice")
                _eventProducer.SendMaterialAddedEventAsync(
                    new MaterialAddedEvent { FileId = material.File.Id, MaterialId = material.Id});

            _eventProducer.SendMaterialAddedEventAsync(new MaterialAddedEvent { Nodes = nodes?.ToList() });
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

        private Material Map(Materials.Material material, Guid? parentId = null)
        {
            return new Material
            {
                Id = material.Id,
                FileId = material.File?.Id,
                ParentId = parentId,
                Metadata = material.Metadata.ToString(),
                Data = material.Data?.ToString(),
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
