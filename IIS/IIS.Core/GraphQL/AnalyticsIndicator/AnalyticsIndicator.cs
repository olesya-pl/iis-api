using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.AnalyticsIndicator
{
    public class AnalyticsIndicator
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }

        [GraphQLNonNullType]
        public string Code { get; set; }

        [GraphQLType(typeof(IdType))]
        public string ParentId { get; set; }

        public AnalyticsIndicator(IIS.Core.Analytics.EntityFramework.AnalyticsIndicator indicator)
        {
            Id = indicator.Id.ToString();
            Title = indicator.Title;
            Code = indicator.Code;

            if (indicator.ParentId != null)
            {
                ParentId = indicator.ParentId.ToString();
            }
        }
    }
}