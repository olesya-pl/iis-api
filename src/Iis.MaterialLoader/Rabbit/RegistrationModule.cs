using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Iis.RabbitMq.Helpers;
using Iis.RabbitMq.Channels;
using Iis.Messages.Materials;

namespace Iis.MaterialLoader.Rabbit
{
    internal static class RegistrationModule
    {
        private const string EventProducerSectionName = "eventProducer";
        private const string ExternalExchangesSectionName = "externalExchanges";
        private const string MaterialLoaderLoggerName = "MaterialLoader.RabbitMq.Connection";
        private const int RetryTimeOutSec = 5;

        public static IServiceCollection RegisterMqInstances(this IServiceCollection services, IConfiguration configuration)
        {
            var eventProducerConfig = configuration.GetSection(EventProducerSectionName)
                                                        .Get<EventProducerConfig>();
            var externalExchangesConfig = configuration.GetSection(ExternalExchangesSectionName)
                                                        .Get<ExternalExchangesConfig>();

            services.AddSingleton(eventProducerConfig);

            services.AddSingleton<IConnection>(provider => { 
                var factory = provider.GetService<IConnectionFactory>();
                var logger = provider.GetService<ILoggerFactory>().CreateLogger(MaterialLoaderLoggerName);
                return factory.CreateAndWaitConnection(RetryTimeOutSec, logger);
            });

            var provider = services.BuildServiceProvider();
            var connection = provider.GetService<IConnection>();

            BindExchanges(connection, externalExchangesConfig.Source, externalExchangesConfig.DestinationList);

            services.AddSingleton<IPublishMessageChannel<MaterialCreatedMessage>>(provider => {
                var connection = provider.GetService<IConnection>();

                return new PublishMessageChannel<MaterialCreatedMessage>(connection, eventProducerConfig.TargetChannel);
            });

            return services;
        }

        private static void BindExchanges(IConnection connection, ChannelConfig sourceConfig, IReadOnlyCollection<ChannelConfig> destinationConfigList)
        {
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(sourceConfig.ExchangeName, sourceConfig.ExchangeType ?? ExchangeType.Topic);

                foreach (var destinationConfig in destinationConfigList)
                {
                    channel.ExchangeDeclare(destinationConfig.ExchangeName, destinationConfig.ExchangeType ?? ExchangeType.Topic);
                    foreach (var routingKey in destinationConfig.RoutingKeys)
                    {
                        channel.ExchangeBind(destinationConfig.ExchangeName, sourceConfig.ExchangeName, routingKey);
                    }
                }
            }
        }
    }
}