using Iis.RabbitMq.Channels;

namespace Iis.Services.Contracts.Configurations
{
    public class MaterialConsumerConfiguration
    {
        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }
}