using HotChocolate;
using Iis.Api.GraphQL.Entities.ObjectTypes;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.Entities
{
    public class ObjectSignQuery
    {
        [GraphQLNonNullType]
        public async Task<GraphQLCollection<OntologyValue>> GetValuesByEntityType(
            [Service]IOntologyService ontologyService,
            [Service]IOntologySchema schema,
            string entityType,
            string value,
            int limit)
        {
            var nodeType = schema.GetEntityTypeByName(entityType);
            if (nodeType == null)
            {
                throw new Exception($"Node type is not found: {entityType}");
            }
            var list = (await ontologyService.GetNodesByUniqueValue(nodeType.Id, value, "value", limit))
                .Select(a => new OntologyValue { Id = a.Id, Value = a.Value })
                .ToList();
            return new GraphQLCollection<OntologyValue>(list, list.Count);
        }
    }
}
