using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IIS.Legacy.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using OAttributeType = IIS.Core.Ontology.AttributeType;
using OType = IIS.Core.Ontology.Type;
using OEntityType = IIS.Core.Ontology.EntityType;
using EmbeddingRelationType = IIS.Core.Ontology.EmbeddingRelationType;
using InheritanceRelationType = IIS.Core.Ontology.InheritanceRelationType;
using ORelationType = IIS.Core.Ontology.RelationType;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class ReanimatorController : Controller
    {
        private readonly ILegacyOntologyProvider _legacyOntologyProvider;
        //private readonly QueueReanimator _reanimator;
        private OntologyContext _context;

        public ReanimatorController(ILegacyOntologyProvider legacyOntologyProvider, OntologyContext context)
        {
            _legacyOntologyProvider = legacyOntologyProvider;
            //_reanimator = reanimator;
            _context = context;
        }

        [HttpGet]
        [Route("/api/data")]
        public async Task<IActionResult> Data()
        {
            _context.Nodes.RemoveRange(_context.Nodes.ToArray());
            _context.Attributes.RemoveRange(_context.Attributes.ToArray());
            _context.Relations.RemoveRange(_context.Relations.ToArray());
            _context.SaveChanges();

            var person = _context.Types.Where(e => e.Name == "Person")
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.Type)
                .Include(e => e.OutgoingRelations).ThenInclude(e => e.TargetType)
                .First();
            var nameRelationType = person.OutgoingRelations.First(e => e.Type.Name == "firstName");
            var nameType = nameRelationType.TargetType;
            var entity = new Node { Type = person, Id = System.Guid.NewGuid() };
            var attr = new Attribute { Node = new Node { Type = nameType, Id = System.Guid.NewGuid() }, Value = "Petro" };
            var nameRelation = new Relation
            {
                Node = new Node { Type = nameRelationType.Type, Id = System.Guid.NewGuid() },
                SourceNode = entity,
                TargetNode = attr.Node
            };
            _context.AddRange(entity, attr, nameRelation);
            await _context.SaveChangesAsync();

            var entity2 = new Node { Type = person, Id = System.Guid.NewGuid() };
            var attr2 = new Attribute { Node = new Node { Type = nameType, Id = System.Guid.NewGuid() }, Value = "Inner Petro" };
            var nameRelation2 = new Relation
            {
                Node = new Node { Type = nameRelationType.Type, Id = System.Guid.NewGuid() },
                SourceNode = entity,
                TargetNode = entity2
            };

            _context.AddRange(entity2, attr2, nameRelation2);
            await _context.SaveChangesAsync();

            return Ok();
        }

        public async Task<IActionResult> Get()
        {
            _context.Types.RemoveRange(_context.Types.ToArray());
            _context.AttributeTypes.RemoveRange(_context.AttributeTypes.ToArray());
            _context.RelationTypes.RemoveRange(_context.RelationTypes.ToArray());
            await _context.SaveChangesAsync();

            var types = await _legacyOntologyProvider.GetTypesAsync();
            var ormTypes = types.Select(MapType).ToArray();
            _context.AddRange(ormTypes);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private Dictionary<System.Guid, Type> _typesCache = new Dictionary<System.Guid, Type>();

        private Type MapType(OType type)
        {
            if (_typesCache.ContainsKey(type.Id)) return _typesCache[type.Id];

            if (type is OEntityType) return _typesCache[type.Id] = ToEntity((OEntityType)type);
            if (type is OAttributeType) return _typesCache[type.Id] = ToAttribute((OAttributeType)type);
            
            throw new System.Exception("Unknown type.");
        }

        private Type ToEntity(OEntityType entityType)
        {
            var type = _typesCache[entityType.Id] = ToType(entityType);
            type.Kind = Kind.Entity;
            type.IsAbstract = entityType.IsAbstract;

            foreach (var relation in entityType.Nodes.OfType<ORelationType>())
            {
                var relationType = default(RelationType);
                if (relation is EmbeddingRelationType)
                {
                    relationType = ToEmbeddingRelation((EmbeddingRelationType)relation, type.Id);
                }
                else if (relation is InheritanceRelationType)
                {
                    relationType = ToInheritanceRelation((InheritanceRelationType)relation, type.Id);
                }
                type.OutgoingRelations.Add(relationType);
            }

            return type;
        }

        private RelationType ToEmbeddingRelation(EmbeddingRelationType relationType, System.Guid sourceId)
        {
            var relation = new RelationType
            {
                Id = relationType.Id,
                IsArray = relationType.EmbeddingOptions == Ontology.EmbeddingOptions.Multiple,
                Kind = RelationKind.Embedding,
                TargetType = MapType(relationType.TargetType),
                SourceTypeId = sourceId,
                Type = ToType(relationType)
            };
            relation.Type.Kind = Kind.Relation;
            return relation;
        }

        private RelationType ToInheritanceRelation(InheritanceRelationType relationType, System.Guid sourceId)
        {
            var relation = new RelationType
            {
                Id = relationType.Id,
                Kind = RelationKind.Inheritance,
                TargetType = MapType(relationType.ParentType),
                SourceTypeId = sourceId,
                Type = ToType(relationType)
            };
            relation.Type.Kind = Kind.Relation;
            return relation;
        }

        private Type ToAttribute(OAttributeType attributeType)
        {
            Ontology.EntityFramework.Context.ScalarType MapScalarType(Ontology.ScalarType scalarType)
            {
                switch (scalarType)
                {
                    case Ontology.ScalarType.Boolean: return Ontology.EntityFramework.Context.ScalarType.Boolean;
                    case Ontology.ScalarType.DateTime: return Ontology.EntityFramework.Context.ScalarType.Date;
                    case Ontology.ScalarType.Decimal: return Ontology.EntityFramework.Context.ScalarType.Decimal;
                    case Ontology.ScalarType.File: return Ontology.EntityFramework.Context.ScalarType.File;
                    case Ontology.ScalarType.Geo: return Ontology.EntityFramework.Context.ScalarType.Geo;
                    case Ontology.ScalarType.Integer: return Ontology.EntityFramework.Context.ScalarType.Int;
                    case Ontology.ScalarType.String: return Ontology.EntityFramework.Context.ScalarType.String;
                    default: throw new System.NotImplementedException();
                }
            }

            var type = ToType(attributeType);
            type.Kind = Kind.Attribute;
            var attr = new AttributeType
            {
                Id = attributeType.Id,
                ScalarType = MapScalarType(attributeType.ScalarTypeEnum)
            };
            type.AttributeType = attr;
            attr.Type = type;
            
            return type;
        }

        private static Type ToType(OType type)
        {
            return new Type
            {
                Id = type.Id,
                Name = type.Name,
                Title = type.Title,
                CreatedAt = type.CreatedAt,
                UpdatedAt = type.UpdatedAt,
                Meta = type.Meta?.ToString()
            };
        }

        [HttpPost("/api/schemarestore")]
        public async Task<IActionResult> SchemaRestore()
        {
            //await _reanimator.RestoreSchema();
            return Ok();
        }

        [HttpPost("/api/restore")]
        public async Task<IActionResult> Restore(CancellationToken cancellationToken)
        {
            //await _reanimator.RestoreOntology(cancellationToken);
            return Ok();
        }
    }
}
