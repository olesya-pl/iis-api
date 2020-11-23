using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.Resolvers;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Core.GraphQL.Entities.ObjectTypes.Mutations
{
    public class MutatorResponseType : ObjectType
    {
        private readonly string _name;
        private readonly IOutputType _ontologyType;

        private readonly Operation _operation;

        public MutatorResponseType(Operation operation, INodeTypeModel entityType, IOutputType ontologyType)
        {
            _operation = operation;
            _name = GetName(operation, entityType);
            _ontologyType = ontologyType;
        }

        public static string GetName(Operation operation, INodeTypeModel entityType)
        {
            return $"{operation}{OntologyObjectType.GetName(entityType)}Response";
        }

        protected override void Configure(IObjectTypeDescriptor d)
        {
            d.Name(_name);
            d.Field("type").Type<NonNullType<StringType>>()
                .Resolver(ctx => ctx.Service<IOntologyMutationResolver>().ResolveResponseType(ctx));
            d.Field("details").Type(new NonNullType(_ontologyType))
                .Resolver(ctx => ctx.Service<IOntologyMutationResolver>().ResolveResponseDetails(ctx));
        }
    }
}
