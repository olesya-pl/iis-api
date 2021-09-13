using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Entities
{
    public class OntologyFilterableQueryResponse
    {
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Items { get; set; }
        public int Count { get; set; }
        public Dictionary<string, AggregationItem> Aggregations { get; set; }
        public IReadOnlyList<AggregationNodeTypeItem> NodeTypeAggregations { get; set; }
    }
}
