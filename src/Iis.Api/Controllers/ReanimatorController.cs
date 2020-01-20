using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IIS.Legacy.EntityFramework;
using OAttributeType = Iis.Domain.AttributeType;
using OEntityType = Iis.Domain.EntityType;
using EmbeddingRelationType = Iis.Domain.EmbeddingRelationType;
using InheritanceRelationType = Iis.Domain.InheritanceRelationType;
using ORelationType = Iis.Domain.RelationType;
using System.Linq;
using System.Collections.Generic;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Seeding;
using Iis.DataModel;
using Iis.Domain;
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
        private Seeder _seeder;
        private Ontology.EntityFramework.OntologyProvider _ontologyProvider;

        public ReanimatorController(ILegacyOntologyProvider legacyOntologyProvider, Ontology.IOntologyProvider ontologyProvider, Seeder seeder, OntologyContext context)
        {
            _legacyOntologyProvider = legacyOntologyProvider;
            _seeder = seeder;
            _context = context;
            _ontologyProvider = (Ontology.EntityFramework.OntologyProvider)ontologyProvider;
        }

        [HttpGet]
        [Route("/api/test")]
        public async Task<IActionResult> Test(CancellationToken cancellationToken)
        {
            var tasks = Enumerable.Range(1, 10)
                .Select(e => _ontologyProvider.GetOntologyAsync(cancellationToken));

            var data = await Task.WhenAll(tasks);

            return Ok();
        }

        [HttpGet]
        [Route("/api/clear")]
        public async Task<IActionResult> Clear(CancellationToken cancellationToken)
        {
            _context.Nodes.RemoveRange(await _context.Nodes.ToArrayAsync(cancellationToken));
            _context.Attributes.RemoveRange(await _context.Attributes.ToArrayAsync(cancellationToken));
            _context.Relations.RemoveRange(await _context.Relations.ToArrayAsync(cancellationToken));
            await _context.SaveChangesAsync(cancellationToken);

            return Ok("Database is empty now.");
        }

        [HttpGet]
        [Route("/api/seed")]
        public async Task<IActionResult> Seed(CancellationToken cancellationToken)
        {
            //if (_context.Nodes.Any()) return Ok("Cannot seed database because it is not empty. Clear db explicitly via /api/clear");

            await _seeder.SeedAsync("contour", cancellationToken);

            return Ok("Seeded");
        }

        [HttpGet]
        [Route("/api/migrate")]
        public async Task<IActionResult> Migrate(CancellationToken cancellationToken)
        {
            await _context.Database.MigrateAsync(cancellationToken);

            return Ok("Migration has been applied.");
        }

        [HttpGet]
        [Route("/api/cleartypes")]
        public async Task<IActionResult> ClearTypes(CancellationToken cancellationToken)
        {
            _context.NodeTypes.RemoveRange(await _context.NodeTypes.ToArrayAsync(cancellationToken));
            _context.AttributeTypes.RemoveRange(await _context.AttributeTypes.ToArrayAsync(cancellationToken));
            _context.RelationTypes.RemoveRange(await _context.RelationTypes.ToArrayAsync(cancellationToken));
            return Ok("Migration has been applied.");
        }

        public async Task<IActionResult> Get()
        {
            throw new NotImplementedException("Broken Reanimators type conversion because of relation model change. Use Graphql mutation 'migrateTypes'.");

            _context.NodeTypes.RemoveRange(_context.NodeTypes.ToArray());
            _context.AttributeTypes.RemoveRange(_context.AttributeTypes.ToArray());
            _context.RelationTypes.RemoveRange(_context.RelationTypes.ToArray());
            await _context.SaveChangesAsync();

            var types = await _legacyOntologyProvider.GetOntologyAsync();
            var ormTypes = types.Types.Select(MapType).ToArray();
            _context.AddRange(ormTypes);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private Dictionary<System.Guid, NodeTypeEntity> _typesCache = new Dictionary<System.Guid, NodeTypeEntity>();

        private NodeTypeEntity MapType(NodeType type)
        {
            if (_typesCache.ContainsKey(type.Id)) return _typesCache[type.Id];

            if (type is OEntityType) return _typesCache[type.Id] = ToEntity((OEntityType)type);
            if (type is OAttributeType) return _typesCache[type.Id] = ToAttribute((OAttributeType)type);

            throw new System.Exception("Unknown type.");
        }

        private NodeTypeEntity ToEntity(OEntityType entityType)
        {
            var type = _typesCache[entityType.Id] = ToType(entityType);
            type.Kind = Kind.Entity;
            type.IsAbstract = entityType.IsAbstract;

            foreach (var relation in entityType.RelatedTypes.OfType<ORelationType>())
            {
                var relationType = default(RelationTypeEntity);
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

        private RelationTypeEntity ToEmbeddingRelation(EmbeddingRelationType relationType, System.Guid sourceId)
        {
            var relation = new RelationTypeEntity
            {
                Id = relationType.Id,
//                IsArray = relationType.EmbeddingOptions == Ontology.EmbeddingOptions.Multiple,
                Kind = RelationKind.Embedding,
                TargetType = MapType(relationType.TargetType),
                SourceTypeId = sourceId,
                NodeType = ToType(relationType)
            };
            relation.NodeType.Kind = Kind.Relation;
            return relation;
        }

        private RelationTypeEntity ToInheritanceRelation(InheritanceRelationType relationType, System.Guid sourceId)
        {
            var relation = new RelationTypeEntity
            {
                Id = relationType.Id,
                Kind = RelationKind.Inheritance,
                TargetType = MapType(relationType.ParentType),
                SourceTypeId = sourceId,
                NodeType = ToType(relationType)
            };
            relation.NodeType.Kind = Kind.Relation;
            return relation;
        }

        private NodeTypeEntity ToAttribute(OAttributeType attributeType)
        {
            Iis.DataModel.ScalarType MapScalarType(Iis.Domain.ScalarType scalarType)
            {
                switch (scalarType)
                {
                    case Iis.Domain.ScalarType.Boolean: return Iis.DataModel.ScalarType.Boolean;
                    case Iis.Domain.ScalarType.DateTime: return Iis.DataModel.ScalarType.Date;
                    case Iis.Domain.ScalarType.Decimal: return Iis.DataModel.ScalarType.Decimal;
                    case Iis.Domain.ScalarType.File: return Iis.DataModel.ScalarType.File;
                    case Iis.Domain.ScalarType.Geo: return Iis.DataModel.ScalarType.Geo;
                    case Iis.Domain.ScalarType.Integer: return Iis.DataModel.ScalarType.Int;
                    case Iis.Domain.ScalarType.String: return Iis.DataModel.ScalarType.String;
                    default: throw new System.NotImplementedException();
                }
            }

            var type = ToType(attributeType);
            type.Kind = Kind.Attribute;
            var attr = new AttributeTypeEntity
            {
                Id = attributeType.Id,
                ScalarType = MapScalarType(attributeType.ScalarTypeEnum)
            };
            type.AttributeType = attr;
            attr.NodeType = type;

            return type;
        }

        private static NodeTypeEntity ToType(NodeType type)
        {
            return new NodeTypeEntity
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
