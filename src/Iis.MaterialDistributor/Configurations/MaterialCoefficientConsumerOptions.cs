using Iis.RabbitMq.Channels;

namespace Iis.MaterialDistributor.Configurations
{
    public class MaterialCoefficientConsumerOptions
    {
        public const string SectionName = "materialCoefficientsConsumer";

        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }
}