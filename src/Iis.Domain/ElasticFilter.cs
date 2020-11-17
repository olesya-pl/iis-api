using Iis.Interfaces.Elastic;

namespace Iis.Domain
{
    public class ElasticFilter : IElasticNodeFilter
    {
        public int Limit { get; set; }
        public int Offset { get; set; }
        public string Suggestion { get; set; }
    }
}
