using System.Collections.Generic;

namespace IIS.Core.GraphQL.Entities
{
    public class AggregationItem
    {
        public List<AggregationBucket> Buckets { get; set; }
    }
}
