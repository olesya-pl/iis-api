using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.AnalyticsQuery
{
    public class Mutation
    {
        private IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Mutation(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AnalyticsQuery> CreateAnalyticsQuery(IResolverContext ctx, [Service] OntologyContext context, [GraphQLNonNullType] CreateAnalyticsQueryInput data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);

            var tokenPayload = ctx.ContextData["token"] as TokenPayload;
            var query = new Core.Analytics.EntityFramework.AnalyticsQuery {
                Id = Guid.NewGuid(),
                CreatorId = tokenPayload.UserId,
                LastUpdaterId = tokenPayload.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _mapInputToModel(data, query);
            _tryToAddIndicators(context, query, data.Indicators);

            context.AnalyticsQuery.Add(query);
            await context.SaveChangesAsync();
            return new AnalyticsQuery(query);
        }

        public async Task<AnalyticsQuery> UpdateAnalyticsQuery(IResolverContext ctx, [Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id, [GraphQLNonNullType] UpdateAnalyticsQueryInput data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var query = await context.AnalyticsQuery.FindAsync(id);
            if (query == null)
                throw new InvalidOperationException($"Cannot find analytics query with id = {id}");

            var tokenPayload = ctx.ContextData["token"] as TokenPayload;
            _mapInputToModel(data, query);
            _tryToToggleIndicators(context, query, data.Indicators);
            query.LastUpdaterId = tokenPayload.UserId;
            await context.SaveChangesAsync();

            return new AnalyticsQuery(query);
        }

        private void _mapInputToModel(IAnalyticsQueryInput data, Core.Analytics.EntityFramework.AnalyticsQuery query)
        {
            query.UpdatedAt = DateTime.UtcNow;

            if (data.Title != null)
            {
                query.Title = data.Title;
            }

            if (data.Description != null)
            {
                query.Description = data.Description;
            }
        }

        private void _tryToAddIndicators(OntologyContext ctx, Core.Analytics.EntityFramework.AnalyticsQuery query,  IEnumerable<CreateAnalyticsQueryIndicatorInput> indicators)
        {
            if (indicators == null)
            {
                return;
            }

            _addQueryIndicators(ctx, query.Id, indicators);
        }

        private void _addQueryIndicators(OntologyContext ctx, Guid queryId, IEnumerable<CreateAnalyticsQueryIndicatorInput> indicators)
        {
            foreach (var input in indicators)
            {
                ctx.AnalyticsQueryIndicators.Add(new Core.Analytics.EntityFramework.AnalyticsQueryIndicator
                {
                    IndicatorId = input.IndicatorId,
                    QueryId = queryId,
                    Title = input.Title,
                    SortOrder = input.SortOrder
                });
            }
        }

        private void _tryToToggleIndicators(OntologyContext ctx, Core.Analytics.EntityFramework.AnalyticsQuery query, AnalyticsQueryIndicatorsInput data)
        {
            _tryToAddIndicators(ctx, query, data.Create);

            if (data.Delete != null && data.Delete.Any())
            {
                var ToRemove = new HashSet<Guid>(data.Delete);
                ctx.AnalyticsQueryIndicators.RemoveRange(ctx.AnalyticsQueryIndicators.Where(i => ToRemove.Contains(i.Id)));
            }
        }

        public async Task<AnalyticsQuery> DeleteAnalyticsQuery([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            var query = await context.AnalyticsQuery.FindAsync(id);
            if (query == null)
                throw new InvalidOperationException($"Analytics query with id = {id} not found");
            context.AnalyticsQuery.Remove(query);
            await context.SaveChangesAsync();

            return new AnalyticsQuery(query);
        }
    }
}
