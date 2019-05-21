using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Ontology.EntityFramework
{
    public abstract class OType
    {
        public OType()
        {
            AttributeRestrictions = new HashSet<OAttributeRestriction>();
            DerivedTypes = new HashSet<OType>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public bool IsAbstract { get; set; }
        public JObject Meta { get; set; }
        public EntityKind Kind { get; set; }
        public int? ParentId { get; set; }

        public virtual ICollection<OAttributeRestriction> AttributeRestrictions { get; set; }
        public virtual OType Parent { get; set; }
        public virtual ICollection<OType> DerivedTypes { get; set; }
    }

    public class OTypeEntity : OType
    {
        public virtual ICollection<ORestriction> ForwardRestrictions { get; set; }
        public virtual ICollection<ORestriction> BackwardRestrictions { get; set; }
        public virtual ICollection<OEntity> Entities { get; set; }

        public new OTypeEntity Parent
        {
            get { return (OTypeEntity)base.Parent; }
            set { base.Parent = value; }
        }

        public OTypeEntity()
        {
            Entities = new HashSet<OEntity>();
            ForwardRestrictions = new HashSet<ORestriction>();
            BackwardRestrictions = new HashSet<ORestriction>();
        }
    }

    public class OTypeRelation : OType
    {
        public virtual ICollection<ORestriction> Restrictions { get; set; }
        public virtual ICollection<ORelation> Relations { get; set; }

        public OTypeRelation()
        {
            Restrictions = new HashSet<ORestriction>();
            Relations = new HashSet<ORelation>();
        }
    }
}
