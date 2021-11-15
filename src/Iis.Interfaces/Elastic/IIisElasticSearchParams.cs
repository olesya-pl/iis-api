using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IIisElasticSearchParams
    {
        IReadOnlyList<IIisElasticField> SearchFields { get; }
        IEnumerable<string> BaseIndexNames { get; }
        IEnumerable<string> ResultFields { get; }
        string Query { get; }
        int From { get; }
        int Size { get; }
        string SortColumn { get; }
        string SortOrder { get; }
        bool IsLenient { get; }
        bool IsExact { get; }
    }
}
