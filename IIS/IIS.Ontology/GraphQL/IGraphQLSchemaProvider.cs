using System.Threading.Tasks;
using GraphQL.Types;

namespace IIS.Ontology.GraphQL
{
    public interface IGraphQLSchemaProvider
    {
        Task<ISchema> GetSchemaAsync();
    }
}
