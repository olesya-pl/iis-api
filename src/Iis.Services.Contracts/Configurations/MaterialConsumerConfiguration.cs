using Iis.RabbitMq.Channels;

namespace Iis.Services.Contracts.Configurations
{
    /// <summary>
    /// Type for MaterialConsumer RabbitMqConsumer configuration
    /// </summary>
    public class MaterialConsumerConfiguration
    {
        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }

}