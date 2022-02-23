using HotChocolate;

namespace IIS.Core.GraphQL
{
    public interface ISchemaProvider
    {
        void ConfigureSchema(ISchemaBuilder schemaBuilder);
    }
}