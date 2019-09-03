using System;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.GraphQL.Materials;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using AttributeType = IIS.Core.Ontology.AttributeType;

namespace IIS.Core.Materials.EntityFramework.Workers
{
    public class MetadataExtractor
    {
        private readonly OntologyContext _context;
        private readonly IMaterialService _materialService;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyTypesService _ontologyTypesService;

        public MetadataExtractor(OntologyContext context, IMaterialService materialService, IOntologyService ontologyService, IOntologyTypesService ontologyTypesService)
        {
            _context = context;
            _materialService = materialService;
            _ontologyService = ontologyService;
            _ontologyTypesService = ontologyTypesService;
        }

        public async Task ExtractInfo(Guid materialId) =>
            await ExtractInfo(await _materialService.GetMaterialAsync(materialId));

        public async Task ExtractInfo(Materials.Material material)
        {
            _context.Add(ToDal(material.Metadata, material.Id));
            await _context.SaveChangesAsync();
            await ExtractFeatures(material.Id); // todo: to separate service
        }

        // ----- features ----- //

        public async Task ExtractFeatures(Guid materialId)
        {
            var material = await _materialService.GetMaterialAsync(materialId);
            foreach (var info in material.Infos)
                await ExtractFeatures(info);
        }

        public async Task ExtractFeatures(Materials.MaterialInfo info)
        {
            var view = info.Data.ToObject<Metadata>();
            var type = _ontologyTypesService.GetEntityType("EmailSign");
            var relationType = type.GetProperty("value");
            foreach (var node in view.Features.Nodes)
            {
                var feat = ToDomain(node);
                feat = await MapToNodeDirect(feat, type, relationType);
                _context.Add(ToDal(feat, info.Id));
            }
            await _context.SaveChangesAsync();
        }

        // uses hardcoded EntityType and RelationType
        private async Task<Materials.MaterialFeature> MapToNodeDirect(Materials.MaterialFeature feature, EntityType type, EmbeddingRelationType relationType)
        {
            var relationTypeId = relationType.Id;
            var existingRelation = await _context.Relations
                .Include(e => e.Node)
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Where(e => e.Node.TypeId == relationTypeId)
                .FirstOrDefaultAsync(e => e.TargetNode.Attribute.Value == feature.Value); // SingleOrDefault()?
            if (existingRelation == null)
                feature.Node = await CreateEntity(feature.Value, type, relationType);
            else
                feature.Node = new Entity(existingRelation.SourceNodeId, null); // pass only id
            return feature;
        }

        private async Task<Ontology.Node> CreateEntity(string value, EntityType type, EmbeddingRelationType relationType)
        {
            var entity = new Entity(Guid.NewGuid(), type);
            var relation = new Ontology.Relation(Guid.NewGuid(), relationType);
            var attribute = new Ontology.Attribute(Guid.NewGuid(), relationType.AttributeType, value);
            relation.AddNode(attribute);
            entity.AddNode(relation);
            await _ontologyService.SaveNodeAsync(entity);
            return entity;
        }

        public MaterialInfo ToDal(JObject metadata, Guid materialId)
        {
            return new MaterialInfo
            {
                Id = Guid.NewGuid(), Data = metadata.ToString(), MaterialId = materialId,
                Source = nameof(MetadataExtractor), SourceType = "InnerWorker", SourceVersion = "0.0"
            };
        }

        public MaterialFeature ToDal(Materials.MaterialFeature feature, Guid materialInfoId)
        {
            return new MaterialFeature
            {
                MaterialInfoId = materialInfoId, Id = feature.Id,
                Relation = feature.Relation, Value = feature.Value, NodeId = feature.Node.Id
            };
        }

        private Materials.MaterialFeature ToDomain(GraphQL.Materials.Node node)
        {
            return new Materials.MaterialFeature(Guid.NewGuid(), node.Relation, node.Value);
        }
    }
}
