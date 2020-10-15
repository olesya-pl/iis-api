using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public class SearchEntitiesByConfiguredFieldsResult
    {
        public int Count { get; set; }
        public List<JObject> Entities { get; set; }
        public Dictionary<string, AggregationItem> Aggregations { get; set; }
    }
}
