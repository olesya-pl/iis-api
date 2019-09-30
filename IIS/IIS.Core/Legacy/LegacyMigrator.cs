using System;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Files.EntityFramework;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using N = IIS.Core.Ontology;
using Attribute = IIS.Core.Ontology.EntityFramework.Context.Attribute;

namespace IIS.Legacy.EntityFramework
{
    public class LegacyMigrator : ILegacyMigrator
    {
        private readonly string _connectionString;
        private ContourContext _contourContext;
        private readonly OntologyContext _ontologyContext;
        private readonly N.IOntologyProvider _ontologyProvider;

        public LegacyMigrator(IConfiguration configuration, OntologyContext ontologyContext, N.IOntologyProvider ontologyProvider)
        {
            _connectionString = configuration.GetConnectionString("db-legacy");
            _ontologyContext = ontologyContext;
            _ontologyProvider = ontologyProvider;

        }

        private N.Ontology _ontology;
        private DateTime _now;

        private void Init()
        {
            if (_connectionString == null)
                throw new ArgumentException("There is no db-legacy connection string configured.");
            var opts = new DbContextOptionsBuilder().UseNpgsql(_connectionString).Options;
            _contourContext = new ContourContext(opts);

            _now = DateTime.UtcNow;
            _contourContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            _ontologyContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task Migrate(CancellationToken cancellationToken = default)
        {
            Init();
            _ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
            await DropEntities(cancellationToken);
            await MigrateEntities(cancellationToken);
            await MigrateRelations(cancellationToken);
            await MigrateAttributes(cancellationToken);
            await _ontologyContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DropEntities(CancellationToken cancellationToken = default)
        {
            await _ontologyContext.Database.ExecuteSqlCommandAsync("DELETE FROM \"Nodes\"", cancellationToken);
        }

        public async Task PagedMigration(int total, int pageSize,
            Func<int, int, CancellationToken, Task> loader, CancellationToken cancellationToken = default)
        {
            for (var skip = 0; skip < total; skip += pageSize)
            {
                await loader(skip, pageSize, cancellationToken);
            }
        }

        public async Task MigrateEntities(CancellationToken cancellationToken = default)
        {
            foreach (var oEntity in _contourContext.Entities
                .Include(e => e.Type))
            {
                _ontologyContext.Add(Map(oEntity));
            }
        }

        public async Task MigrateRelations(CancellationToken cancellationToken = default)
        {
            foreach (var oRelation in _contourContext.Relations
                .Include(e => e.Type)
                .Include(e => e.Source.Type))
            {
                _ontologyContext.Add(Map(oRelation));
            }
        }

        public async Task MigrateAttributes(CancellationToken cancellationToken = default)
        {
            foreach (var oAttribute in _contourContext.AttributeValues
                .Include(e => e.Attribute)
                .Include(e => e.Entity.Type))
            {
                var attr = Map(oAttribute);
                var relationId = Guid.NewGuid();
                var relation = new Node
                {
                    Id = relationId,
                    CreatedAt = _now,
                    UpdatedAt = _now,
                    IsArchived = false,
                    TypeId = GetRelationType(oAttribute).Id,
                    Relation = new Relation
                    {
                        Id = relationId,
                        SourceNodeId = oAttribute.EntityId,
                        TargetNodeId = attr.Id,
                    }
                };
                _ontologyContext.Add(attr);
                _ontologyContext.Add(relation);
            }
        }

        private N.EntityType GetEntityType(OEntity oEntity)
        {
            return _ontology.GetEntityType(oEntity.Type.Code);
        }

        private N.EmbeddingRelationType GetRelationType(ORelation oRelation)
        {
            var sourceType = GetEntityType(oRelation.Source);
            var relationName = oRelation.Type.Code.ToLowerCamelcase();
            var relationType = sourceType.GetProperty(relationName)
                               ?? throw new Exception($"Relation type {sourceType.Name}.{relationName} was not found");
            if (!relationType.IsEntityType)
                throw new Exception("Found wrong relation type");
            return relationType;
        }

        private N.EmbeddingRelationType GetRelationType(OAttributeValue oAttributeValue)
        {
            var sourceType = GetEntityType(oAttributeValue.Entity);
            var relationName = oAttributeValue.Attribute.Code.ToLowerCamelcase();
            var relationType = sourceType.GetProperty(relationName)
                               ?? throw new Exception($"Relation type {sourceType.Name}.{relationName} was not found");
            if (!relationType.IsAttributeType)
                throw new Exception("Found wrong relation type");
            return relationType;
        }

        private N.AttributeType GetAttributeType(OAttributeValue oAttributeValue)
        {
            var relationType = GetRelationType(oAttributeValue);
            return relationType.AttributeType;
        }

        private Node Map(OEntity oEntity)
        {
            var result = new Node
            {
                Id = oEntity.Id,
                CreatedAt = oEntity.CreatedAt,
                UpdatedAt = oEntity.UpdatedAt,
                IsArchived = oEntity.DeletedAt != null,
                TypeId = GetEntityType(oEntity).Id,
            };
            return result;
        }

        private Node Map(ORelation oRelation)
        {
            var result = new Node
            {
                Id = oRelation.Id,
                CreatedAt = oRelation.CreatedAt,
                UpdatedAt = oRelation.CreatedAt,
                IsArchived = oRelation.DeletedAt != null,
                TypeId = GetRelationType(oRelation).Id,
                Relation = new Relation
                {
                    Id = oRelation.Id,
                    SourceNodeId = oRelation.SourceId,
                    TargetNodeId = oRelation.TargetId,
                }
            };
            return result;
        }

        private Node Map(OAttributeValue oAttributeValue)
        {
            var attributeType = GetAttributeType(oAttributeValue);
            var value = attributeType.ScalarTypeEnum == N.ScalarType.File
                ? MapFileValue(oAttributeValue.Value)
                : oAttributeValue.Value;
            var result = new Node
            {
                Id = oAttributeValue.Id,
                CreatedAt = oAttributeValue.CreatedAt,
                UpdatedAt = oAttributeValue.CreatedAt,
                IsArchived = oAttributeValue.DeletedAt != null,
                TypeId = attributeType.Id,
                Attribute = new Attribute
                {
                    Id = oAttributeValue.Id,
                    Value = value
                }
            };
            return result;
        }

        private string MapFileValue(string legacyValue)
        {
            return JObject.Parse(legacyValue).GetValue("fileId")?.Value<string>();
        }

        public async Task MigrateFiles(CancellationToken cancellationToken = default)
        {
            Init();
            foreach (var oFile in _contourContext.TemporaryFiles)
            {
                _ontologyContext.Add(new File
                {
                    Id = oFile.Id,
                    Contents = oFile.File,
                    ContentType = oFile.Type,
                    Name = oFile.Title,
                    IsTemporary = false,
                    UploadTime = oFile.CreatedAt.GetValueOrDefault(),
                });
            }

            await _ontologyContext.SaveChangesAsync(cancellationToken);
        }
    }
}
