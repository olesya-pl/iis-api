using System;

namespace Iis.Domain
{
    public class IncomingRelation
    {
        public string RelationTypeName { get; set; }
        public string RelationTypeTitle { get; set; }
        public Guid EntityId { get; set; }
        public string EntityTypeName { get; set; }
    }
}