using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using IIS.Core.Files;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.Domain.MachineLearning;

namespace IIS.Core.Materials.EntityFramework
{
    public class MaterialService : IMaterialService
    {
        private readonly OntologyContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private readonly IElasticService _elasticService;
        private readonly IMaterialEventProducer _eventProducer;
        private readonly IMaterialProvider _materialProvider;
        private readonly IEnumerable<IMaterialProcessor> _materialProcessors;

        public MaterialService(OntologyContext context, 
            IFileService fileService, 
            IElasticService elasticService, 
            IMapper mapper,
            IMaterialEventProducer eventProducer,
            IMaterialProvider materialProvider, 
            IEnumerable<IMaterialProcessor> materialProcessors)
        {
            _context = context;
            _fileService = fileService;
            _elasticService = elasticService;
            _mapper = mapper;
            _eventProducer = eventProducer;
            _materialProvider = materialProvider;
            _materialProcessors = materialProcessors;
        }

        public async Task SaveAsync(Material material)
        {
            await SaveAsync(material, null);
        }

        public async Task SaveAsync(Material material, IEnumerable<IIS.Core.GraphQL.Materials.Node> nodes)
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

            if (material.ParentId.HasValue && _materialProvider.GetMaterialAsync(material.ParentId.Value) == null)
                throw new ArgumentException($"Material with guid {material.ParentId.Value} does not exist");

            var materialEntity = Map(material);

            _context.Add(materialEntity);

            foreach (var child in material.Children)
            {
                child.ParentId = material.Id;
                await SaveAsync(child);
            }

            foreach (var info in material.Infos)
                _context.Add(Map(info, material.Id));

            await _context.SaveChangesAsync();

            var (entity, mlEntities) = await _materialProvider.GetMaterialWithMLResponsesAsync(material.Id);

            await _elasticService.PutMaterialAsync(entity, mlEntities);

            _eventProducer.SendMaterialEvent(new MaterialEventMessage{Id = materialEntity.Id, Source = materialEntity.Source, Type = materialEntity.Type});
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

            SendIcaoEvent(nodes);
        }

        public async Task SaveAsync(MaterialEntity material)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                _context.Update(material);
                
                await _context.SaveChangesAsync();

                var (entity, mlEntities) = await _materialProvider.GetMaterialWithMLResponsesAsync(material.Id);

                await _elasticService.PutMaterialAsync(entity, mlEntities);
                
                _eventProducer.SendMaterialEvent(new MaterialEventMessage{Id = material.Id, Source = material.Source, Type = material.Type});

            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        public async Task<MlResponse> SaveMlHandlerResponseAsync(MlResponse response)
        {
            var responseEntity = _mapper.Map<MlResponse, MLResponseEntity>(response);

            _context.MLResponses.Add(responseEntity);

            _context.SaveChanges();
            
            var (entity, mlEntities) = await _materialProvider.GetMaterialWithMLResponsesAsync(responseEntity.MaterialId);

            await _elasticService.PutMaterialAsync(entity, mlEntities);

            return _mapper.Map<MlResponse>(entity);
        }
        
        private Guid GetIcaoNode(string icaoValue)
        {
            var q = from n in _context.Nodes
                    join t in _context.NodeTypes on n.NodeTypeId equals t.Id
                    join r in _context.Relations on n.Id equals r.SourceNodeId
                    join rp in _context.Relations on n.Id equals rp.TargetNodeId
                    join a in _context.Attributes on r.TargetNodeId equals a.Id
                    where t.Name == "ICAOSign" && a.Value == icaoValue
                    select rp.SourceNodeId;

            return q.FirstOrDefault();
        }

        private void SendIcaoEvent(IEnumerable<GraphQL.Materials.Node> nodes)
        {
            if (nodes == null) return;
            var node = nodes.Where(n => n.UpdateField != null).SingleOrDefault();
            if (node == null) return;

            var entityId = GetIcaoNode(node.Value);
            var materialAddedEvent = new MaterialAddedEvent
            {
                EntityId = entityId,
                Nodes = new List<GraphQL.Materials.Node> { node }
            };

            _eventProducer.SendMaterialAddedEventAsync(materialAddedEvent);
        }

        private MaterialEntity Map(Material material)
        {
            return new MaterialEntity
            {
                Id = material.Id,
                FileId = material.File?.Id,
                ParentId = material.ParentId,
                Metadata = material.Metadata.ToString(),
                Data = material.Data?.ToString(),
                Type = material.Type,
                Source = material.Source,
            };
        }

        private MaterialInfoEntity Map(MaterialInfo info, Guid materialId)
        {
            return new MaterialInfoEntity
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
