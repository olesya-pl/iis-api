using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Types;
using IIS.Core;

namespace IIS.Ontology.GraphQL
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
            var query = (IObjectGraphType)CreateType(type);
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
        }

        private IGraphType CreateType(TypeEntity type)
        {
            if (ComplexTypes.ContainsKey(type.Name)) return ComplexTypes[type.Name];
            var graphType = CreateComplexType(type);
            if (type.HasParent)
            {
                var derived = (IObjectGraphType)graphType;
                var abstractType = (IInterfaceGraphType)CreateType(type.Parent);
                derived.AddResolvedInterface(abstractType);
                derived.IsTypeOf = relation => ((Entity)((Relation)relation).Target).IsTypeOf(type);
            }
            foreach (var constraintName in type.ConstraintNames)
            {
                var constraint = type.GetConstraint(constraintName);
                var targetGraphType = default(IGraphType);
                if (type.HasAttribute(constraintName))
                {
                    targetGraphType = CreateAttribute(constraint);
                }
                else if (type.HasEntity(constraintName))
                {
                    var target = type.GetEntity(constraintName);
                    targetGraphType = CreateType(target);
                }
                else if (type.HasUnion(constraintName))
                {
                    var target = type.GetUnion(constraintName);
                    var unionGraphType = new UnionGraphType { Name = target.Name };
                    foreach (var item in target.Classes)
                    {
                        var unionPart = (IObjectGraphType)CreateType(item);
                        unionGraphType.AddPossibleType(unionPart);
                    }
                    targetGraphType = unionGraphType;
                }
                var field = CreateFieldType(constraint, targetGraphType);
                graphType.AddField(field);
            }
            return graphType;
        }

        private IComplexGraphType CreateComplexType(TypeEntity type)
        {
            if (type.IsAbstract) return CreateInterfaceType(type);
            return CreateObjectType(type);
        }

        private static IInterfaceGraphType CreateInterfaceType(TypeEntity type)
        {
            var graphType = new InterfaceGraphType { Name = type.Name };
            ComplexTypes[type.Name] = graphType;
            return graphType;
        }

        private static IObjectGraphType CreateObjectType(TypeEntity type)
        {
            var graphType = new ObjectGraphType { Name = type.Name };
            ComplexTypes[type.Name] = graphType;
            return graphType;
        }

        private static IGraphType CreateAttribute(Constraint constraint)
        {
            var attributeType = ((AttributeClass)constraint.Target).Type;
            var resolvedType = constraint.IsArray
                ? (IGraphType)ComplexTypes[(string)attributeType]
                : ScalarTypes[attributeType];
            return resolvedType;
        }

        private static FieldType CreateFieldType(Constraint constraint, IGraphType resolvedType)
        {
            var isArray = constraint.IsArray;
            var isRequired = constraint.IsRequired;
            if (isRequired && !isArray) resolvedType = new NonNullGraphType(resolvedType);
            if (isArray) resolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(resolvedType)));
            var resolver = constraint.Resolver;
            var fieldType = new FieldType
            {
                Name = constraint.Name,
                Resolver = resolver == null ? null : new GenericAsyncResolver(resolver),
                ResolvedType = resolvedType
            };
            return fieldType;
        }
    }
}
