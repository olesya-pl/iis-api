using System;
using System.Linq;
using System.Threading.Tasks;

using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.Utility;

using Newtonsoft.Json.Linq;

using Node = Iis.Domain.Node;

namespace Iis.Api.Ontology
{
    public class NodeToJObjectMapper
    {
        private readonly IOntologyService _ontologyService;
        private readonly FileUrlGetter _fileUrlGetter;
        private readonly string[] EventDotNames = new[] {
            "name", "description", "updatedAt", "startsAt", "endsAt" };

        public NodeToJObjectMapper(IOntologyService ontologyService,
            FileUrlGetter fileUrlGetter)
        {
            _ontologyService = ontologyService;
            _fileUrlGetter = fileUrlGetter;
        }

        public JObject NodeToJObject(Node node)
        {
            if (node == null)
                return null;

            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            foreach (var attribute in node.GetChildAttributesExcludingNestedObjects().GroupBy(p => p.DotName))
            {
                if (attribute.Key == "attachment" || attribute.Key == "photo")
                {
                    result.Add(new JProperty(attribute.Key, attribute.Select(p =>
                    {
                        var res = new JObject();
                        
                        res.Add("id", p.Attribute.Value.ToString());
                        res.Add("url", _fileUrlGetter.GetFileUrl((Guid)p.Attribute.Value));
                        return res;
                    })));
                }
                else if (attribute.Count() == 1)
                {
                    result.Add(new JProperty(attribute.Key, attribute.FirstOrDefault().Attribute.Value.ToString()));
                }
                else
                {
                    result.Add(new JProperty(attribute.Key, attribute.Select(p => p.Attribute.Value.ToString()).ToList()));
                }
            }
            INodeTypeLinked nodeType = node.OriginalNode.NodeType;
            var titleRelationType = nodeType.GetRelationTypeByName("__title");
            if (titleRelationType != null)
            {
                var formula = titleRelationType.NodeType.MetaObject.Formula;
                if (!string.IsNullOrWhiteSpace(formula))
                {
                    result.Add(new JProperty("__title", node.OriginalNode.ResolveFormula(formula)));
                }
            }

            return result;
        }

        public async Task<EventAssociatedWithEntity> EventToAssociatedWithEntity(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            var importanceId = node.Nodes.FirstOrDefault(p => p.Type.Name == "importance").Nodes.First().Id;
            var stateId = node.Nodes.FirstOrDefault(p => p.Type.Name == "state").Nodes.First().Id;

            var importanceAttributes = (_ontologyService.LoadNodes(importanceId)).GetChildAttributes();
            var stateAttributes = (_ontologyService.LoadNodes(stateId)).GetChildAttributes();

            var attributies = node.GetChildAttributes();

            var res = new EventAssociatedWithEntity {
                Id = node.Id,
                Name = attributies.FirstOrDefault(p => p.dotName == "name").attribute?.Value as string,
                Description = attributies.FirstOrDefault(p => p.dotName == "description").attribute?.Value as string,
                UpdatedAt = attributies.FirstOrDefault(p => p.dotName == "updatedAt").attribute?.Value as DateTime?,
                StartsAt = attributies.FirstOrDefault(p => p.dotName == "startsAt").attribute?.Value as DateTime?,
                EndsAt = attributies.FirstOrDefault(p => p.dotName == "endsAt").attribute?.Value as DateTime?,
                State = new EventState {
                    Id = stateId,
                    Name = stateAttributes.FirstOrDefault(p => p.dotName == "name").attribute?.Value as string,
                    Code = stateAttributes.FirstOrDefault(p => p.dotName == "code").attribute?.Value as string
                },
                Importance = new EventImportance
                {
                    Id = importanceId,
                    Name = importanceAttributes.FirstOrDefault(p => p.dotName == "name").attribute?.Value as string,
                    Code = importanceAttributes.FirstOrDefault(p => p.dotName == "code").attribute?.Value as string
                }
            };

            return res;
        }

        public JObject EventToJObject(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            var attributies = node.GetChildAttributesExcludingNestedObjects()
                .Where(a => EventDotNames.Contains(a.DotName));

            foreach (var attribute in attributies)
            {
                result.Add(new JProperty(attribute.DotName, attribute.Attribute.Value));
            }

            return result;
        }
    }
}
