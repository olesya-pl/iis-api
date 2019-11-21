using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using IIS.Core.Analytics.EntityFramework;

namespace IIS.Core.GraphQL.AnalyticsIndicator
{
    public class Query
    {
        public async Task<GraphQLCollection<AnalyticsIndicator>> GetAnalyticsRootIndicatorList([Service] OntologyContext context, [GraphQLNonNullType] PaginationInput pagination)
        {
            var list = context.AnalyticsIndicators
                .Where(i => i.ParentId == null)
                .GetPage(pagination)
                .Select(i => new AnalyticsIndicator(i));
            return new GraphQLCollection<AnalyticsIndicator>(await list.ToListAsync(), await list.CountAsync());
        }

        public async Task<GraphQLCollection<AnalyticsIndicator>> GetAnalyticsIndicatorList([Service] IAnalyticsRepository repository, [GraphQLType(typeof(NonNullType<IdType>))] Guid parentId)
        {
            var children = await repository.GetAllChildrenAsync(parentId);
            var list = children.Select(i => new AnalyticsIndicator(i)).ToList();

            return new GraphQLCollection<AnalyticsIndicator>(list, list.Count);
        }
    }
}