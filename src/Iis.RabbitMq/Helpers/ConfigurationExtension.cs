using RabbitMQ.Client;
using Iis.RabbitMq.Channels;
namespace Iis.RabbitMq.Helpers
{
    internal static class ConfigurationExtension
    {
        public static bool IsNotDefaultExchangeUsed(this ChannelConfig config)
        {
            return !string.IsNullOrWhiteSpace(config.ExchangeName);
        }
    }
}