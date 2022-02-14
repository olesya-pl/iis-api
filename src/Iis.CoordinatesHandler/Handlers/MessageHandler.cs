using System;
using System.Threading;
using System.Threading.Tasks;
using Iis.CoordinatesEventHandler.Configurations;
using Iis.CoordinatesEventHandler.Processors;
using Iis.Messages.Materials;
using Iis.RabbitMq.Channels;
using Iis.RabbitMq.Helpers;
using Iis.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Iis.CoordinatesEventHandler.Handlers
{
    public class MessageHandler : BackgroundService
    {
        private const int RetryIntervalSec = 5;
        private const string ApplicationName = "Iis.Api.CoordinatesEventHandler";
        private ILogger<MessageHandler> _logger;
        private IConnectionFactory _connectionFactory;
        private IConnection _connection;
        private MessageHandlerConfiguration _handlerConfiguration;
        private IServiceProvider _provider;
        private IConsumeMessageChannel<MaterialProcessingEventMessage> _consumeChannel;

        public MessageHandler(
            ILogger<MessageHandler> logger,
            IConnectionFactory connectionFactory,
            MessageHandlerConfiguration handlerConfiguration,
            IServiceProvider provider)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _handlerConfiguration = handlerConfiguration;
            _provider = provider;
        }

        public override void Dispose()
        {
            if (_consumeChannel != null)
            {
                _consumeChannel.Dispose();
                _consumeChannel = null;
            }

            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }

            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _connectionFactory.CreateAndWaitConnection(RetryIntervalSec, _logger, ApplicationName);

            _consumeChannel = new ConsumeMessageChannel<MaterialProcessingEventMessage>(
                _connection,
                _handlerConfiguration.SourceChannel,
                _logger
            );

            _consumeChannel.OnMessageReceived = ProcessMessageAsync;

            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(MaterialProcessingEventMessage message)
        {
            var processor = _provider.GetService<ICoordinatesProcessorsFactory>().GetProcessor(message.Source, message.Type);

            if (processor.IsDummy) return;

            var materialProvider = _provider.GetService<IMaterialProvider>();
            var locationService = _provider.GetService<ILocationHistoryService>();

            var material = await materialProvider.GetMaterialAsync(message.Id);

            if (material is null) return;

            var locationHistoryList = await processor.GetLocationHistoryListAsync(material);

            await locationService.SaveLocationHistoryAsync(locationHistoryList);
        }
    }
}