using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Attribute = Iis.Domain.Attribute;

namespace IIS.Core.Ontology.Seeding
{
    public class Seeder
    {
        private readonly ILogger<Seeder> _logger;
        private IOntologyProvider _ontologyProvider;
        private IOntologyService _ontologyService;

        public Seeder(
            ILogger<Seeder> logger,
            IOntologyProvider ontologyProvider,
            IOntologyService ontologyService)
        {
            _logger = logger;
            _ontologyProvider = ontologyProvider;
            _ontologyService = ontologyService;
        }

        public async Task SeedAsync(string subdir, CancellationToken cancellationToken = default)
        {
            var firstNodes = new List<Node>();
            var nodes = new List<Node>();

            OntologyModel ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
            string path = Path.Combine(Environment.CurrentDirectory, "data", subdir);
            IEnumerable<string> fileNames = Directory.EnumerateFiles(path);
            foreach (string fileName in fileNames)
            {
                string typeName = Path.GetFileNameWithoutExtension(fileName);
                // tmp hardcode
                //if (typeName == "EventType") continue;

                string content = File.ReadAllText(fileName);
                JArray jArray = JArray.Parse(content);
                foreach (JToken jToken in jArray.AsJEnumerable())
                {
                    var jObject = (JObject)jToken;
                    Entity entity = Map(jObject, ontology, typeName, firstNodes);
                    nodes.Add(entity);
                }
            }

            //foreach (var node in firstNodes)
            //{
            //    await _ontologyService.SaveNodeAsync(node, cancellationToken);
            //}
            //foreach (var node in nodes)
            //{
            //    await _ontologyService.SaveNodeAsync(node, cancellationToken);
            //}
            await _ontologyService.SaveNodesAsync(firstNodes.Union(nodes).ToList(), cancellationToken);
        }

        private Entity Map(JObject jObject, OntologyModel ontology, string typeName, List<Node> first)
        {
            EntityType type = ontology.GetEntityType(typeName);
            Entity entity = new Entity(Guid.NewGuid(), type);
            foreach (EmbeddingRelationType relationType in type.AllProperties)
            {
                var jProperty = default(JProperty);
                if (typeName == "Country" && relationType.Name == "code")
                {
                    jProperty = jObject.Property("id");
                }
                else if (typeName == "Country" && relationType.Name == "name")
                {
                    jProperty = jObject.Property("text");
                }
                else if (relationType.Name == "child")
                {
                    jProperty = jObject.Property("children");
                }
                else
                {
                    jProperty = jObject.Property(relationType.Name);
                }

                if (jProperty is null)
                {
                    //_logger.LogTrace("WARNING! The key {relationTypeName} of type {typeName} was not present in a seed file.", relationType.Name, typeName);
                    continue;
                }

                if (relationType.Name == "child")
                {
                    var jArray = (JArray)jProperty.Value;
                    foreach (var jToken in jArray)
                    {
                        var jObject2 = (JObject)jToken;
                        var entity2 = Map(jObject2, ontology, typeName, first);
                        first.Add(entity2);
                        var relation2 = new Relation(Guid.NewGuid(), relationType);
                        relation2.AddNode(entity2);
                        entity.AddNode(relation2);
                    }
                }
                else
                {
                    var relation = new Relation(Guid.NewGuid(), relationType);
                    var jValue = (JValue)jProperty.Value;
                    var attribute = new Attribute(Guid.NewGuid(), (AttributeType)relationType.TargetType, jValue.Value);
                    relation.AddNode(attribute);
                    entity.AddNode(relation);
                }
            }
            return entity;
        }
    }
}