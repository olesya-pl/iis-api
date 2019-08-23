using HotChocolate;

namespace IIS.Core.GraphQL
{
    public interface IGraphQlSchemaProvider
    {
        ISchema GetSchema();
    }
}
