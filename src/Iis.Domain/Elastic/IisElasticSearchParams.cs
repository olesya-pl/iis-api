using Iis.Interfaces.Elastic;
using System.Collections.Generic;

namespace Iis.Domain.Elastic
{
    public class IisElasticSearchParams : IIisElasticSearchParams
    {
        public List<string> BaseIndexNames { get; set; } = new List<string>();
        public string Query { get; set; }
        public List<string> ResultFields { get; set; } = new List<string> { "*" };
        public IReadOnlyList<IIisElasticField> SearchFields { get; set; }
        public bool IsLenient { get; set; } = true;
        public int From { get; set; }
        public int Size { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }
    }
}
