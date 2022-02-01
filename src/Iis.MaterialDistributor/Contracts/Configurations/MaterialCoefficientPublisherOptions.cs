using Iis.RabbitMq.Channels;

namespace Iis.MaterialDistributor.Contracts.Configurations
{
    public class MaterialCoefficientPublisherOptions
    {
        public const string SectionName = "materialCoefficientsPublisher";

        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }
}