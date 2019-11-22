using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.GraphQL.AnalyticsQuery
{
    public class Query
    {
        public async Task<AnalyticsQuery> GetAnalyticsQuery([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var query = await context.AnalyticsQuery
                .SingleOrDefaultAsync(q => q.Id == id);

            if (query == null)
                throw new InvalidOperationException($"Cannot find analytics query with id \"{id}\"");

            return new AnalyticsQuery(query);
        }

        public async Task<GraphQLCollection<AnalyticsQuery>> GetAnalyticsQueryList([Service] OntologyContext context, [GraphQLNonNullType] PaginationInput pagination)
        {
            var queries = context.AnalyticsQuery
                .GetPage(pagination)
                .Select(q => new AnalyticsQuery(q));

            return new GraphQLCollection<AnalyticsQuery>(await queries.ToListAsync(), await context.AnalyticsQuery.CountAsync());
        }
    }
}
