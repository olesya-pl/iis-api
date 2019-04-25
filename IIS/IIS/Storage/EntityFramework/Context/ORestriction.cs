using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class ORestriction
    {
        public int Id { get; set; }
        public int RelationTypeId { get; set; }
        public int InitiatorTypeId { get; set; }
        public int TargetTypeId { get; set; }
        public string Meta { get; set; }

        public virtual OType Source { get; set; }
        public virtual OType RelationType { get; set; }
        public virtual OType Target { get; set; }

        // todo: create field
        public bool IsMultiple { get => Meta.Contains("\"multiple\":true"); }
        public bool IsRequired { get => Meta.Contains("\"required\":true"); }
    }
}
