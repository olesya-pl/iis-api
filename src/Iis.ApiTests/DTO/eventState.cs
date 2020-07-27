using HotChocolate;
using HotChocolate.Types;
using Newtonsoft.Json.Linq;

namespace AcceptanceTests.DTO
{
    public class EventStateListResponse
    {
        public JObject Items { get; set; }
    }
    public class EventStateListType
    {
        //public int Count { get; set; }
        public ItemType Items { get; set; }
    }
    public class ItemType
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public System.Guid Id { get; set; }
        public string Name { get; set; }
    }
}