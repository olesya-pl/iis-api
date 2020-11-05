using System;
using HotChocolate;
using Iis.Domain;

namespace Iis.Api.Ontology
{
    public class RelationsCountQuery
    {
        public int GetRelationsCount([Service] IOntologyService ontologyService,
            Guid entityId)
        {
            return ontologyService.GetRelationsCount(entityId);
        }
    }
}
