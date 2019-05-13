using System.Threading.Tasks;
using GraphQL.Types;

namespace IIS.GraphQL
{
    public interface IGraphQLSchemaProvider
    {
        Task<ISchema> GetSchemaAsync();
    }
}
