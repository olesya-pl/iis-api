using System.Linq;
using System;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;

namespace IIS.Core.GraphQL.AnalyticsQuery
{
    public interface IAnalyticsQueryInput
    {
        string Title { get; set; }
        string Description { get; set; }
    }

    public class CreateAnalyticsQueryInput : IAnalyticsQueryInput
    {
        [GraphQLNonNullType]
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<CreateAnalyticsQueryIndicatorInput> Indicators { get; set; } = Enumerable.Empty<CreateAnalyticsQueryIndicatorInput>();
    }

    public class UpdateAnalyticsQueryInput : IAnalyticsQueryInput
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public AnalyticsQueryIndicatorsInput Indicators { get; set; }
    }

    public class AnalyticsQueryIndicatorsInput
    {
        public IEnumerable<CreateAnalyticsQueryIndicatorInput> Create { get; set; } = Enumerable.Empty<CreateAnalyticsQueryIndicatorInput>();

        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public IEnumerable<Guid> Delete { get; set; } = Enumerable.Empty<Guid>();
    }

    public class CreateAnalyticsQueryIndicatorInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid IndicatorId { get; set; }

        public string Title { get; set; }

        [GraphQLNonNullType]
        public int SortOrder { get; set; }
    }
}