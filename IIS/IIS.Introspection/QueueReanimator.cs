using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IIS.Core;
using IIS.Core.Ontology;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace IIS.Introspection
{
    public class QueueReanimator
    {
        private readonly ConnectionFactory _mqFactory;
        private readonly IOntology _ontology;
        private readonly IOSchema _schema;

        public QueueReanimator(ConnectionFactory mqFactory, IOntology ontology, IOSchema schema)
        {
            _mqFactory = mqFactory;
            _ontology = ontology;
            _schema = schema;
        }

        public async Task RestoreOntology()
        {
            using (var connection = _mqFactory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "entities",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
                    var root = await _schema.GetRootAsync();
                    foreach (var type in root.GetEntities())
                    {
                        var limit = 20;
                        for (int offset = 0; ; offset += limit)
                        {
                            var entities = await _ontology.GetEntitiesAsync(type.Name, limit, offset);
                            if (!entities.Any()) break;
                            foreach (var entity in entities)
                            {
                                var typeName = type.HasParent ? (type.Parent.Name + type.Name).ToUnderscore() : type.Name.ToUnderscore();
                                var stringBuilder = new StringBuilder();
                                var textWriter = new StringWriter(stringBuilder);
                                var jsonWriter = new JsonTextWriter(textWriter);
                                var writer = new JsonOntologyWriter(jsonWriter);
                                jsonWriter.WriteStartObject();
                                jsonWriter.WritePropertyName("type");
                                jsonWriter.WriteValue(typeName);
                                jsonWriter.WritePropertyName("entity");
                                entity.AcceptVisitor(writer);
                                jsonWriter.WriteEndObject();

                                var message = stringBuilder.ToString();

                                var body = Encoding.UTF8.GetBytes(message);
                                channel.BasicPublish(exchange: "",
                                                     routingKey: "entities",
                                                     basicProperties: null,
                                                     body: body);
                            }
                        }
                    }
                }
            }
        }
    }
}
