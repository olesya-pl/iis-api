using System;
using MediatR;

namespace Iis.Ontology.DataRead.Concept
{
    public sealed class GetEntityConceptQuery : IRequest<EntityConcept>
    {
        public Guid EntityId { get; }

        public GetEntityConceptQuery(Guid entityId)
        {
            EntityId = entityId;
        }
    }
}