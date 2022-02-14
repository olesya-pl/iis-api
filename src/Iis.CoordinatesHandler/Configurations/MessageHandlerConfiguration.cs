using Iis.RabbitMq.Channels;

namespace Iis.CoordinatesEventHandler.Configurations
{
    public class MessageHandlerConfiguration
    {
        public ChannelConfig SourceChannel { get; set; }
    }
}