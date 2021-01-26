using Iis.Interfaces.Elastic;

namespace IIS.Core.GraphQL.ElasticConfig
{
    public class ElasticField : IIisElasticField
    {
        public string Name { get; set; }
        public string Alias { get; }
        public bool IsExcluded { get; set; }
        public int Fuzziness { get; set; }
        public decimal Boost { get; set; }
        public bool IsAggregated { get; set; }
    }
}
