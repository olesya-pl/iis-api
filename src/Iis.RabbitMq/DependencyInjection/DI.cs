using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Iis.RabbitMq.DependencyInjection
{
    public static class DI
    {
        private const string MessageQueueSectionName = "mq";

        public static IServiceCollection RegisterMqFactory(this IServiceCollection services, IConfiguration configuration, out string mqConnectionString)
        {
            var mqConfig = configuration.GetSection(MessageQueueSectionName).Get<MqFactoryConfiguration>();

            if (mqConfig == null) throw new InvalidOperationException($"No config was found with name \"{MessageQueueSectionName}\"");

            var connectionFactory = new ConnectionFactory
            {
                HostName = mqConfig.HostName,
                UserName = mqConfig.UserName ?? ConnectionFactory.DefaultUser,
                Password = mqConfig.Password ?? ConnectionFactory.DefaultPass,
                VirtualHost = mqConfig.VirtualHost ?? ConnectionFactory.DefaultVHost,
                DispatchConsumersAsync = true,
                Port = mqConfig.Port ?? AmqpTcpEndpoint.UseDefaultPort
            };
            var portString = connectionFactory.Port == -1 ? string.Empty :
                ":" + connectionFactory.Port;
            mqConnectionString = $"amqp://{connectionFactory.UserName}:{connectionFactory.Password}@{connectionFactory.HostName}{portString}/{connectionFactory.VirtualHost}";

            return services
                    .AddTransient<IConnectionFactory>(serviceProvider => connectionFactory);
        }
    }
}