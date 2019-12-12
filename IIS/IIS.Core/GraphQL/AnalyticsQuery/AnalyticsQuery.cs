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
        public Guid Id { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }
        public string Description { get; set; }

        [GraphQLNonNullType]
        public DateTime CreatedAt { get; set; }

        [GraphQLType(typeof(ListType<NonNullType<ObjectType<DateRange>>>))]
        public IEnumerable<DateRange> DateRanges { get; set; }

        [GraphQLNonNullType]
        public DateTime UpdatedAt { get; set; }
        private IIS.Core.Analytics.EntityFramework.AnalyticsQuery _query { get; set; }
        private IEnumerable<IIS.Core.Analytics.EntityFramework.AnalyticsQueryIndicator> _indicators { get; set; }

        public AnalyticsQuery(IIS.Core.Analytics.EntityFramework.AnalyticsQuery query)
        {
            Id = query.Id;
            Title = query.Title;
            Description = query.Description;
            CreatedAt = query.CreatedAt;
            UpdatedAt = query.UpdatedAt;
            DateRanges = query.DateRanges != null ? query.DateRanges.Select(r => new DateRange(r)) : null;
            _query = query;
        }

        [GraphQLNonNullType]
        public async Task<User> GetCreator([Service] OntologyContext context)
        {
            var user = await context.Users.FindAsync(_query.CreatorId);
            return new User(user);
        }

        [GraphQLNonNullType]
        public async Task<User> GetLastUpdater([Service] OntologyContext context)
        {
            var user = await context.Users.FindAsync(_query.LastUpdaterId);
            return new User(user);
        }

        [GraphQLType(typeof(NonNullType<ListType<NonNullType<ObjectType<AnalyticsQueryIndicator>>>>))]
        public async Task<IEnumerable<AnalyticsQueryIndicator>> GetIndicators([Service] OntologyContext context)
        {
            var indicators = await _getQueryIndicators(context);
            return indicators.Select(i => new AnalyticsQueryIndicator(i));
        }

        public async Task<AnalyticsIndicator.AnalyticsIndicator> GetRootIndicator([Service] OntologyContext context, [Service] IIS.Core.Analytics.EntityFramework.IAnalyticsRepository repository)
        {
            var queryIndicators = await _getQueryIndicators(context);
            var queryIndicator = _indicators.FirstOrDefault();

            if (queryIndicator == null)
                return null;

            var rootIndicator = await repository.getRootAsync(queryIndicator.IndicatorId);

            return new AnalyticsIndicator.AnalyticsIndicator(rootIndicator);
        }

        private async Task<IEnumerable<IIS.Core.Analytics.EntityFramework.AnalyticsQueryIndicator>> _getQueryIndicators(OntologyContext context)
        {
            if (_indicators == null)
            {
                _indicators = await context.AnalyticsQueryIndicators
                    .Include(i => i.Indicator)
                    .Where(i => i.QueryId == Id)
                    .OrderBy(i => i.SortOrder)
                    .ToListAsync();
            }

            return _indicators;
        }
    }

    public class DateRange
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public int Id { get; set; }

        [GraphQLNonNullType]
        public DateTime StartDate { get; set; }

        [GraphQLNonNullType]
        public DateTime EndDate { get; set; }

        [GraphQLNonNullType]
        public string Color { get; set; }

        public DateRange(IIS.Core.Analytics.EntityFramework.AnalyticsQuery.DateRange range)
        {
            Id = range.Id;
            StartDate = range.StartDate;
            EndDate = range.EndDate;
            Color = range.Color;
        }
    }
}