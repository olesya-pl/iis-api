using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IIisElasticSearchParams
    {
        List<string> BaseIndexNames { get; }
        string Query { get; }
        List<string> ResultFields { get; }
        IReadOnlyList<IIisElasticField> SearchFields { get; }
        bool IsLenient { get; }
        int From { get; }
        int Size { get; }
    }
}
