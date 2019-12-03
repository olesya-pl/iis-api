using System.Linq;
using System;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.AnalyticsQuery
{
    public abstract class AnalyticsQueryInput
    {
        public virtual string Title { get; set; }
        public string Description { get; set; }

        [GraphQLType(typeof(ListType<NonNullType<InputObjectType<DateRangeInput>>>))]
        public IEnumerable<DateRangeInput> DateRanges { get; set; } = Enumerable.Empty<DateRangeInput>();
    }

    public class DateRangeInput
    {
        [GraphQLNonNullType]
        public DateTime StartDate { get; set; }

        [GraphQLNonNullType]
        public DateTime EndDate { get; set; }

        [GraphQLNonNullType]
        public string Color { get; set; }
    }

    public class CreateAnalyticsQueryInput : AnalyticsQueryInput
    {
        [GraphQLNonNullType]
        public override string Title { get; set; }
        public IEnumerable<CreateAnalyticsQueryIndicatorInput> Indicators { get; set; } = Enumerable.Empty<CreateAnalyticsQueryIndicatorInput>();
    }

    public class UpdateAnalyticsQueryInput : AnalyticsQueryInput
    {
        public AnalyticsQueryIndicatorsInput Indicators { get; set; }
    }

    public class AnalyticsQueryIndicatorsInput
    {
        public IEnumerable<CreateAnalyticsQueryIndicatorInput> Create { get; set; } = Enumerable.Empty<CreateAnalyticsQueryIndicatorInput>();

        public IEnumerable<UpdateAnalyticsQueryIndicatorInput> Update { get; set; } = Enumerable.Empty<UpdateAnalyticsQueryIndicatorInput>();

        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public IEnumerable<Guid> Delete { get; set; } = Enumerable.Empty<Guid>();
    }

    public class CreateAnalyticsQueryIndicatorInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid IndicatorId { get; set; }

        public string Title { get; set; }

        public int? SortOrder { get; set; }
    }

    public class UpdateAnalyticsQueryIndicatorInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }

        public string Title { get; set; }

        public int? SortOrder { get; set; }
    }
}