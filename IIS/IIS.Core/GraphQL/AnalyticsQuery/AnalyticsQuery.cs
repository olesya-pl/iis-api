using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Users;
using IIS.Core.Ontology.EntityFramework.Context;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace IIS.Core.GraphQL.AnalyticsQuery
{
    public class AnalyticsQuery
    {

        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }
        public string Description { get; set; }

        [GraphQLNonNullType]
        public DateTime CreatedAt { get; set; }

        [GraphQLNonNullType]
        public DateTime UpdatedAt { get; set; }

        private Guid _creatorId { get; set; }
        private Guid _lastUpdaterId { get; set; }
        public RootAnalyticsQueryIndicator RootIndicator { get; set; }

        public AnalyticsQuery(IIS.Core.Analytics.EntityFramework.AnalyticsQuery query)
        {
            Id = query.Id.ToString();
            Title = query.Title;
            Description = query.Description;
            CreatedAt = query.CreatedAt;
            UpdatedAt = query.UpdatedAt;
            RootIndicator = new RootAnalyticsQueryIndicator(query.RootIndicator);
            _creatorId = query.CreatorId;
            _lastUpdaterId = query.LastUpdaterId;
        }

        public async Task<User> GetCreator([Service] OntologyContext context)
        {
            var user = await context.Users.FindAsync(_creatorId);
            return new User(user);
        }

        public async Task<User> GetLastUpdater([Service] OntologyContext context)
        {
            var user = await context.Users.FindAsync(_lastUpdaterId);
            return new User(user);
        }

        public async Task<IEnumerable<AnalyticsQueryIndicator>> GetIndicators([Service] OntologyContext context)
        {
            var list = await context.AnalyticsQueryIndicators
                .Include(i => i.Indicator)
                .Where(i => i.QueryId.ToString() == Id)
                .OrderBy(i => i.SortOrder)
                .ToListAsync();

            return list.Select(i => new AnalyticsQueryIndicator(i));
        }
    }
}