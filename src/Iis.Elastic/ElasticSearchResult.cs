using System.Collections.Generic;
using Iis.Interfaces.Elastic;

namespace Iis.Elastic
{
    public class ElasticSearchResult : IElasticSearchResult
    {
        public int Count { get; set; }
        public IEnumerable<string> Identifiers { get; set; }
    }
}
