using Iis.Interfaces.Ontology.Data;
using System;

namespace Iis.Domain
{
    public class IncomingRelation
    {
        public Guid RelationId { get; set; }
        public string RelationTypeName { get; set; }
        public string RelationTypeTitle { get; set; }
        public Guid EntityId { get; set; }
        public string EntityTypeName { get; set; }
        public Node Entity { get; set; }
    }
}