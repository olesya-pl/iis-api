using System.Threading.Tasks;
using GraphQL.Types;

namespace IIS.Search.GraphQL
{
    public interface IGraphQLSchemaProvider
    {
        Task<ISchema> GetSchemaAsync();
    }
}