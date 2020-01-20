using System;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;
using System.Threading.Tasks;
using IIS.Core.Analytics.EntityFramework;

namespace IIS.Core.GraphQL.AnalyticsIndicator
{
    public class AnalyticsIndicator
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }

        [GraphQLType(typeof(IdType))]
        public Guid? ParentId { get; set; }

        private readonly Iis.DataModel.Analytics.AnalyticIndicatorEntity _indicator;

        public AnalyticsIndicator(Iis.DataModel.Analytics.AnalyticIndicatorEntity indicator)
        {
            Id = indicator.Id;
            Title = indicator.Title;

            if (indicator.ParentId != null)
            {
                ParentId = indicator.ParentId;
            }

            _indicator = indicator;
        }

        [GraphQLType(typeof(AnyType))]
        public async Task<IEnumerable<AnalyticsQueryIndicatorResult>> GetValues([Service] IAnalyticsRepository repository, DateTime? from, DateTime? to)
        {
            if (_indicator.Query == null)
                return null;

            return await repository.calcAsync(_indicator, from, to);
        }
    }

    public class AnalyticsIndicatorValue
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid EntityId { get; set; }

        [GraphQLType(typeof(NonNullType<AnyType>))]
        public string Value { get; set; }

        public AnalyticsIndicatorValue(AnalyticsQueryIndicatorResult result)
        {
            EntityId = result.Id;
            Value = result.Value.ToString();
        }
    }
}