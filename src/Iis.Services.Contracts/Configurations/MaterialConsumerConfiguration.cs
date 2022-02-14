using Iis.RabbitMq.Channels;

namespace Iis.Services.Contracts.Configurations
{
    public class MaterialConsumerConfiguration
    {
        public ChannelConfig SourceChannel { get; set; }
    }
}