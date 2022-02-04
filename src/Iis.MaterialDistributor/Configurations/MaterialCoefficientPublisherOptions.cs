using Iis.RabbitMq.Channels;

namespace Iis.MaterialDistributor.Configurations
{
    public class MaterialCoefficientPublisherOptions
    {
        public const string SectionName = "materialCoefficientsPublisher";

        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }
}