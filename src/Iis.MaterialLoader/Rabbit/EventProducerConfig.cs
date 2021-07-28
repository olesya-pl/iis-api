using Iis.RabbitMq.Channels;
namespace Iis.MaterialLoader.Rabbit
{
    public class EventProducerConfig
    {
        public ChannelConfig TargetChannel { get; set; }
    }
}