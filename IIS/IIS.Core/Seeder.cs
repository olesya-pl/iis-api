using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Newtonsoft.Json.Linq;

namespace IIS.Core
{
    public class Seeder
    {
        private IOntologyProvider _ontologyProvider;
        private IOntologyService _ontologyService;

        public Seeder(IOntologyProvider ontologyProvider, IOntologyService ontologyService)
        {
            _ontologyProvider = ontologyProvider;
            _ontologyService = ontologyService;
        }

        public async Task Seed(CancellationToken cancellationToken)
        {
            var first = new List<Node>();
            var nodes = new List<Node>();

            var ontology = await _ontologyProvider.GetOntologyAsync(cancellationToken);
            var path = Path.Combine(Environment.CurrentDirectory, "data");
            var fileNames = Directory.EnumerateFiles(path);
            foreach (var fileName in fileNames)
            {
                var typeName = new FileInfo(fileName).Name.Replace(".json", "");
                // tmp hardcode
                //if (typeName == "EventType") continue;
                
                using (var reader = new StreamReader(File.OpenRead(fileName)))
                {
                    var content = reader.ReadToEnd();
                    var jArray = JArray.Parse(content);
                    foreach (var jToken in jArray.AsJEnumerable())
                    {
                        var jObject = (JObject)jToken;
                        var entity = Map(jObject, ontology, typeName, first);
                        nodes.Add(entity);
                    }
                }
            }
            foreach (var node in first)
            {
                await _ontologyService.SaveNodeAsync(node, cancellationToken);
            }
            foreach (var node in nodes)
            {
                await _ontologyService.SaveNodeAsync(node, cancellationToken);
            }
        }

        private Entity Map(JObject jObject, Ontology.Ontology ontology, string typeName, List<Node> first)
        {
            var type = ontology.GetEntityType(typeName);
            var entity = new Entity(Guid.NewGuid(), type);
            foreach (var relationType in type.AllProperties)
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
                    Debug.WriteLine($"WARNING! The key '{relationType.Name}' of type '{typeName}' was not present in a seed file.");
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
                    var attribute = new Ontology.Attribute(Guid.NewGuid(), (AttributeType)relationType.TargetType, jValue.Value);
                    relation.AddNode(attribute);
                    entity.AddNode(relation);
                }
            }
            return entity;
        }
    }
}
