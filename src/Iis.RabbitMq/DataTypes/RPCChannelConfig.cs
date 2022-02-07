namespace Iis.RabbitMq.Channels
{
    /// <summary>
    /// Type for RabbitMq channel config for Remote procedure call(exchange config and routing keys for binding)
    /// </summary>
    public sealed class RPCChannelConfig : BaseChannelConfig
    {
        public string RoutingKey { get; set; }
    }
}