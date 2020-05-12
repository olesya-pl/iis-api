using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Files.EntityFramework;
using IIS.Core.GraphQL.Materials;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Http;
using System.Net;
using Iis.DataModel.Materials;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Iis.DataModel;

namespace IIS.Core.Materials
{
    public class MaterialEventConsumer : BackgroundService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<MaterialEventConsumer> _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly FileServiceFactory _fileServiceFactory;
        private readonly OntologyContext _context;
        private readonly IGsmTranscriber _gsmTranscriber;
        private readonly IConfiguration _config;


        public MaterialEventConsumer(
            ILogger<MaterialEventConsumer> logger,
            IConnectionFactory connectionFactory,
            FileServiceFactory fileServiceFactory,
            OntologyContext context,
            IGsmTranscriber gsmTranscriber,
            IConfiguration config
        )
        {
            _logger = logger;
            _connectionFactory = connectionFactory;
            _fileServiceFactory = fileServiceFactory;
            _context = context;
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
                    await _createFeaturesInArcgis(eventData);
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
                    var mi = new MaterialInfoEntity
                    {
                        Id = Guid.NewGuid(),
                        Data = data?.ToString(),
                        MaterialId = eventData.MaterialId,
                        Source = "ML",
                        SourceType = "GSM",
                        SourceVersion = "xz"
                    };
                    _context.Add(mi);
                    await _context.SaveChangesAsync();
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

        private async Task _createFeaturesInArcgis(MaterialAddedEvent eventData)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            foreach (var node in eventData.Nodes.Where(n => n.UpdateField != null))
            {
                var features = node.UpdateField.Values.Select(value => new ArcgisFeature(eventData.EntityId, value));
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

            private const double M_PI_4 = Math.PI / 4.0;
            private const double D_R = Math.PI / 180.0;
            private const double R_MAJOR = 6378137.0;

            public PointGeometry(double lng, double lat)
            {
                LatLong2SpherMerc(lng, lat, out X, out Y);
            }

            private static double deg_rad(double ang)
            {
                return ang * D_R;
            }

            void LatLong2SpherMerc(double lon, double lat, out double x, out double y)
            {
                lat = Math.Min(89.5, Math.Max(lat, -89.5));
                x = R_MAJOR * deg_rad(lon);
                y = R_MAJOR * Math.Log(Math.Tan(M_PI_4 + deg_rad(lat) / 2));
            }
        }

        public class GeoMaterialAttributes {
            public readonly double Lat;
            public readonly double Lng;
            public readonly DateTime RegisteredAt;
            public readonly string EntityId;

            public GeoMaterialAttributes(Guid entityId, FieldValue value)
            {
                EntityId = entityId.ToString("N");
                Lat = value.Lat;
                Lng = value.Lng;
                RegisteredAt = value.RegisteredAt;
            }
        }
    }
}
