using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IMultiElasticSearchParams
    {
        List<string> BaseIndexNames { get; set; }
        int From { get; set; }
        bool IsLenient { get; set; }
        List<string> ResultFields { get; set; }
        List<(string Query, List<IIisElasticField> Fields)> SearchParams { get; set; }
        int Size { get; set; }
    }
}