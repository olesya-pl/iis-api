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
        public async Task<GraphQLCollection<Report>> GetReportList([Service] IReportElasticService reportElasticService, [GraphQLNonNullType] PaginationInput pagination, SortingInput sorting)
        {
            var (count, items) = await reportElasticService.SearchAsync(pagination.PageSize, pagination.Offset(), sorting?.ColumnName, sorting?.Order);
            return new GraphQLCollection<Report>(items.Select(x => new Report(x)).ToList(), count);
        }

        [GraphQLType(typeof(ReportType))]
        public async Task<Report> GetReport([Service] IReportElasticService reportElasticService, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var report = await reportElasticService.GetAsync(id);

            if(report is null) 
                return null;
            
            return new Report(report);
        }
    }
}
