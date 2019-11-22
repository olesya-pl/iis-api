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
        IEnumerable<AnalyticsQueryIndicatorInput> AddIndicators { get; set; }
        IEnumerable<Guid> RemoveIndicators { get; set; }
    }

    public class CreateAnalyticsQueryInput : IAnalyticsQueryInput
    {
        [GraphQLNonNullType]
        public string Title { get; set; }
        public string Description { get; set; }

        public IEnumerable<AnalyticsQueryIndicatorInput> AddIndicators { get; set; } = Enumerable.Empty<AnalyticsQueryIndicatorInput>();

        [GraphQLType(typeof(ListType<NonNullType<IdType>>))]
        public IEnumerable<Guid> RemoveIndicators { get; set; } = Enumerable.Empty<Guid>();
    }

    public class UpdateAnalyticsQueryInput : IAnalyticsQueryInput
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public IEnumerable<AnalyticsQueryIndicatorInput> AddIndicators { get; set; } = Enumerable.Empty<AnalyticsQueryIndicatorInput>();

        public IEnumerable<Guid> RemoveIndicators { get; set; } = Enumerable.Empty<Guid>();
    }

    public class AnalyticsQueryIndicatorInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }

        public string Title { get; set; }

        public int SortOrder { get; set; }
    }
}