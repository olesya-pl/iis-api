using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticSearchResult
    {
        int Count { get; }
        IEnumerable<IElasticSearchResultItem> Items { get; }
    }

    public interface IElasticSearchResultItem
    {
        string Identifier { get; set; }
        JToken Higlight { get; set; }
        JObject SearchResult { get; set; }
    }
}