using System.Threading;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Iis.RabbitMq.Helpers
{
    public static class ConnectionFactoryExtensions
    {
        public static IConnection CreateAndWaitConnection(this IConnectionFactory connectionFactory, int retryTimeoutInSeconds = 5, ILogger logger = null, string clientName = null)
        {
            while (true)
            {
                try
                {
                    return connectionFactory.CreateConnection(clientName);
                }
                catch (BrokerUnreachableException)
                {
                    logger?.LogError($"Attempting to connect again in {retryTimeoutInSeconds} sec.");
                    Thread.Sleep(retryTimeoutInSeconds * 1000);
                }
            }
        }
    }
}