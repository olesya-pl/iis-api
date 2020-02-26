using System;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Api.Export;

namespace IIS.Core.GraphQL.Export
{
    public class ExportQuery
    {
        [GraphQLNonNullType]
        public async Task<byte[]> ExportPerson(
            [Service]ExportService exportService,
            [GraphQLNonNullType] Guid personId)
        {
            return await exportService.ExportPersonAsync(personId);
        }
    }
}