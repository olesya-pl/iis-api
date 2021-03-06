using Iis.Interfaces.Elastic;
using System.Collections.Generic;

namespace Iis.Domain.Elastic
{
    public class MultiElasticSearchParams : IMultiElasticSearchParams
    {
        public List<string> BaseIndexNames { get; set; } = new List<string>();
        public List<string> ResultFields { get; set; } = new List<string> { "*" };
        public List<(string Query, List<IIisElasticField> Fields)> SearchParams { get; set; }
        public bool IsLenient { get; set; } = true;
        public int From { get; set; }
        public int Size { get; set; }
    }
}
