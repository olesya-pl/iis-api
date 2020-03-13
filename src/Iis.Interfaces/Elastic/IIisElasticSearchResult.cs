using System.Collections.Generic;

namespace Iis.Interfaces.Elastic
{
    public interface IIisElasticSearchResult
    {
        int Count { get; }
        List<string> Ids { get; }
    }
}