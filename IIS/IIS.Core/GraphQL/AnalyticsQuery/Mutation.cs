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
using Microsoft.EntityFrameworkCore;

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
            await _tryToToggleIndicators(context, query, data.Indicators);
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

        private async Task _tryToToggleIndicators(OntologyContext ctx, Core.Analytics.EntityFramework.AnalyticsQuery query, AnalyticsQueryIndicatorsInput patch)
        {
            if (patch == null)
                return;

            _tryToAddIndicators(ctx, query, patch.Create);
            await _tryToUpdateIndicators(ctx, query, patch.Update);

            if (patch.Delete != null && patch.Delete.Any())
            {
                ctx.AnalyticsQueryIndicators.RemoveRange(ctx.AnalyticsQueryIndicators.Where(i => patch.Delete.Contains(i.Id)));
            }
        }

        private async Task _tryToUpdateIndicators(OntologyContext ctx, Core.Analytics.EntityFramework.AnalyticsQuery query, IEnumerable<UpdateAnalyticsQueryIndicatorInput> indicatorsInput)
        {
            if (indicatorsInput == null)
                return;

            var ids = indicatorsInput.Select(i => i.Id);
            var analyticsQueries = await ctx.AnalyticsQueryIndicators
                .Where(q => ids.Contains(q.Id))
                .ToDictionaryAsync(q => q.Id);

            foreach (var input in indicatorsInput)
            {
                analyticsQueries.TryGetValue(input.Id, out var analyticsQuery);

                if (analyticsQuery == null)
                    throw new InvalidOperationException($"Trying to update non existing analytics query with id '{input.Id}'");

                analyticsQuery.Title = input.Title ?? analyticsQuery.Title;
                analyticsQuery.SortOrder = input.SortOrder ?? analyticsQuery.SortOrder;
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
