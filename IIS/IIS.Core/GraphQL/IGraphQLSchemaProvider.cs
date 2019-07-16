using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Types;

namespace IIS.Core.GraphQL
{
    public interface IGraphQLSchemaProvider
    {
        Task<ISchema> GetSchemaAsync(CancellationToken cancellationToken = default);
    }
}
