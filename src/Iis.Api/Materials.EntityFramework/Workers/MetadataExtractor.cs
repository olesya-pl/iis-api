using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.GraphQL.Materials;
using IIS.Core.Ontology;
using Iis.DataModel;
using Iis.Domain;
using Iis.Domain.Materials;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Attribute = Iis.Domain.Attribute;
using Material = Iis.Domain.Materials.Material;
using Node = Iis.Domain.Node;
using IIS.Domain;
using Iis.Interfaces.Ontology.Schema;
using MaterialInfo = Iis.Domain.Materials.MaterialInfo;
using MaterialFeature = Iis.Domain.Materials.MaterialFeature;

namespace IIS.Core.Materials.EntityFramework.Workers
{
    public class MetadataExtractor : IMaterialProcessor
    {
        private readonly OntologyContext _context;
        private readonly IMaterialProvider _materialProvider;
        private readonly IOntologyService _ontologyService;
        private readonly IOntologyModel _ontology;

        public MetadataExtractor(OntologyContext context, IMaterialProvider materialProvider, IOntologyService ontologyService, IOntologyModel ontology)
        {
            _context = context;
            _materialProvider = materialProvider;
            _ontologyService = ontologyService;
            _ontology = ontology;
        }

        public async Task ExtractInfoAsync(Material material)
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
            var material = await _materialProvider.GetMaterialAsync(materialId);
            foreach (var info in material.Infos)
                await ExtractFeatures(info);
        }

        private async Task ExtractFeatures(MaterialInfo info)
        {
            var view = info.Data.ToObject<Metadata>();
            var features = new List<MaterialFeature>();
            await _context.Semaphore.WaitAsync();
            try
            {
                foreach (var node in view.Features.Nodes)
                {
                    var type = _ontology.GetEntityType(node.Type)
                               ?? throw new ArgumentException($"EntityType {node.Type} does not exist");
                    // TODO: should work with any entity type. Currently works only with entities which has value
                    var relationType = type.GetProperty("value");
                    MaterialFeature feat = ToDomain(node);
                    feat = await MapToNodeDirect(feat, type, relationType);
                    features.Add(feat);
                    _context.Add(ToDal(feat, info.Id));
                }

                await CreateRelations(features);
                await _context.SaveChangesAsync();
            }
            finally
            {
                _context.Semaphore.Release();
            }
        }

        // uses hardcoded EntityType and RelationType
        private async Task<MaterialFeature> MapToNodeDirect(MaterialFeature feature, EntityType type, IEmbeddingRelationTypeModel relationType)
        {
            var relationTypeId = relationType.Id;
            var existingRelation = await _context.Relations
                .Include(e => e.Node)
                .Include(e => e.TargetNode).ThenInclude(e => e.Attribute)
                .Where(e => e.Node.NodeTypeId == relationTypeId)
                .FirstOrDefaultAsync(e => e.TargetNode.Attribute.Value == feature.Value); // SingleOrDefault()?
            if (existingRelation == null)
                feature.Node = await CreateEntity(feature.Value, type, relationType);
            else
                feature.Node = new Entity(existingRelation.SourceNodeId, type); // pass only id
            return feature;
        }

        private async Task<Node> CreateEntity(string value, EntityType type, IEmbeddingRelationTypeModel relationType)
        {
            var entity = new Entity(Guid.NewGuid(), type);
            var relation = new Relation(Guid.NewGuid(), relationType);
            var attribute = new Attribute(Guid.NewGuid(), relationType.IAttributeTypeModel, value);
            relation.AddNode(attribute);
            entity.AddNode(relation);
            await _ontologyService.SaveNodeAsync(entity);
            return entity;
        }

        // ----- Relation creation ----- //

        private string GetName(MaterialFeature feat1, MaterialFeature feat2)
        {
            if (feat1.Node.Type.Id == feat2.Node.Type.Id)
                return $"{feat1.Node.Type.Name}_{feat1.Relation}_{feat2.Relation}";
            return $"{feat1.Node.Type.Name}_{feat1.Relation}_{feat2.Node.Type.Name}_{feat2.Relation}";
        }

        private async Task CreateRelations(List<MaterialFeature> features)
        {
            foreach (var feat1 in features)
            {
                foreach (var feat2 in features)
                {
                    if (feat1.Relation == feat2.Relation) continue;
                    var rname = GetName(feat1, feat2);
                    var rtype = await GetRelationType(rname, feat1.Node.Type.Id, feat2.Node.Type.Id);
                    await SaveRelation(feat1.Node, feat2.Node, rtype);
                }
            }
        }

        private async Task<IEmbeddingRelationTypeModel> GetRelationType(string name, Guid sourceTypeId, Guid targetTypeId)
        {
            // todo: change direct db access
            var ct = _context.NodeTypes.SingleOrDefault(e => e.Name == name);
            if (ct != null) return new EmbeddingRelationType(ct.Id, ct.Name, EmbeddingOptions.Multiple);
            var rt = new EmbeddingRelationType(Guid.NewGuid(), name, EmbeddingOptions.Multiple);
            var ctxType = new NodeTypeEntity
            {
                Id = rt.Id, Kind = Kind.Relation, Name = name, Meta = "{}", Title = name,
            };
            ctxType.RelationType = new RelationTypeEntity
            {
                Id = rt.Id, Kind = RelationKind.Embedding,
                EmbeddingOptions = Iis.Interfaces.Ontology.Schema.EmbeddingOptions.Multiple,
                SourceTypeId = sourceTypeId, TargetTypeId = targetTypeId,
            };
            _context.Add(ctxType);
            await _context.SaveChangesAsync();
            return rt;
        }

        private async Task SaveRelation(Node node1, Node node2, IEmbeddingRelationTypeModel rtype)
        {
            var node = new Iis.DataModel.NodeEntity
            {
                Id = Guid.NewGuid(), NodeTypeId = rtype.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
            };
            node.Relation = new RelationEntity
            {
                Id = node.Id, SourceNodeId = node1.Id, TargetNodeId = node2.Id
            };
            _context.Add(node);
            await _context.SaveChangesAsync();
        }

        public Iis.DataModel.Materials.MaterialInfoEntity ToDal(JObject metadata, Guid materialId)
        {
            return new Iis.DataModel.Materials.MaterialInfoEntity
            {
                Id = Guid.NewGuid(), Data = metadata.ToString(), MaterialId = materialId,
                Source = nameof(MetadataExtractor), SourceType = "InnerWorker", SourceVersion = "0.0"
            };
        }

        public Iis.DataModel.Materials.MaterialFeatureEntity ToDal(MaterialFeature feature, Guid materialInfoId)
        {
            return new Iis.DataModel.Materials.MaterialFeatureEntity
            {
                MaterialInfoId = materialInfoId, Id = feature.Id,
                Relation = feature.Relation, Value = feature.Value, NodeId = feature.Node.Id
            };
        }

        private MaterialFeature ToDomain(GraphQL.Materials.Node node)
        {
            return new MaterialFeature(Guid.NewGuid(), node.Relation, node.Value);
        }
    }
}
