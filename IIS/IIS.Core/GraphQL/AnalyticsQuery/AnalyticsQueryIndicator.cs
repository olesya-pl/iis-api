using System;
using HotChocolate;
using HotChocolate.Types;
using System.Threading.Tasks;
using IIS.Core.Analytics.EntityFramework;

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

        public AnalyticsQueryIndicator(IIS.Core.Analytics.EntityFramework.AnalyticsQueryIndicator queryIndicator)
        {
            Id = queryIndicator.Id.ToString();
            Title = queryIndicator.Title ?? queryIndicator.Indicator.Title;
            Indicator = new AnalyticsIndicator.AnalyticsIndicator(queryIndicator.Indicator);
        }

        public AnalyticsQueryIndicator(IIS.Core.Analytics.EntityFramework.AnalyticsIndicator indicator)
        {
            Id = indicator.Id.ToString();
            Title = indicator.Title;
            Indicator = new AnalyticsIndicator.AnalyticsIndicator(indicator);
        }
    }
}