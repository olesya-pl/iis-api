using System;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Api.Export;

namespace IIS.Core.GraphQL.Export
{
    public class ExportQuery
    {
        [GraphQLNonNullType]
        public async Task<byte[]> ExportNode(
            [Service]ExportService exportService,
            [GraphQLNonNullType] Guid id)
        {
            return await exportService.ExportNodeAsync(id);
        }
    }
}