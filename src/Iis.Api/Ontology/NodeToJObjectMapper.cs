using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Iis.Domain;
using Iis.Utility;
using Iis.Interfaces.Ontology.Schema;
using Node = Iis.Domain.Node;

namespace Iis.Api.Ontology
{
    public class NodeMapper
    {
        private readonly IOntologyService _ontologyService;        

        public NodeMapper(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }        

        public EventAssociatedWithEntity ToEventToAssociatedWithEntity(Node node)
        {
            var result = new JObject(new JProperty(nameof(node.Id).ToLower(), node.Id.ToString("N")));

            var importanceId = node.Nodes.FirstOrDefault(p => p.Type.Name == "importance").Nodes.First().Id;
            var stateId = node.Nodes.FirstOrDefault(p => p.Type.Name == "state").Nodes.First().Id;

            var importanceAttributes = (_ontologyService.GetNode(importanceId)).GetChildAttributes();
            var stateAttributes = (_ontologyService.GetNode(stateId)).GetChildAttributes();

            var attributies = node.GetTopLevelAttributes();

            var associatedEvent = new EventAssociatedWithEntity {
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

            return associatedEvent;
        }        
    }
}
