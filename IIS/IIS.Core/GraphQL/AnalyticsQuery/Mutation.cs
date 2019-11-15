using HotChocolate;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
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

        public async Task<AnalyticsQuery> CreateAnalyticsQuery(IResolverContext ctx, [Service] OntologyContext context, [GraphQLNonNullType] AnalyticsQueryInput data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);

            var tokenPayload = ctx.ContextData["token"] as TokenPayload;
            var query = new Core.Analytics.EntityFramework.AnalyticsQuery {
                CreatorId = tokenPayload.UserId,
                LastUpdaterId = tokenPayload.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _mapInputToModel(data, query);

            context.AnalyticsQuery.Add(query);
            await context.SaveChangesAsync();
            return new AnalyticsQuery(query);
        }

        public async Task<AnalyticsQuery> UpdateAnalyticsQuery(IResolverContext ctx, [Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] string id, [GraphQLNonNullType] AnalyticsQueryInput data)
        {
            Validator.ValidateObject(data, new ValidationContext(data), true);
            var query = await context.AnalyticsQuery.FindAsync(Guid.Parse(id));
            if (query == null)
                throw new InvalidOperationException($"Cannot find analytics query with id = {id}");

            var tokenPayload = ctx.ContextData["token"] as TokenPayload;
            _mapInputToModel(data, query);
            query.LastUpdaterId = tokenPayload.UserId;
            await context.SaveChangesAsync();

            return new AnalyticsQuery(query);
        }

        private void _mapInputToModel(AnalyticsQueryInput data, Core.Analytics.EntityFramework.AnalyticsQuery query)
        {
            query.UpdatedAt = DateTime.UtcNow;
            query.Title = data.Title;

            if (data.Description != null)
            {
                query.Description = data.Description;
            }
        }

        public async Task<AnalyticsQuery> DeleteAnalyticsQuery([Service] OntologyContext context, [GraphQLType(typeof(NonNullType<IdType>))] Guid id)
        {
            //TODO: should not be able to delete yourself
            var query = await context.AnalyticsQuery.FindAsync(id);
            if (query == null)
                throw new InvalidOperationException($"Analytics query with id = {id} not found");
            context.AnalyticsQuery.Remove(query);
            await context.SaveChangesAsync();

            return new AnalyticsQuery(query);
        }
    }
}
