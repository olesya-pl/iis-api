using System;
using System.Linq;
using System.Collections.Generic;
using Iis.Domain;
using Attribute = Iis.Domain.Attribute;
using Node = Iis.Domain.Node;

namespace Iis.Api.Ontology
{
    public class NodeMapper
    {
        private const string ImportancePropertyName = "importance";
        private const string DescriptionPropertyName = "description";
        private const string StatePropertyName = "state";
        private const string NamePropertyName = "name";
        private const string StartsAtPropertyName = "startsAt";
        private const string EndsAtPropertyName = "endsAt";
        private const string CodePropertyName = "code";
        private readonly IOntologyService _ontologyService;

        public NodeMapper(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }

        public EventAssociatedWithEntity ToEventToAssociatedWithEntity(Node node)
        {
            var importanceId = node.Nodes.FirstOrDefault(p => p.Type.Name == ImportancePropertyName).Nodes.First().Id;
            var stateId = node.Nodes.FirstOrDefault(p => p.Type.Name == StatePropertyName).Nodes.First().Id;

            var importanceAttributes = _ontologyService.GetNode(importanceId).GetChildAttributes();
            var stateAttributes = _ontologyService.GetNode(stateId).GetChildAttributes();

            var attributies = node.GetTopLevelAttributes();

            var associatedEvent = new EventAssociatedWithEntity
            {
                Id = node.Id,
                UpdatedAt = node.UpdatedAt,
                Name = GetAttributeValueAsClass<string>(attributies, NamePropertyName),
                Description = GetAttributeValueAsClass<string>(attributies, DescriptionPropertyName),
                StartsAt = GetAttributeValueAsStruct<DateTime>(attributies, StartsAtPropertyName),
                EndsAt = GetAttributeValueAsStruct<DateTime>(attributies, EndsAtPropertyName),
                State = new EventState
                {
                    Id = stateId,
                    Name = GetAttributeValueAsClass<string>(stateAttributes, NamePropertyName),
                    Code = GetAttributeValueAsClass<string>(stateAttributes, CodePropertyName)
                },
                Importance = new EventImportance
                {
                    Id = importanceId,
                    Name = GetAttributeValueAsClass<string>(importanceAttributes, NamePropertyName),
                    Code = GetAttributeValueAsClass<string>(importanceAttributes, CodePropertyName)
                }
            };

            return associatedEvent;
        }

        private static T GetAttributeValueAsClass<T>(IReadOnlyList<(Attribute attribute, string dotName)> attributeList, string attributeName) where T : class
        {
            return attributeList.FirstOrDefault(p => p.dotName == attributeName).attribute?.Value as T;
        }

        private static T? GetAttributeValueAsStruct<T>(IReadOnlyList<(Attribute attribute, string dotName)> attributeList, string attributeName) where T : struct
        {
            return attributeList.FirstOrDefault(p => p.dotName == attributeName).attribute?.Value as T?;
        }
    }
}
