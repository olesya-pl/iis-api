using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.DbLayer.Repositories;
using Iis.Messages.Materials;
using Iis.RabbitMq.Channels;
using Iis.RabbitMq.Helpers;
using IIS.Repository.Factories;
using Iis.Services.Contracts.Configurations;
using Iis.Services.Contracts.Interfaces;
using Iis.Utility;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Iis.Api.Materials
{
    public class MaterialCoefficientsConsumer : BackgroundService
    {
        private const int RetryIntervalSeconds = 5;
        private const string ApplicationName = "Iis.Api.MaterialCoefficientsConsumer";
        private readonly ILogger<MaterialCoefficientsConsumer> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly MaterialCoefficientsConsumerConfiguration _configuration;
        private readonly IUnitOfWorkFactory<IIISUnitOfWork> _unitOfWorkFactory;
        private readonly IMaterialElasticService _materialElasticService;

        private IConsumeMessageChannel<MaterialCoefficientEvaluatedEventMessage> _consumeChannel;
        private IConnection _connection;

        public MaterialCoefficientsConsumer(
            ILogger<MaterialCoefficientsConsumer> logger,
            IConnectionFactory connectionFactory,
            IOptions<MaterialCoefficientsConsumerConfiguration> options,
            IUnitOfWorkFactory<IIISUnitOfWork> unitOfWorkFactory,
            IMaterialElasticService materialElasticService)
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _configuration = options.Value;
            _unitOfWorkFactory = unitOfWorkFactory;
            _materialElasticService = materialElasticService;
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
            }

            base.Dispose();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _connection = _connectionFactory.CreateAndWaitConnection(RetryIntervalSeconds, _logger, ApplicationName);

            _consumeChannel = new ConsumeMessageChannel<MaterialCoefficientEvaluatedEventMessage>(
                _connection,
                _configuration.SourceChannel,
                _logger
            );

            _consumeChannel.OnMessageReceived = ProcessMessageAsync;

            return Task.CompletedTask;
        }

        private async Task ProcessMessageAsync(MaterialCoefficientEvaluatedEventMessage message)
        {
            await message.MaterialCoefficients.ForEachAsync(materialCoefficient =>
            {
                return RunWithCommitAsync(_ => _.MaterialRepository.EditMaterialAsync(
                    materialCoefficient.Id,
                    material =>
                    {
                        material.PermanentCoefficient = materialCoefficient.Value;
                    }));
            });

            var materialIds = message.MaterialCoefficients.Select(_ => _.Id)
                .Distinct()
                .ToHashSet();
            await _materialElasticService.PutMaterialsToElasticSearchAsync(materialIds);
        }

        private async Task RunWithCommitAsync(Func<IIISUnitOfWork, Task> action)
        {
            using var unitOfWork = _unitOfWorkFactory.Create();

            await action(unitOfWork);
            await unitOfWork.CommitAsync();
        }
    }
}