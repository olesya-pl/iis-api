using Iis.RabbitMq.Channels;

namespace Iis.MaterialDistributor.Contracts.Configurations
{
    public class MaterialCoefficientConsumerOptions
    {
        public const string SectionName = "materialCoefficientsConsumer";

        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }
}