using Iis.RabbitMq.Channels;

namespace Iis.Services.Contracts.Configurations
{
     /// <summary>
    /// Type for Material Event configuration
    /// </summary>
    public class MaterialEventConfiguration
    {
        public ChannelConfig TargetChannel {get;set;}
    }
}