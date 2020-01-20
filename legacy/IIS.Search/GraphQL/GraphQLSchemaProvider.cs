using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using IIS.Search.Schema;
using GraphQLSchema = GraphQL.Types.Schema;

namespace IIS.Search.GraphQL
{
    public class GraphQLSchemaProvider : IGraphQLSchemaProvider
    {
        private readonly ISchemaProvider _schemaProvider;
        private readonly IDictionary<string, IFieldResolver> _resolvers;

        public GraphQLSchemaProvider(ISchemaProvider schemaProvider, IDictionary<string, IFieldResolver> resolvers)
        {
            _schemaProvider = schemaProvider;
            _resolvers = resolvers;
        }

        public async Task<ISchema> GetSchemaAsync()
        {
            var type = await _schemaProvider.GetSchemaAsync();
            var query = (IObjectGraphType)CreateType(type);

            foreach (var field in query.Fields)
            {
                field.Resolver = _resolvers["entities"];
                var argument = new QueryArgument(new StringGraphType()) { Name = "query" };
                field.Arguments = new QueryArguments(argument);
            }

            var schema = new GraphQLSchema { Query = query };
            schema.RegisterTypes(ComplexTypes.Values.ToArray());

            return schema;
        }

        private static readonly Dictionary<string, IGraphType> ComplexTypes = new Dictionary<string, IGraphType>();
        private static readonly Dictionary<ScalarType, ScalarGraphType> ScalarTypes = new Dictionary<ScalarType, ScalarGraphType>
        {
            [ScalarType.String] = new StringGraphType(),
            [ScalarType.Keyword] = new StringGraphType(),
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

        private IGraphType CreateType(ComplexType type)
        {
            if (ComplexTypes.ContainsKey(type.Name)) return ComplexTypes[type.Name];

            var graphType = default(IComplexGraphType);
            if (type.Kind == Kind.Class)
            {
                var objGraphType = new ObjectGraphType { Name = type.Name };
                foreach (var parent in type.Parents.Where(p => p.Kind == Kind.Interface))
                {
                    var abstractType = (IInterfaceGraphType)CreateType(parent);
                    //abstractType.AddPossibleType(objGraphType);
                    objGraphType.AddResolvedInterface(abstractType);
                }
                foreach (var parent in type.Parents.Where(p => p.Kind == Kind.Union))
                {
                    var abstractType = (IAbstractGraphType)CreateType(parent);
                    abstractType.AddPossibleType(objGraphType);
                }
                objGraphType.IsTypeOf = entity => true; ///////////
                graphType = objGraphType;
            }
            else if (type.Kind == Kind.ComplexAttribute)
            {
                graphType = new ObjectGraphType { Name = type.Name };
            }
            else if (type.Kind == Kind.Interface)
            {
                graphType = new InterfaceGraphType { Name = type.Name };
            }
            else if (type.Kind == Kind.Union)
            {
                var union = new UnionGraphType { Name = type.Name };
                ComplexTypes[type.Name] = union;
                return union;
            }

            ComplexTypes[type.Name] = graphType;

            foreach (var member in type.AllMembers) graphType.AddField(CreateField(member));

            return graphType;
        }

        private FieldType CreateField(Member member)
        {
            var resolvedType = default(IGraphType);
            var resolver = default(IFieldResolver);
            if (member.Membership == Membership.Type)
            {
                var typeMember = (ComplexType)member.Target;
                resolvedType = CreateType(typeMember);
                resolver = _resolvers["entityRelation"];
            }
            else if (member.Membership == Membership.Attribute)
            {
                var attributeMember = (Attribute)member.Target;
                var scalarType = attributeMember.ScalarType;
                resolvedType = member.IsArray ? ComplexTypes[scalarType] : ScalarTypes[(ScalarType)scalarType];
                resolver = _resolvers["attribute"];
            }
            if (member.IsRequired && !member.IsArray) resolvedType = new NonNullGraphType(resolvedType);
            if (member.IsArray) resolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(resolvedType)));

            return new FieldType { Name = member.Name, ResolvedType = resolvedType, Resolver = resolver };
        }
    }
}
