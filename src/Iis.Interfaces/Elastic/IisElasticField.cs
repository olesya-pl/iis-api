using Iis.Interfaces.Elastic;

namespace Iis.Interfaces.Elastic
{
    public class IisElasticField : IIisElasticField
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsExcluded { get; set; }
        public int Fuzziness { get; set; }
        public decimal Boost { get; set; } = 1.0m;
        public bool IsAggregated { get; set; }
    }
}
