namespace Iis.Api.Configuration
{
    /// <summary>
    /// Type for RabbitMq channel config (exchange config and routing keys for binding) 
    /// </summary>
    public sealed class ChannelConfig
    {
        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get;set;}
        public string[] RoutingKeys { get; set; }
    }
}