using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class RelationRestriction
    {
        public int Id { get; set; }
        public int RelationTypeId { get; set; }
        public int InitiatorTypeId { get; set; }
        public int TargetTypeId { get; set; }
        public string Meta { get; set; }

        public virtual EntityType Source { get; set; }
        public virtual EntityType RelationType { get; set; }
        public virtual EntityType Target { get; set; }
    }
}
