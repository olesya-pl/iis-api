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

        private readonly Guid _indicatorId;

        public AnalyticsQueryIndicator(IIS.Core.Analytics.EntityFramework.AnalyticsQueryIndicator queryIndicator)
        {
            Id = queryIndicator.Id.ToString();
            Title = queryIndicator.Title ?? queryIndicator.Indicator.Title;
            _indicatorId = queryIndicator.IndicatorId;
        }

        [GraphQLType(typeof(AnyType))]
        public async Task<string[]> GetValues()
        {
            // TODO: remove mocked data and execute query instead
            return new string[] {
                "5",
                "6",
                "7",
                "8"
            };
        }

        public async Task<AnalyticsIndicator.AnalyticsIndicator> GetRootIndicator([Service] IAnalyticsRepository repository)
        {
            // TODO: add dataloader support to reduce amount of queries
            var root = await repository.getRootAsync(_indicatorId);
            return new AnalyticsIndicator.AnalyticsIndicator(root);
        }
    }

    public class RootAnalyticsQueryIndicator
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public string Id { get; set; }

        [GraphQLNonNullType]
        public string Title { get; set; }

        public RootAnalyticsQueryIndicator(IIS.Core.Analytics.EntityFramework.AnalyticsIndicator indicator)
        {
            Id = indicator.Id.ToString();
            Title = indicator.Title;
        }

        [GraphQLType(typeof(AnyType))]
        public async Task<string[]> GetValues()
        {
            // TODO: remove mocked data and execute query instead
            return new string[] {
                "УСБУ у Вінницькій області",
                "УСБУ у Волинській області",
                "УСБУ в Дніпропетровській області",
                "УСБУ в Житомирській області"
            };
        }
    }
}