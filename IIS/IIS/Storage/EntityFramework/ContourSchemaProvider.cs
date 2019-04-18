using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IIS.Storage.EntityFramework
{
    public class ContourSchemaProvider : ISchemaProvider
    {
        private readonly ContourContext _context;

        public ContourSchemaProvider(string connectionString)
        {
            var options = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;
            _context = new ContourContext(options);
        }

        public async Task<ISchema> GetSchemaAsync()
        {
            var entityTypes = await _context.EntityTypes
                //.Where(_ => _.Type != "relation")
                .Include(_ => _.EntityTypeAttributes).ThenInclude(_ => _.Attribute)
                .Include(_ => _.ForwardRelationRestrictions).ThenInclude(_ => _.Source)
                .Include(_ => _.ForwardRelationRestrictions).ThenInclude(_ => _.RelationType)
                .ToArrayAsync();
            var schema = new Schema();
            var queryType = new ObjectGraphType();
            queryType.Name = "Endpoints";
            foreach (var entityType in entityTypes)
            {
                if (entityType.Type == "relation") continue;

                var field = new FieldType
                {
                    Name = entityType.Code,
                    ResolvedType = ToObjectGraphType(entityType)
                };
                queryType.AddField(field);
            }

            schema.Query = queryType;

            return schema;
        }

        private static ObjectGraphType ToObjectGraphType(EntityType entityType)
        {
            var obj = new ObjectGraphType { Name = entityType.Code };

            GraphTypeCache[entityType.Code] = obj;

            obj.AddField(new FieldType { Name = nameof(EntityType.Id), ResolvedType = new IntGraphType() });

            //foreach (var attr in entityType.EntityTypeAttributes)
                //obj.AddField(ToFieldType(attr));

            foreach (var relation in entityType.ForwardRelationRestrictions)
            {
                var itemType = default(IGraphType);

                if (HandledTypes.Contains(relation.Source.Id))
                {
                    itemType = GraphTypeCache[relation.Source.Code];
                }
                else
                {
                    HandledTypes.Add(relation.Source.Id);

                    itemType = GraphTypeMap.ContainsKey(relation.Source.Code) 
                        ? GraphTypeMap[relation.Source.Code] 
                        : ToObjectGraphType(relation.Source); // dont move to itself
                }

                obj.AddField(new FieldType
                {
                    Name = relation.RelationType.Code,
                    ResolvedType = itemType // relation.IsCollection ? new ListGraphType(itemType) : 
                });

                //itemType.AddField(CreateRelationField());
            }

            return obj;

        }

        private static readonly HashSet<int> HandledTypes = new HashSet<int>();
        private static readonly Dictionary<string, ObjectGraphType> GraphTypeCache = new Dictionary<string, ObjectGraphType>();
        private static readonly Dictionary<string, FieldType> FieldTypeCache = new Dictionary<string, FieldType>();
        private static readonly Dictionary<string, IGraphType> GraphTypeMap = new Dictionary<string, IGraphType>
        {
            ["string"] = new StringGraphType(),
            ["int"] = new IntGraphType(),
            ["decimal"] = new DecimalGraphType(),
            ["date"] = new DateGraphType(),
            ["boolean"] = new BooleanGraphType(),
            ["geo"] = new StringGraphType(), // todo: custom scalar
            ["file"] = new StringGraphType(),// todo: custom object
            ["json"] = new StringGraphType()
        };

        private static FieldType ToFieldType(EntityTypeAttribute attribute)
        {
            if (FieldTypeCache.ContainsKey(attribute.Attribute.Code))
                return FieldTypeCache[attribute.Attribute.Code];

            var field = new FieldType
            {
                Name = attribute.Attribute.Code,
                ResolvedType = GraphTypeMap[attribute.Attribute.Type]
            };

            FieldTypeCache.Add(attribute.Attribute.Code, field);

            return field;
        }

        private static FieldType CreateRelationField()
        {
            var relationInfo = new ObjectGraphType { Name = "RelationInfo" };
            relationInfo.AddField(new FieldType { Name = nameof(Relation.Id), ResolvedType = new IntGraphType() });

            return new FieldType
            {
                Name = "_relationInfo",
                ResolvedType = relationInfo
            };
        }
    }
}
