using HotChocolate.Resolvers;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.Entities.InputTypes;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.Entities
{
    public static class InputExtensions
    {
        public static ElasticFilter CreateNodeFilter(this IResolverContext ctx)
        {
            var pagination = ctx.Argument<PaginationInput>("pagination");
            var filter = ctx.Argument<FilterInput>("filter");
            return new ElasticFilter {
                Limit = pagination.PageSize,
                Offset = pagination.Offset(),
                Suggestion = filter?.Suggestion?.Trim() ?? filter?.SearchQuery?.Trim()
            };
        }

        public static ElasticFilter CreateNodeFilter(this IResolverContext ctx, INodeTypeLinked criteriaType)
        {
            var result = ctx.CreateNodeFilter();
            return result;
        }
    }
}
