﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Materials;
using Iis.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Iis.DbLayer.Repositories;

namespace Iis.Api.Materials
{
    public class MaterialOperatorConsumer : BackgroundService
    {
        private readonly UserService<IIISUnitOfWork> _userService;
        private readonly ILogger<MaterialOperatorConsumer> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IMaterialService _materialService;
        private readonly MaterialOperatorAssignerConfiguration _configuration;

        public MaterialOperatorConsumer(
            ILogger<MaterialOperatorConsumer> logger,
            IConnectionFactory connectionFactory,
            MaterialOperatorAssignerConfiguration configuration,
            IMaterialService materialService,
            UserService<IIISUnitOfWork> userService)
        {
            _userService = userService;
            _logger = logger;
            _materialService = materialService;
            _configuration = configuration;

            while (true)
            {
                try
                {
                    _connection = connectionFactory.CreateConnection();
                    break;
                }
                catch (BrokerUnreachableException)
                {
                    var timeout = 5000;
                    _logger.LogError($"Attempting to connect again in {timeout / 1000} sec.");
                    Thread.Sleep(timeout);
                }
            }

            _channel = _connection.CreateModel();
            _channel.QueueDeclare(
                queue: _configuration.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var availableOperators = await _userService.GetAvailableOperatorIdsAsync();

                    if (!availableOperators.Any())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                        continue;
                    }

                    foreach (var assignee in availableOperators)
                    {
                        var message = _channel.BasicGet(_configuration.QueueName, false);
                        if (message == null)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                            continue;
                        }
                        var materialId = new Guid(System.Text.Encoding.UTF8.GetString(message.Body));
                        await _materialService.AssignMaterialOperatorAsync(materialId, assignee);
                        _channel.BasicAck(message.DeliveryTag, false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError("MaterialOperatorAssigner. Exception={e}", e);
                }
            }
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
