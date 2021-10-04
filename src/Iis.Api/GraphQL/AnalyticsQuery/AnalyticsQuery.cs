using System;
using HotChocolate;
using HotChocolate.Types;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Iis.DataModel;
using User = IIS.Core.GraphQL.Users.User;
using AutoMapper;

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
        private Iis.DataModel.Analytics.AnalyticQueryEntity _query { get; set; }
        private IEnumerable<Iis.DataModel.Analytics.AnalyticQueryIndicatorEntity> _indicators { get; set; }

        public AnalyticsQuery(Iis.DataModel.Analytics.AnalyticQueryEntity query)
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
        public async Task<User> GetCreator([Service] OntologyContext context, [Service] IMapper mapper)
        {
            var user = await context.Users.FindAsync(_query.CreatorId);
            return mapper.Map<User>(user);
        }

        [GraphQLNonNullType]
        public async Task<User> GetLastUpdater([Service] OntologyContext context, [Service] IMapper mapper)
        {
            var user = await context.Users.FindAsync(_query.LastUpdaterId);
            return mapper.Map<User>(user);
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

        private async Task<IEnumerable<Iis.DataModel.Analytics.AnalyticQueryIndicatorEntity>> _getQueryIndicators(OntologyContext context)
        {
            if (_indicators == null)
            {
                _indicators = await context.AnalyticQueryIndicators
                    .Include(i => i.Indicator)
                    .Include(i => i.Query)
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

        public string Color { get; set; }

        public DateRange(Iis.DataModel.Analytics.AnalyticQueryEntity.DateRange range)
        {
            Id = range.Id;
            StartDate = range.StartDate;
            EndDate = range.EndDate;
            Color = range.Color;
        }
    }
}