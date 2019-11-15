using System;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Users;
using IIS.Core.Ontology.EntityFramework.Context;
using System.Threading.Tasks;

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

        public AnalyticsQuery(IIS.Core.Analytics.EntityFramework.AnalyticsQuery query)
        {
            Id = query.Id.ToString();
            Title = query.Title;
            Description = query.Description;
            CreatedAt = query.CreatedAt;
            UpdatedAt = query.UpdatedAt;
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
    }
}
