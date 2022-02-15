using Iis.RabbitMq.Channels;

namespace Iis.Services.Contracts.Configurations
{
    public class MaterialCoefficientsConsumerConfiguration
    {
        public ChannelConfig SourceChannel { get; set; }
    }
}