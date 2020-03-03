using Iis.Interfaces.Elastic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Elastic
{
    public class IisElasticSearchResult : IIisElasticSearchResult
    {
        public int Count { get; set; }
        public List<string> Ids { get; set; }
    }
}
