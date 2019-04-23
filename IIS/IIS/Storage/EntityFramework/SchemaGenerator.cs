using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using IIS.Storage.EntityFramework.Context;

namespace IIS.Storage.EntityFramework
{
    public class SchemaGenerator : ISchemaGenerator
    {
        public ISchema Generate(IEnumerable<EntityType> entityTypes, string rootName)
        {
            var schema = new Schema { Query = new ObjectGraphType { Name = rootName } };
            var types = entityTypes.Where(e => e.Type != "relation")
                .Select(GraphExtensions.ToGraphType).ToArray();

            foreach (var type in types)
                schema.Query.AddField(new FieldType { Name = type.Name, ResolvedType = type });

            return schema;
        }
    }

    internal static class GraphExtensions
    {
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

        public static IComplexGraphType ToGraphType(EntityType entityType)
        {
            if (GraphTypeMap.ContainsKey(entityType.Code)) return (IComplexGraphType)GraphTypeMap[entityType.Code];

            var graphType = NewGraphType(entityType)
                .EnsureInterface(entityType)
                .EnsureServiceAttributes(entityType)
                .EnsureAttributes(entityType)
                .EnsureRelations(entityType);

            return graphType;
        }

        public static IComplexGraphType NewGraphType(EntityType entityType)
        {
            var graphType = entityType.IsAbstract
                ? new InterfaceGraphType { Name = entityType.Code } as IComplexGraphType
                : new ObjectGraphType { Name = entityType.Code } as IComplexGraphType;

            GraphTypeMap[entityType.Code] = graphType;

            return graphType;
        }

        public static IComplexGraphType EnsureInterface(this IComplexGraphType graphType, EntityType entityType)
        {
            if (entityType.Parent == null) return graphType;

            var objectType = (IObjectGraphType)graphType;
            var abstractType = (IInterfaceGraphType)ToGraphType(entityType.Parent);
            objectType.AddFields(abstractType.Fields);
            objectType.AddResolvedInterface(abstractType);
            objectType.IsTypeOf = _ => true;

            return graphType;
        }

        public static IComplexGraphType EnsureServiceAttributes(this IComplexGraphType graphType, EntityType entityType)
        {
            if (!graphType.HasField(nameof(EntityType.Id)))
                graphType.AddField(new FieldType { Name = nameof(EntityType.Id), ResolvedType = new IntGraphType() });

            return graphType;
        }

        public static IComplexGraphType EnsureAttributes(this IComplexGraphType graphType, EntityType entityType)
        {
            return graphType.AddFields(entityType.EntityTypeAttributes.Select(attribute =>
                new FieldType { Name = attribute.Attribute.Code, ResolvedType = GraphTypeMap[attribute.Attribute.Type] }));
        }

        public static IComplexGraphType EnsureRelations(this IComplexGraphType graphType, EntityType entityType)
        {
            var relationGroups = entityType.ForwardRelationRestrictions.GroupBy(r => r.RelationType.Code);

            foreach (var group in relationGroups)
            {
                if (graphType.HasField(group.Key)) continue;

                var main = group.First();
                var type = default(IGraphType);

                if (group.Count() > 1 && !main.IsMultiple)
                {
                    // todo: camelize?
                    var unionType = new UnionGraphType { Name = $"{entityType.Code}{group.Key}RelationUnion" };
                    foreach (var item in group) unionType.AddPossibleType((IObjectGraphType)ToGraphType(item.Target));
                    type = unionType;
                }
                else if (main.IsMultiple) type = new ListGraphType(ToGraphType(main.Target));
                else type = ToGraphType(main.Target);

                // todo: pluralize
                graphType.AddField(new FieldType { Name = group.Key, ResolvedType = type });
            }

            return graphType;
        }

        public static IComplexGraphType AddFields(this IComplexGraphType graphType, IEnumerable<FieldType> fields)
        {
            foreach (var field in fields) graphType.AddField(field);

            return graphType;
        }

        private static FieldType CreateRelationField()
        {
            var relation = new ObjectGraphType { Name = "Relation" };
            relation.AddField(new FieldType { Name = nameof(Relation.Id), ResolvedType = new IntGraphType() });
            // all except DeletedAt
            return new FieldType
            {
                Name = "_relation",
                ResolvedType = relation
            };
        }
    }
}
