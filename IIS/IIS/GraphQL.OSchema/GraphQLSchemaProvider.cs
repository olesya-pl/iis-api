using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Types;
using IIS.OSchema;

namespace IIS.GraphQL.OSchema
{
    public class GraphQLSchemaProvider : IGraphQLSchemaProvider
    {
        private readonly IOSchema _oSchema;

        public GraphQLSchemaProvider(IOSchema oSchema)
        {
            _oSchema = oSchema;
        }

        public async Task<ISchema> GetSchemaAsync()
        {
            var type = await _oSchema.GetRootAsync();
            var query = (ObjectGraphType)Build(type);
            var schema = new Schema { Query = query };

            return schema;
        }

        private static readonly Dictionary<string, IComplexGraphType> ComplexTypes = new Dictionary<string, IComplexGraphType>();
        private static readonly Dictionary<ScalarType, ScalarGraphType> ScalarTypes = new Dictionary<ScalarType, ScalarGraphType>
        {
            [ScalarType.String] = new StringGraphType(),
            [ScalarType.Int] = new IntGraphType(),
            [ScalarType.Decimal] = new DecimalGraphType(),
            [ScalarType.Date] = new DateGraphType(),
            [ScalarType.Boolean] = new BooleanGraphType(),
            [ScalarType.Geo] = new StringGraphType(), // todo: custom scalar
            [ScalarType.File] = new StringGraphType(),// todo: custom object
            [ScalarType.Json] = new StringGraphType()
        };

        static GraphQLSchemaProvider()
        {
            foreach (var type in ScalarTypes.Keys)
            {
                var value = new ObjectGraphType { Name = type.Camelize() + "Value" };
                value.AddField(new FieldType { Name = "Id", ResolvedType = new NonNullGraphType(ScalarTypes[ScalarType.Int]) });
                value.AddField(new FieldType { Name = "Value", ResolvedType = new NonNullGraphType(ScalarTypes[type]) });

                ComplexTypes[(string)type] = value;
            }

            _graphTypeFactories.Add(TargetKind.Attribute, CreateAttribute);
            _graphTypeFactories.Add(TargetKind.Entity, CreateEntity);
            _graphTypeFactories.Add(TargetKind.Union, CreateUnion);
        }

        private static readonly Dictionary<TargetKind, Func<TypeEntity, string, IGraphType>> _graphTypeFactories =
            new Dictionary<TargetKind, Func<TypeEntity, string, IGraphType>>();

        public static IComplexGraphType Build(TypeEntity type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (ComplexTypes.ContainsKey(type.Name)) return ComplexTypes[type.Name];

            var graphType = NewGraphType(type);
            graphType = EnsureInterface(type, graphType);
            graphType = EnsureRelations(type, graphType);

            return graphType;
        }

        private static IComplexGraphType NewGraphType(TypeEntity type)
        {
            return ComplexTypes[type.Name] = type.IsAbstract
                ? new InterfaceGraphType { Name = type.Name } as IComplexGraphType
                : new ObjectGraphType { Name = type.Name } as IComplexGraphType;
        }

        private static IComplexGraphType EnsureInterface(TypeEntity type, IComplexGraphType graphType)
        {
            if (type.Parent == null) return graphType;

            var abstractType = (IInterfaceGraphType)Build(type.Parent);
            var objectType = (IObjectGraphType)graphType;
            objectType.AddResolvedInterface(abstractType);
            objectType.IsTypeOf = relation => ((Entity)((EntityRelation)relation).GetSingularTarget()).Type.Name == type.Name;

            return graphType;
        }

        private static IComplexGraphType EnsureRelations(TypeEntity type, IComplexGraphType graphType)
        {
            foreach (var constraint in type.Constraints) graphType.AddField(CreateField(type, constraint.Name));

            return graphType;
        }

        private static FieldType CreateField(TypeEntity type, string constraintName)
        {
            var childType = CreateGraphConstraint(type, constraintName);
            var resolver = type[constraintName].Resolver;
            var fieldResolver = resolver == null ? null : new GenericAsyncResolver(resolver);
            var field = new FieldType { Name = constraintName, ResolvedType = childType, Resolver = fieldResolver };
            return field;
        }

        private static IGraphType CreateGraphConstraint(TypeEntity type, string constraintName)
        {
            var constraint = type[constraintName];
            var isArray = constraint.IsArray;
            var isRequired = constraint.IsRequired;
            var childType = _graphTypeFactories[constraint.Kind](type, constraintName);

            if (isRequired && !isArray) childType = new NonNullGraphType(childType);
            if (isArray) childType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(childType)));

            return childType;
        }

        private static IGraphType CreateUnion(TypeEntity type, string constraintName)
        {
            // todo: camelize and nonNull for union parts
            var unionConstraint = type.GetUnion(constraintName);
            var union = new UnionGraphType { Name = $"{type.Name}{constraintName}RelationUnion" };
            var possibleTypes = unionConstraint.Targets
                .Select(item => (IObjectGraphType)Build(item));

            foreach (var item in possibleTypes) union.AddPossibleType(item);

            return union;
        }

        private static IGraphType CreateEntity(TypeEntity type, string constraintName)
        {
            var entityConstraint = type.GetEntity(constraintName);
            var entity = Build(entityConstraint.Target);

            return entity;
        }

        private static IGraphType CreateAttribute(TypeEntity type, string constraintName)
        {
            var attributeConstraint = type.GetAttribute(constraintName);
            var attributeType = attributeConstraint.Type;
            var attribute = attributeConstraint.IsArray
                ? (IGraphType)ComplexTypes[(string)attributeType]
                : ScalarTypes[attributeType];

            return attribute;
        }
    }
}
