using Iis.RabbitMq.Channels;

namespace Iis.MaterialDistributor.Configurations
{
    public class MaterialCoefficientPublisherOptions
    {
        public const string SectionName = "materialCoefficientsPublisher";

        public ChannelConfig SourceChannel { get; set; }
    }
}