using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Elastic
{
    public interface IIisElasticSearchParams
    {
        List<string> BaseIndexNames { get; }
        string Query { get; }
        List<string> ResultFields { get; }
        List<string> SearchFields { get; }
        bool IsLenient { get; }
    }
}
