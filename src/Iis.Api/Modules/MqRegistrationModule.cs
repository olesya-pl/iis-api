using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Iis.Services.Contracts.Configurations;

namespace Iis.Api.Modules
{
    internal static class MqRegistrationModule
    {
        private const string mqSectionName = "mq";

        public static IServiceCollection RegisterMqFactory(this IServiceCollection services, IConfiguration configuration, out string mqConnectionString)
        {

            var mqConfig = configuration.GetSection(mqSectionName).Get<MqConfiguration>();

            if (mqConfig == null) throw new InvalidOperationException($"No config was found with name \"{mqSectionName}\"");

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