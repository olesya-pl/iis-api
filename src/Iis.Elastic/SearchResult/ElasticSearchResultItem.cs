using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchResult
{
    public class ElasticSearchResultItem : IElasticSearchResultItem
    {
        public string Identifier { get; set; }
        public JToken Higlight { get; set; }
        public JObject SearchResult { get; set; }
    }

}
