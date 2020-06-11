using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Newtonsoft.Json.Linq;
using IIS.Core.Files;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;
using Iis.DataModel;
using Iis.DataModel.Materials;
using Iis.Domain.MachineLearning;
using Iis.Interfaces.Materials;

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

            var materialEntity = _mapper.Map<MaterialEntity>(material);

            _context.Add(materialEntity);

            foreach (var child in material.Children)
            {
                child.ParentId = material.Id;
                await SaveAsync(child);
            }

            foreach (var info in material.Infos)
                _context.Add(Map(info, material.Id));

            

            await _context.SaveChangesAsync();

            //await PutMaterialToElasticSearch(materialEntity.Id);

            //_eventProducer.SendAvailableForOperatorEvent(materialEntity.Id);
            //_eventProducer.SendMaterialEvent(new MaterialEventMessage{Id = materialEntity.Id, Source = materialEntity.Source, Type = materialEntity.Type});

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
        }

        public async Task<MlResponse> SaveMlHandlerResponseAsync(MlResponse response)
        {
            var responseEntity = _mapper.Map<MlResponse, MLResponseEntity>(response);

            _context.MLResponses.Add(responseEntity);

            _context.SaveChanges();

            await PutMaterialToElasticSearch(responseEntity.MaterialId);

            return _mapper.Map<MlResponse>(responseEntity);
        }

        public async Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input)
        {
            var material = _context.Materials.FirstOrDefault(p => p.Id == input.Id);

            if (!string.IsNullOrWhiteSpace(input.Title)) material.Title = input.Title;
            if (input.ImportanceId.HasValue) material.ImportanceSignId = input.ImportanceId.Value;
            if (input.ReliabilityId.HasValue) material.ReliabilitySignId = input.ReliabilityId.Value;
            if (input.RelevanceId.HasValue) material.RelevanceSignId = input.RelevanceId.Value;
            if (input.CompletenessId.HasValue) material.CompletenessSignId = input.CompletenessId.Value;
            if (input.SourceReliabilityId.HasValue) material.SourceReliabilitySignId = input.SourceReliabilityId.Value;
            if (input.ProcessedStatusId.HasValue) material.ProcessedStatusSignId = input.ProcessedStatusId.Value;
            if (input.SessionPriorityId.HasValue) material.SessionPriorityId = input.SessionPriorityId.Value;
            if (input.AssigneeId.HasValue) material.AssigneeId = input.AssigneeId;
            if (!string.IsNullOrWhiteSpace(input.Content)) material.Content = input.Content;

            var loadData = MaterialLoadData.MapLoadData(material.LoadData);
            
            if (input.Objects != null) loadData.Objects = new List<string>(input.Objects);
            if (input.Tags != null) loadData.Tags = new List<string>(input.Tags);
            if (input.States != null) loadData.States = new List<string>(input.States);
            
            material.LoadData = loadData.ToJson();

            await UpdateAsync(material);

            return await _materialProvider.GetMaterialAsync(input.Id);
        }

        private async Task<bool> PutMaterialToElasticSearch(Guid materialId)
        {
            var materialDocument = await _materialProvider.GetMaterialDocumentAsync(materialId);

            return await _elasticService.PutMaterialAsync(materialId, materialDocument);
        }

        private IEnumerable<Guid> GetNodeIdentitiesFromFeatures(JObject metadata)
        {
            //metadata.SelectToken("Features").Select(e => (e as JObject).GetValue("featureId").Value<string>())
            return null;
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

        public async Task<Material> AssignMaterialOperatorAsync(Guid materialId, Guid assigneeId)
        {
            var material = _context.Materials.FirstOrDefault(p => p.Id == materialId);
            if (material == null)
            {
                throw new ArgumentNullException("No material found by given id");
            }
            material.AssigneeId = assigneeId;
            await _context.SaveChangesAsync();
            return await _materialProvider.GetMaterialAsync(materialId);
        }

        public async Task UpdateAsync(MaterialEntity material)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                _context.Update(material);

                await _context.SaveChangesAsync();

                await PutMaterialToElasticSearch(material.Id);

                _eventProducer.SendMaterialEvent(new MaterialEventMessage { Id = material.Id, Source = material.Source, Type = material.Type });

            }
            finally
            {
                _context.Semaphore.Release();
            }
        }
    }
}
