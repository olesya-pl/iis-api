//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using IIS.Core;
//using IIS.Core.Ontology;
//using Newtonsoft.Json;
//using ProtoBuf;
//using RabbitMQ.Client;

//namespace IIS.Introspection
//{
//    public class QueueReanimator
//    {
//        private readonly ConnectionFactory _mqFactory;
//        private readonly IOntology _ontology;
//        private readonly IOSchema _schema;
//        private readonly ISchemaProvider _schemaProvider;

//        public QueueReanimator(ConnectionFactory mqFactory, IOntology ontology, IOSchema schema, ISchemaProvider schemaProvider)
//        {
//            _mqFactory = mqFactory;
//            _ontology = ontology;
//            _schema = schema;
//            _schemaProvider = schemaProvider;
//        }

//        public async Task RestoreSchema()
//        {
//            using (var connection = _mqFactory.CreateConnection())
//            {
//                using (var channel = connection.CreateModel())
//                {
//                    channel.QueueDeclare(queue: "schema",
//                                 durable: false,
//                                 exclusive: false,
//                                 autoDelete: false,
//                                 arguments: null);
//                    var schema = await _schemaProvider.GetSchemaAsync();
//                    using (var stream = new MemoryStream())
//                    {
//                        Serializer.Serialize(stream, schema);
//                        var body = stream.ToArray();
//                        channel.BasicPublish(exchange: "",
//                                         routingKey: "schema",
//                                         basicProperties: null,
//                                         body: body);
//                    }
//                }
//            }
//        }

//        public async Task RestoreOntology(CancellationToken cancellationToken)
//        {
//            using (var connection = _mqFactory.CreateConnection())
//            {
//                using (var channel = connection.CreateModel())
//                {
//                    channel.QueueDeclare(queue: "entities",
//                                 durable: false,
//                                 exclusive: false,
//                                 autoDelete: false,
//                                 arguments: null);
//                    var root = await _schema.GetRootAsync();
//                    var typesToReplication = root.GetEntities().Select(e => e.Name);
//                    var tasks = typesToReplication.Select(type => RestoreForType(type, channel, cancellationToken));
//                    await Task.WhenAll(tasks);
//                }
//            }
//        }

//        private async Task RestoreForType(string typeName, IModel channel, CancellationToken cancellationToken)
//        {
//            var entities = await _ontology.GetEntitiesAsync(typeName, cancellationToken);
//            foreach (var entity in entities)
//            {
//                var stringBuilder = new StringBuilder();
//                var textWriter = new StringWriter(stringBuilder);
//                var jsonWriter = new JsonTextWriter(textWriter);
//                var writer = new JsonOntologyWriter(jsonWriter);
//                jsonWriter.WriteStartObject();
//                jsonWriter.WritePropertyName("type");
//                jsonWriter.WriteValue(typeName);
//                jsonWriter.WritePropertyName("entity");
//                entity.AcceptVisitor(writer);
//                jsonWriter.WriteEndObject();

//                var message = stringBuilder.ToString();

//                var body = Encoding.UTF8.GetBytes(message);
//                channel.BasicPublish(exchange: "",
//                                     routingKey: "entities",
//                                     basicProperties: null,
//                                     body: body);
//            }
//        }
//    }
//}
