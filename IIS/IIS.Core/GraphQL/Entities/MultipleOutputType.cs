using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities
{
    public class MultipleOutputType : ObjectType
    {
        public static string GetName(string scalarName) => $"MultipleOutput_{scalarName}";

        private string _scalarName;
        private IOutputType _outputType;

        public MultipleOutputType(string scalarName, IOutputType outputType)
        {
            _scalarName = scalarName;
            _outputType = outputType;
        }

        protected override void Configure(IObjectTypeDescriptor d)
        {
            d.Name(GetName(_scalarName));
            d.Field("id").Type<NonNullType<IdType>>()
                .ResolverNotImplemented();
            d.Field("value").Type(new NonNullType(_outputType))
                .ResolverNotImplemented();
        }
    }
}