using System;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Api.Export;

namespace IIS.Core.GraphQL.Export
{
    public class ExportQuery
    {
        [GraphQLNonNullType]
        public Task<byte[]> ExportNode(
            [Service]ExportService exportService,
            [GraphQLNonNullType] Guid id)
        {
            return exportService.ExportNodeAsync(id);
        }

        public Task<string> ExportNodeToJson(
            [Service]ExportToJsonService exportService,
            [GraphQLNonNullType] Guid id)
        {
            return exportService.ExportNodeAsync(id);
        }
    }
}