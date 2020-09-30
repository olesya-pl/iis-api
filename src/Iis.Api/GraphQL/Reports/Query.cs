using HotChocolate;
using HotChocolate.Types;
using Iis.Api.GraphQL.Common;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Reports
{
    public class Query
    {
        [GraphQLNonNullType]
        public async Task<GraphQLCollection<Report>> GetReportList([Service] IReportService reportService, [GraphQLNonNullType] PaginationInput pagination, SortingInput sorting)
        {
            var (count, items) = await reportService.GetReportPageAsync(pagination.PageSize, pagination.Offset());
            return new GraphQLCollection<Report>(items.Select(x => new Report(x)).ToList(), count);
        }

        [GraphQLType(typeof(ReportType))]
        public async Task<Report> GetReport([Service] IReportService reportService, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var report = await reportService.GetAsync(id);

            if(report is null) 
                return null;
            
            return new Report(report);
        }
    }
}
