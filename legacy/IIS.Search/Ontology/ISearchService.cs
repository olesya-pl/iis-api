using System.Threading.Tasks;
using IIS.Search.Schema;
using Newtonsoft.Json.Linq;

namespace IIS.Search.Ontology
{
    public interface ISearchService
    {
        Task<JObject> SearchAsync(string typeName, string query);

        void IndexEntity(string message);

        Task SaveSchemaAsync(ComplexType schema);
    }
}
