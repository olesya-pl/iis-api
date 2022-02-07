using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Iis.RabbitMq.Channels
{
    public static class MessageChannels
    {
        public static IRPCConsumeMessageChannel<TRequest, TResponse> CreateRPCConsumer<TRequest, TResponse>(IConnection connection, RPCChannelConfig config, ILogger logger)
        {
            return new RPCConsumeMessageChannel<TRequest, TResponse>(connection, config, logger);
        }

        public static IRPCConsumeMessageChannel<T, T> CreateRPCConsumer<T>(IConnection connection, RPCChannelConfig config, ILogger logger)
        {
            return new RPCConsumeMessageChannel<T, T>(connection, config, logger);
        }

        public static IRPCPublishMessageChannel<TRequest, TResponse> CreateRPCPublisher<TRequest, TResponse>(IConnection connection, RPCChannelConfig config, ILogger logger)
        {
            return new RPCPublishMessageChannel<TRequest, TResponse>(connection, config, logger);
        }

        public static IRPCPublishMessageChannel<T, T> CreateRPCPublisher<T>(IConnection connection, RPCChannelConfig config, ILogger logger)
        {
            return new RPCPublishMessageChannel<T, T>(connection, config, logger);
        }
    }
}