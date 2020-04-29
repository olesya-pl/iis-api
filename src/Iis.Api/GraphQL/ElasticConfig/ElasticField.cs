using Iis.Interfaces.Elastic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.ElasticConfig
{
    public class ElasticField: IIisElasticField
    {
        public string Name { get; set; }
        public bool IsExcluded { get; set; }
        public int Fuzziness { get; set; }
        public decimal Boost { get; set; }
    }
}
