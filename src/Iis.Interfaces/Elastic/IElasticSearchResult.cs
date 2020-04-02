using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticSearchResult
    {
        int Count { get; }
        IEnumerable<string> Identifiers { get; }
    }
}