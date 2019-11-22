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
            _tryToToggleIndicators(context, query, data);

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
            _tryToToggleIndicators(context, query, data);
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

        private void _tryToToggleIndicators(OntologyContext ctx, Core.Analytics.EntityFramework.AnalyticsQuery query,  IAnalyticsQueryInput data)
        {
            var ToRemove = new HashSet<Guid>(data.RemoveIndicators);

            // TODO: validate if addIndicators has the same root Indicator

            foreach (var input in data.AddIndicators)
            {
                if (ToRemove.Contains(input.Id))
                    throw new InvalidOperationException($"\"removeIndicators\" contains ids from \"addIndicators\" array");

                ctx.AnalyticsQueryIndicators.Add(new Core.Analytics.EntityFramework.AnalyticsQueryIndicator
                {
                    IndicatorId = input.Id,
                    QueryId = query.Id,
                    Title = input.Title,
                    SortOrder = input.SortOrder
                });
            }

            if (ToRemove.Any())
            {
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
