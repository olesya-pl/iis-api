namespace Iis.RabbitMq.Channels
{
    public abstract class BaseChannelConfig
    {
        public const string DefaultExchangeName = "";

        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        public ushort PrefetchCount { get; set; } = 1;
    }
}