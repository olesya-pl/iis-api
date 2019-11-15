using HotChocolate;

namespace IIS.Core.GraphQL.AnalyticsQuery
{
    public class AnalyticsQueryInput
    {
        [GraphQLNonNullType]
        public string Title { get; set; }
        public string Description { get; set; }
    }
}