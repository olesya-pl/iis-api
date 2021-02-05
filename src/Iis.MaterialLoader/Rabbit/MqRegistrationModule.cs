using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Iis.MaterialLoader.Rabbit
{
    internal static class MqRegistrationModule
    {
        private const string MqSectionName = "mq";

        public static IServiceCollection AddRabbit(this IServiceCollection services, IConfiguration configuration, out string mqConnectionString)
        {

            var mqConfig = configuration.GetSection(MqSectionName).Get<MqConfiguration>();

            if (mqConfig == null) throw new InvalidOperationException($"No config was found with name \"{MqSectionName}\"");

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