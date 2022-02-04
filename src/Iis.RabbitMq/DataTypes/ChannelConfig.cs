using System.Collections.Generic;

namespace Iis.RabbitMq.Channels
{
    /// <summary>
    /// Type for RabbitMq channel config for basic call(exchange config and routing keys for binding)
    /// </summary>
    public sealed class ChannelConfig : BaseChannelConfig
    {
        public IReadOnlyCollection<string> RoutingKeys { get; set; }
    }
}