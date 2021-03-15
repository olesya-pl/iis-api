using Iis.Interfaces.Ontology.Schema;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IOntologySchemaService
    {
        IOntologySchema GetOntologySchema(IOntologySchemaSource schemaSource);
        IOntologySchema LoadFromDatabase(IOntologySchemaSource schemaSource);
        IOntologySchema LoadFromFile(IOntologySchemaSource schemaSource);
        void SaveToDatabase(IOntologySchema schema, string connectionString);
        void SaveToFile(IOntologySchema ontologySchema, string fileName);
    }
}