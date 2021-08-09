namespace Iis.RabbitMq.Channels
{
    public class MessageState
    {
        public string ExchangeName { get; set; }
        public string RoutingKey { get; set; }
        public string Body { get; set; }
    }
}