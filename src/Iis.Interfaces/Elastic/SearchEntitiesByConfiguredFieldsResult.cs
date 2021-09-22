using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public class SearchEntitiesByConfiguredFieldsResult
    {
        private static readonly SearchEntitiesByConfiguredFieldsResult _emptyResult;

        static SearchEntitiesByConfiguredFieldsResult()
        {
            _emptyResult = new SearchEntitiesByConfiguredFieldsResult
            {
                Count = 0,
                Entities = new List<JObject>(0),
                Aggregations = new Dictionary<string, AggregationItem>()
            };
        }

        public int Count { get; set; }
        public List<JObject> Entities { get; set; }
        public Dictionary<string, AggregationItem> Aggregations { get; set; }
        public static SearchEntitiesByConfiguredFieldsResult Empty => _emptyResult;
    }
}
