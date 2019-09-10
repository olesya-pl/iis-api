using System;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Files.EntityFramework;
using IIS.Core.Ontology.EntityFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace IIS.Core.GSM.Consumer
{
    public class MaterialEventConsumer : BackgroundService
    {
        private struct MaterialAddedEvent
        {
            public Guid Id;
            public Guid MaterialId;
        }
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly ILogger _logger;
        private readonly FileServiceFactory _fileServiceFactory;
        private readonly ContextFactory _contextFactory;
        private readonly IGsmTranscriber _gsmTranscriber;


        public MaterialEventConsumer(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory,
            FileServiceFactory fileServiceFactory, ContextFactory contextFactory, IGsmTranscriber gsmTranscriber)
        {
            _logger = loggerFactory.CreateLogger<MaterialEventConsumer>();
            _connectionFactory = connectionFactory;
            _fileServiceFactory = fileServiceFactory;
            _contextFactory = contextFactory;
            _gsmTranscriber = gsmTranscriber;

            while (true)
            {
                try
                {
                    _connection = _connectionFactory.CreateConnection();
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

            //_connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _channel.QueueDeclare("gsm", true, false);

            stoppingToken.ThrowIfCancellationRequested();

            var gsmConsumer = new EventingBasicConsumer(_channel);//new AsyncEventingBasicConsumer(_channel);
            gsmConsumer.Received += (sender, ea) =>
            {
                GsmConsumer_Received(sender, ea).Wait(stoppingToken);
            };

            _channel.BasicConsume(queue: "gsm",
                                 autoAck: false,
                                 consumer: gsmConsumer);

            await Task.CompletedTask;
        }

        private async Task GsmConsumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                // received message  
                var message = System.Text.Encoding.UTF8.GetString(ea.Body);
                var json = JObject.Parse(message);
                var eventData = json.ToObject<MaterialAddedEvent>();
                _logger.LogInformation("***************** MESSAGE RECEIVED ********************" + message);
                var fileService = _fileServiceFactory.CreateService();
                var file = await fileService.GetFileAsync(eventData.Id);
                var mlResponse = await _gsmTranscriber.TranscribeAsync(file);
                if (mlResponse["status"].Value<string>() != "OK")
                {
                    _channel.BasicNack(ea.DeliveryTag, false, false);
                    _logger.LogInformation("*************************** JOB FAILED **********************************");
                }
                else
                {
                    var data = JObject.Parse("{ 'transcription': '" + mlResponse["transcription"][0][0].Value<string>() + "' }");
                    using (var context = _contextFactory.CreateContext())
                    {
                        var mi = new Materials.EntityFramework.MaterialInfo
                        {
                            Id = Guid.NewGuid(),
                            Data = data?.ToString(),
                            MaterialId = eventData.MaterialId,
                            Source = "ML",
                            SourceType = "GSM",
                            SourceVersion = "xz"
                        };
                        context.Add(mi);
                        await context.SaveChangesAsync();
                    }
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("*************************** JOB DONE **********************************");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "");
                throw;
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
