using Iis.Interfaces.Elastic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Domain.Elastic
{
    public class IisElasticSearchParams: IIisElasticSearchParams
    {
        public List<string> BaseIndexNames { get; set; } = new List<string>();
        public string Query { get; set; }
        public List<string> ResultFields { get; set; } = new List<string> { "_id" };
        public List<string> SearchFields { get; set; } = new List<string> { "*" };
        public bool IsLenient { get; set; } = true;
    }
}
