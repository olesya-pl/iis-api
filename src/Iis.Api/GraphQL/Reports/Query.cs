using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using Iis.Api.GraphQL.Common;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Reports
{
    public class Query
    {
        [GraphQLNonNullType]
        public async Task<GraphQLCollection<Report>> GetReportList(
            IResolverContext ctx,
            [Service] IReportElasticService reportElasticService, 
            [GraphQLNonNullType] PaginationInput pagination, 
            SortingInput sorting, 
            FilterInput filter)
        {
            var tokenPayload = ctx.ContextData[TokenPayload.TokenPropertyName] as TokenPayload;

            var (count, items) = await reportElasticService.SearchAsync(new ReportSearchParams
            {
                PageSize = pagination.PageSize,
                Offset = pagination.Offset(),
                SortColumn = sorting?.ColumnName,
                SortOrder = sorting?.Order,
                Suggestion = filter?.Suggestion
            }, tokenPayload.User);
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
