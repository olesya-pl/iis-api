using Iis.RabbitMq.Channels;

namespace Iis.CoordinatesEventHandler.Configurations
{
    public class MessageHandlerConfiguration
    {
        public string HandlerName { get; set; }
        public ChannelConfig SourceChannel { get; set; }
    }
}