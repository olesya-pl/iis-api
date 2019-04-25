using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class OType
    {
        public OType()
        {
            Entities = new HashSet<OEntity>();
            ForwardRestrictions = new HashSet<ORestriction>();
            Restrictions = new HashSet<ORestriction>();
            BackwardRestrictions = new HashSet<ORestriction>();
            Relations = new HashSet<ORelation>();
            AttributeRestrictions = new HashSet<OAttributeRestriction>();
            DerivedTypes = new HashSet<OType>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public bool IsAbstract { get; set; }
        public string Meta { get; set; }
        public string Type { get; set; }
        public int? ParentId { get; set; }

        public virtual OType Parent { get; set; }
        public virtual ICollection<OAttributeRestriction> AttributeRestrictions { get; set; }
        public virtual ICollection<ORestriction> ForwardRestrictions { get; set; }
        public virtual ICollection<ORestriction> BackwardRestrictions { get; set; }
        public virtual ICollection<ORelation> Relations { get; set; }
        public virtual ICollection<OEntity> Entities { get; set; }
        public virtual ICollection<ORestriction> Restrictions { get; set; }
        public virtual ICollection<OType> DerivedTypes { get; set; }
    }
}
