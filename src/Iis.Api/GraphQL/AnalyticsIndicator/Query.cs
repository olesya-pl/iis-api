using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using Microsoft.EntityFrameworkCore;
using IIS.Core.Analytics.EntityFramework;
using Iis.DataModel;

namespace IIS.Core.GraphQL.AnalyticsIndicator
{
    public class Query
    {
        public async Task<GraphQLCollection<AnalyticsIndicator>> GetAnalyticsRootIndicatorList([Service] OntologyContext context, [GraphQLNonNullType] PaginationInput pagination)
        {
            var list = context.AnalyticIndicators
                .Where(i => i.ParentId == null)
                .GetPage(pagination)
                .Select(i => new AnalyticsIndicator(i));
            return new GraphQLCollection<AnalyticsIndicator>(await list.ToListAsync(), await list.CountAsync());
        }

        public async Task<GraphQLCollection<AnalyticsIndicator>> GetAnalyticsIndicatorList([Service] IAnalyticsRepository repository, [GraphQLType(typeof(NonNullType<IdType>))] Guid parentId)
        {
            var children = repository.GetAllChildrenAsync(parentId);
            var list = children.Select(i => new AnalyticsIndicator(i)).ToList();

            return new GraphQLCollection<AnalyticsIndicator>(list, list.Count);
        }

        public async Task<AnalyticsIndicator> GetAnalyticsIndicator([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var indicator = await context.AnalyticIndicators.FindAsync(id);

            if (indicator == null)
                throw new InvalidOperationException($"Cannot find analytics indicator with id \"{id}\"");

            return new AnalyticsIndicator(indicator);
        }
    }
}