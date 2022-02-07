using Iis.RabbitMq.Channels;

namespace Iis.Services.Contracts.Configurations
{
    public class MaterialNextAssignedPublisherConfig
    {
        public const string SectionName = "materialNextAssignedPublisher";

        public RPCChannelConfig TargetChannel { get; set; }
    }
}