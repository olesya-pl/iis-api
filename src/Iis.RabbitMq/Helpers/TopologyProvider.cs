using System.Collections.Generic;
using RabbitMQ.Client;
using Iis.RabbitMq.Channels;

namespace Iis.RabbitMq.Helpers
{
    internal class TopologyProvider
    {
        private const string DefautExchangeType = ExchangeType.Topic;

        public static void ConfigurePublishing(IModel model, BaseChannelConfig config)
        {
            DeclareExchange(model, config);
        }

        public static void ConfugureConsuming(IModel model, ChannelConfig config, QueryConfig queryConfig)
        {
            DeclareExchange(model, config);

            model.BasicQos(0, config.PrefetchCount, false);

            DeclareQueue(model, config, queryConfig, out string declaredQueueName);

            config.QueueName = declaredQueueName;

            BindQueue(model, config, config.RoutingKeys);
        }

        public static void ConfugureConsuming(IModel model, RPCChannelConfig config, QueryConfig queryConfig)
        {
            DeclareExchange(model, config);

            model.BasicQos(0, config.PrefetchCount, false);

            DeclareQueue(model, config, queryConfig, out string declaredQueueName);

            config.QueueName = declaredQueueName;

            BindQueue(model, config, new[] { config.RoutingKey });
        }

        private static void DeclareExchange(IModel model, BaseChannelConfig config)
        {
            if (IsNotDefinedOrDefaultExchangeUsed(config)) return;

            var exchangeType = config.ExchangeType ?? DefautExchangeType;

            model.ExchangeDeclare(config.ExchangeName, exchangeType);
        }

        private static void DeclareQueue(IModel model, BaseChannelConfig config, QueryConfig queryConfig, out string declaredQueueName)
        {
            declaredQueueName = model.QueueDeclare(
                config.QueueName ?? string.Empty,
                durable: queryConfig.Durable,
                exclusive: queryConfig.Exclusive,
                autoDelete: queryConfig.AutoDelete).QueueName;
        }

        private static void BindQueue(IModel model, BaseChannelConfig config, IReadOnlyCollection<string> routingKeys)
        {
            if (IsNotDefinedOrDefaultExchangeUsed(config)) return;

            foreach (var routingKey in routingKeys)
            {
                model.QueueBind(config.QueueName, config.ExchangeName, routingKey);
            }
        }

        private static bool IsNotDefinedOrDefaultExchangeUsed(BaseChannelConfig config)
        {
            return string.IsNullOrWhiteSpace(config?.ExchangeName);
        }
    }
}