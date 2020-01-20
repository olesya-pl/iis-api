using System.IO;
using System.Threading.Tasks;
using ProtoBuf;

namespace IIS.Search.Schema
{
    public class SchemaRepository : ISchemaRepository
    {
        // todo: config
        private readonly string _schemaPath = Path.Combine("bin", "Debug", "schema.bin");

        public async Task<ComplexType> GetSchemaAsync()
        {
            using (var stream = File.OpenRead(_schemaPath))
            {
                var type = Serializer.Deserialize<ComplexType>(stream);
                return await Task.FromResult(type);
            }
        }

        public async Task SaveSchemaAsync(ComplexType type)
        {
            using (var stream = File.Create(_schemaPath)) Serializer.Serialize(stream, type);
            await Task.CompletedTask;
        }
    }
}
