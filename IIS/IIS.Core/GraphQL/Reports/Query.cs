using HotChocolate;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Reports
{
    public class Query
    {
        [GraphQLNonNullType]
        public async Task<GraphQLCollection<Report>> GetReportList([Service] OntologyContext context, [GraphQLNonNullType] PaginationInput pagination)
        {
            var result = await context.Reports.Skip(pagination.Offset()).Take(pagination.Page).Select(row => new Report(row.Id, row.Title, row.Recipient, row.CreatedAt)).ToListAsync();
            return new GraphQLCollection<Report>(result);
        }
    }
}
