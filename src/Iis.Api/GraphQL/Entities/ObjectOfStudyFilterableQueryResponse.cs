using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Entities
{
    public class ObjectOfStudyFilterableQueryResponse
    {
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Items { get; set; }
        public int Count { get; set; }
        public Dictionary<string, AggregationItem> Aggregations { get; set; }
    }
}
