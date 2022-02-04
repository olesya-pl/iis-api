using Iis.RabbitMq.Channels;

namespace Iis.MaterialDistributor.Configurations
{
    public class MaterialNextAssignedConsumerConfig
    {
        public const string SectionName = "materialNextAssignedConsumer";

        public RPCChannelConfig SourceChannel { get; set; }
    }
}