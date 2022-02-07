namespace Iis.RabbitMq.Channels
{
    internal class QueryConfig
    {
        public static QueryConfig DefaultDurable => new QueryConfig { Durable = true };
        public static QueryConfig Default => new QueryConfig { };
        public bool Durable { get; set; }
        public bool Exclusive { get; set; }
        public bool AutoDelete { get; set; }
    }
}