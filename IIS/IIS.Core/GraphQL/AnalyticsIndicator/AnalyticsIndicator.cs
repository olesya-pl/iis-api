using HotChocolate;
using HotChocolate.Types;
using System.Threading.Tasks;

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

        [GraphQLType(typeof(AnyType))]
        public async Task<string[]> GetValues()
        {
            // TODO: remove mocked data and execute query instead
            if (ParentId == null)
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