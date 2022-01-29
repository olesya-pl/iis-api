using Iis.RabbitMq.Channels;

namespace Iis.Services.Contracts.Configurations
{
    public class MaterialCoefficientsConsumerConfiguration
    {
        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }
}