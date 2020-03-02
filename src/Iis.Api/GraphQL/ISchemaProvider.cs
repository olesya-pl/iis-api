using HotChocolate;

namespace IIS.Core.GraphQL
{
    public interface ISchemaProvider
    {
        ISchema GetSchema();
        void RecreateSchema();
    }
}
