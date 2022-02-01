using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public class ElasticMultiSearchParams
    {
        public List<string> BaseIndexNames { get; set; } = new List<string>();
        public List<string> ResultFields { get; set; } = new List<string> { "*" };
        public List<SearchParameter> SearchParams { get; set; }
        public bool IsLenient { get; set; } = true;
        public int From { get; set; }
        public int Size { get; set; }
    }
}