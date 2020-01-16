using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Files.EntityFramework;
using IIS.Core.GraphQL.Materials;
using IIS.Core.Ontology.EntityFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Http;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using IIS.Core.Ontology;

namespace IIS.Core.GSM.Consumer
{
    public class MaterialEventConsumer : BackgroundService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private struct MaterialAddedEvent
        {
            public Guid FileId;
            public Guid MaterialId;
            public List<IIS.Core.GraphQL.Materials.Node> Nodes { get; set; }
        }

        private readonly ILogger<MaterialEventConsumer> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly FileServiceFactory _fileServiceFactory;
        private readonly ContextFactory _contextFactory;
        private readonly IGsmTranscriber _gsmTranscriber;
        private readonly IConfiguration _config;


        public MaterialEventConsumer(
            ILogger<MaterialEventConsumer> logger,
            IConnectionFactory connectionFactory,
            FileServiceFactory fileServiceFactory,
            ContextFactory contextFactory,
            IGsmTranscriber gsmTranscriber,
            IConfiguration config
        )
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _fileServiceFactory = fileServiceFactory;
            _contextFactory = contextFactory;
            _gsmTranscriber = gsmTranscriber;
            _config = config;

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
                MaterialAddedEvent eventData = json.ToObject<MaterialAddedEvent>();
                if (eventData.FileId == Guid.Empty && eventData.MaterialId == Guid.Empty)
                {
                    await _createFeaturesInArcgis(eventData.Nodes);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation("***************** MESSAGE RECEIVED ********************" + message);
                var fileService = _fileServiceFactory.CreateService();
                var file = await fileService.GetFileAsync(eventData.FileId);
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

        private async Task _createFeaturesInArcgis(List<GraphQL.Materials.Node> nodes)
        {
            // TODO: the next line should be replaced with the id of autolinked entity
            var entityId = Guid.NewGuid();
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            foreach (var node in nodes)
            {
                if (node.UpdateField == null)
                    continue;

                var features = node.UpdateField.Values.Select(value => new ArcgisFeature(entityId, value));
                var url = _config.GetValue<string>("map:layers:trackObjectPosition");

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("features", JsonConvert.SerializeObject(features, Formatting.Indented, jsonSettings)),
                    new KeyValuePair<string, string>("rollbackOnFailure", "true"),
                    new KeyValuePair<string, string>("f", "json")
                });

                var response = await _httpClient.PostAsync($"{url}/addFeatures", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var responseData = JObject.Parse(responseBody);

                if (response.StatusCode != HttpStatusCode.OK || responseData["error"] != null)
                {
                    _logger.LogCritical($"Unable to create features in arcgis for node with value {node.Value}");
                    _logger.LogCritical($"Response {responseBody}");
                }
                else
                {
                    _logger.LogInformation($"Successfully created arcgis features for object location tracking with value {node.Value}");
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

    class ArcgisFeature {
        public PointGeometry Geometry { get; set; }
        public GeoMaterialAttributes Attributes { get; set; }

        public ArcgisFeature(Guid entityId, FieldValue value) {
            Geometry = new PointGeometry(value.Lng, value.Lat);
            Attributes = new GeoMaterialAttributes(entityId, value);
        }

        public class PointGeometry {
            public readonly double X;
            public readonly double Y;

            public PointGeometry(double lng, double lat)
            {
                X = lng;
                Y = lat;
            }
        }

        public class GeoMaterialAttributes {
            public readonly double Lat;
            public readonly double Lng;
            public readonly DateTime RegisteredAt;
            public readonly Guid EntityId;

            public GeoMaterialAttributes(Guid entityId, FieldValue value)
            {
                EntityId = entityId;
                Lat = value.Lat;
                Lng = value.Lng;
                RegisteredAt = value.RegisteredAt;
            }
        }
    }
}
