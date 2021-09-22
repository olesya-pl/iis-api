using System.Collections.Generic;
using Iis.RabbitMq.Channels;
namespace Iis.MaterialLoader.Rabbit
{
    public class ExternalExchangesConfig
    {
        public ChannelConfig Source { get; set; }
        public IReadOnlyCollection<ChannelConfig> DestinationList { get; set; }
    }
}