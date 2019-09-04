using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.GraphQL.Materials;
using IIS.Core.Ontology;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using EmbeddingOptions = IIS.Core.Ontology.EmbeddingOptions;
using Node = IIS.Core.Ontology.Node;

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

        public async Task ExtractInfo(Materials.Material material)
        {
            await _context.Semaphore.WaitAsync();
            try
            {
                _context.Add(ToDal(material.Metadata, material.Id));
                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.Semaphore.Release();
            }
            await ExtractFeatures(material.Id); // todo: to separate service
        }

        // ----- features ----- //

        private async Task ExtractFeatures(Guid materialId)
        {
            var material = await _materialService.GetMaterialAsync(materialId);
            foreach (var info in material.Infos)
                await ExtractFeatures(info);
        }

        private async Task ExtractFeatures(Materials.MaterialInfo info)
        {
            var view = info.Data.ToObject<Metadata>();
            var type = _ontologyTypesService.GetEntityType("EmailSign");
            var relationType = type.GetProperty("value");
            var features = new List<Materials.MaterialFeature>();
            foreach (var node in view.Features.Nodes)
            {
                var feat = ToDomain(node);
                feat = await MapToNodeDirect(feat, type, relationType);
                features.Add(feat);
                _context.Add(ToDal(feat, info.Id));
            }
            await CreateRelations(features, type);
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
                feature.Node = new Entity(existingRelation.SourceNodeId, type); // pass only id
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

        // ----- Relation creation ----- //

        private string GetName(string rel1, string rel2) => $"EmailSign_{rel1}_{rel2}";

        private async Task CreateRelations(List<Materials.MaterialFeature> features, EntityType type)
        {
            foreach (var feat1 in features)
            {
                foreach (var feat2 in features)
                {
                    if (feat1.Relation == feat2.Relation) continue;
                    var rname = GetName(feat1.Relation, feat2.Relation);
                    var rtype = await GetRelationType(rname, type.Id);
                    await SaveRelation(feat1.Node, feat2.Node, rtype);
                }
            }
        }

        private async Task<EmbeddingRelationType> GetRelationType(string name, Guid entityTypeId)
        {
            var type = _ontologyTypesService.Types.OfType<EmbeddingRelationType>().SingleOrDefault(t => t.Name == name);
            if (type != null) return type;
            // kill me for this
            var rt = new EmbeddingRelationType(Guid.NewGuid(), name, EmbeddingOptions.Multiple);
            ((List<Ontology.Type>)_ontologyTypesService.Types).Add(rt);
            var ctxType = new IIS.Core.Ontology.EntityFramework.Context.Type
            {
                Id = rt.Id, Kind = Kind.Relation, Name = name, Meta = "{}", Title = name,
            };
            ctxType.RelationType = new Ontology.EntityFramework.Context.RelationType
            {
                Id = rt.Id, Kind = RelationKind.Embedding,
                EmbeddingOptions = Ontology.EntityFramework.Context.EmbeddingOptions.Multiple,
                SourceTypeId = entityTypeId, TargetTypeId = entityTypeId,
            };
            _context.Add(ctxType);
            await _context.SaveChangesAsync();
            return rt;
        }

        private async Task SaveRelation(Node node1, Node node2, EmbeddingRelationType rtype)
        {
            var node = new IIS.Core.Ontology.EntityFramework.Context.Node
            {
                Id = Guid.NewGuid(), TypeId = rtype.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            };
            node.Relation = new IIS.Core.Ontology.EntityFramework.Context.Relation
            {
                Id = node.Id, SourceNodeId = node1.Id, TargetNodeId = node2.Id
            };
            _context.Add(node);
            await _context.SaveChangesAsync();
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
