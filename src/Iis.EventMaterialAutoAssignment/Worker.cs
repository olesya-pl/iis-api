using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Iis.EventMaterialAutoAssignment
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly EventMaterialAssignerConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IConnection _connection;
        private readonly IModel _incommingChannel;
        private readonly IModel _outgoingChannel;


        public Worker(ILogger<Worker> logger,
            IConnectionFactory connectionFactory,
            EventMaterialAssignerConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;

            while (true)
            {
                try
                {
                    _connection = connectionFactory.CreateConnection();
                    break;
                }
                catch (BrokerUnreachableException e)
                {
                    var timeout = 5000;
                    _logger.LogError($"Attempting to connect again in {timeout / 1000} sec.");
                    Thread.Sleep(timeout);
                }
            }

            _incommingChannel = _connection.CreateModel();
            _incommingChannel.QueueDeclare(
                queue: _configuration.IncomingQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _outgoingChannel = _connection.CreateModel();
            _outgoingChannel.QueueDeclare(
                queue: _configuration.OutgoingQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _outgoingChannel.ExchangeDeclare(_configuration.OutgoingExchangeName, ExchangeType.Topic);

            _outgoingChannel.QueueBind(_configuration.OutgoingQueueName, _configuration.OutgoingExchangeName, _configuration.OutgoingRoutingKey);
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channelConsumer = new AsyncEventingBasicConsumer(_incommingChannel);

            channelConsumer.Received += async (sender, args) =>
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AssignmentConfigContext>();
                        var configs = await context.AssignmentConfigs
                            .AsNoTracking()
                            .Include(p => p.Keywords)
                            .Select(p => new { p.Keywords, p.Name})
                            .ToArrayAsync();
                        
                        var json = Encoding.UTF8.GetString(args.Body);
                        var materialIds = JsonConvert.DeserializeObject<List<Guid>>(json);

                        foreach (var config in configs)
                        {
                            var message = new TermsMessage()
                            {
                                Title = config.Name,
                                MaterialIds = materialIds,
                                Terms = config.Keywords.Select(p => p.Keyword).ToArray()
                            };

                            var serialized = JsonConvert.SerializeObject(message);
                            var body = Encoding.UTF8.GetBytes(serialized);

                            _outgoingChannel.BasicPublish(exchange: _configuration.OutgoingExchangeName,
                                                routingKey: _configuration.OutgoingRoutingKey,
                                                basicProperties: null,
                                                body: body);
                        }

                        _incommingChannel.BasicAck(args.DeliveryTag, false);
                    }                    
                }
                catch (Exception e)
                {
                    _logger.LogError("EventMaterialAssigner. Exception {e}", e);
                    _incommingChannel.BasicReject(args.DeliveryTag, false);
                }
            };

            _incommingChannel.BasicConsume(_configuration.IncomingQueueName, false, channelConsumer);

            return Task.CompletedTask;

        }
    }
}
