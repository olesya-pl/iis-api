using HotChocolate.Types;
using IIS.Core.GraphQL.Entities.Resolvers;

namespace IIS.Core.GraphQL.Entities.ObjectTypes
{
    public class MultipleOutputType : ObjectType
    {
        private readonly IOutputType _outputType;

        private readonly string _scalarName;

        public MultipleOutputType(string scalarName, IOutputType outputType)
        {
            _scalarName = scalarName;
            _outputType = outputType;
        }

        public static string GetName(string scalarName)
        {
            return $"MultipleOutput_{scalarName}";
        }

        protected override void Configure(IObjectTypeDescriptor d)
        {
            d.Name(GetName(_scalarName));
            d.Field("id").Type<NonNullType<IdType>>()
                .Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveMultipleId(ctx));
            d.Field("value").Type(new NonNullType(_outputType))
                .Resolver(ctx => ctx.Service<IOntologyQueryResolver>().ResolveMultipleAttributeRelationTarget(ctx));
        }
    }
}
