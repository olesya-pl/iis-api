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

        [GraphQLType(typeof(AnyType))]
        public async Task<string[]> GetValues()
        {
            // TODO: remove mocked data and execute query instead
            if (Indicator.ParentId == null)
                return new string[] {
                    "УСБУ у Вінницькій області",
                    "УСБУ у Волинській області",
                    "УСБУ в Дніпропетровській області",
                    "УСБУ в Житомирській області"
                };

            return new string[] {
                "5",
                "6",
                "7",
                "8"
            };
        }
    }
}