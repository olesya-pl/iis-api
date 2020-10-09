using Iis.Interfaces.Elastic;

namespace Iis.Domain.Elastic
{
    public class IisElasticField : IIisElasticField
    {
        public string Name { get; set; }
        public bool IsExcluded { get; set; } = false;
        public int Fuzziness { get; set; } = 0;
        public decimal Boost { get; set; } = 1.0m;
        public bool IsAggregated { get; set; }
    }
}
