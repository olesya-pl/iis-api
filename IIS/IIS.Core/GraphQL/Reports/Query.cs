using HotChocolate;
using HotChocolate.Types;
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
            var query = context.Reports.Skip(pagination.Offset()).Take(pagination.PageSize).Include(r => r.ReportEvents);
            var result = await query.Select(row => new Report(row)).ToListAsync();
            var totalCount = await context.Reports.CountAsync();
            return new GraphQLCollection<Report>(result, totalCount);
        }

        [GraphQLType(typeof(ReportType))]
        public async Task<Report> GetReport([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid reportId)
        {
            var dbReport = await context.Reports.Include(r => r.ReportEvents).SingleOrDefaultAsync(r => r.Id == reportId);
            return new Report(dbReport);
        }
    }
}
