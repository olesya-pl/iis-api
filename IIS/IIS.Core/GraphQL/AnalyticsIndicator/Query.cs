using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

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

        public async Task<GraphQLCollection<AnalyticsIndicator>> GetAnalyticsIndicatorList([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid parentId)
        {
            // TODO: better to use QueryBuilder because escaping is Postgres specific
            var list = await context.AnalyticsIndicators
                .FromSql(@"
                    WITH RECURSIVE children AS (
                        SELECT *
                        FROM ""AnalyticsIndicators""
                        WHERE ""ParentId"" = {0}
                        UNION
                        SELECT i.*
                        FROM ""AnalyticsIndicators"" i
                          INNER JOIN children c ON c.""Id"" = i.""ParentId""
                    )
                    SELECT * FROM children
                ", parentId)
                .Select(i => new AnalyticsIndicator(i))
                .ToListAsync();

            return new GraphQLCollection<AnalyticsIndicator>(list, list.Count);
        }
    }
}