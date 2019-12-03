using System;
using System.Linq;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using HotChocolate.Resolvers;
using System.Threading.Tasks;
using IIS.Core.Analytics.EntityFramework;
using IIS.Core.GraphQL.AnalyticsIndicator;

namespace IIS.Core.GraphQL.AnalyticsQuery
{
    public class AnalyticsQueryIndicator
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }

        [GraphQLNonNullType]
        public AnalyticsIndicator.AnalyticsIndicator Indicator { get; set; }
        private readonly IIS.Core.Analytics.EntityFramework.AnalyticsQueryIndicator _queryIndicator;

        public AnalyticsQueryIndicator(IIS.Core.Analytics.EntityFramework.AnalyticsQueryIndicator queryIndicator)
        {
            Id = queryIndicator.Id.ToString();
            Title = queryIndicator.Title ?? queryIndicator.Indicator.Title;
            Indicator = new AnalyticsIndicator.AnalyticsIndicator(queryIndicator.Indicator);
            _queryIndicator = queryIndicator;
        }

        public async Task<IEnumerable<AnalyticsIndicatorDateRangeValue>> GetQueryValues([Service] IAnalyticsRepository repository, IResolverContext ctx)
        {
            var (query, indicator) = (_queryIndicator.Query, _queryIndicator.Indicator);
            var list = new List<AnalyticsIndicatorDateRangeValue>();

            // TODO: improve analytics query builder to support "OR" conditions, so then everything can be made in single db hop
            foreach (var dateRange in query.DateRanges)
            {
                var valuesForDateRange = await repository.calcAsync(indicator, dateRange.StartDate, dateRange.EndDate);
                var item = new AnalyticsIndicatorDateRangeValue {
                    DateRange = new DateTime[] { dateRange.StartDate, dateRange.EndDate },
                    Values = valuesForDateRange.Select(value => new AnalyticsIndicatorValue(value))
                };
                list.Add(item);
            }

            return list;
        }
    }
    public class AnalyticsIndicatorDateRangeValue
    {
        public IEnumerable<DateTime> DateRange { get; set; }
        public IEnumerable<AnalyticsIndicatorValue> Values { get; set; } = new List<AnalyticsIndicatorValue>();
    }
}