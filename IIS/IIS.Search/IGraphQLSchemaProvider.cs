using System.Threading.Tasks;
using GraphQL.Types;

namespace IIS.Search
{
    public interface IGraphQLSchemaProvider
    {
        Task<ISchema> GetSchemaAsync();
    }
}