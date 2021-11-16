using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticMultiSearchParams
    {
        List<string> BaseIndexNames { get; set; }
        int From { get; set; }
        bool IsLenient { get; set; }
        List<string> ResultFields { get; set; }
        List<SearchParameter> SearchParams { get; set; }
        int Size { get; set; }
    }
}