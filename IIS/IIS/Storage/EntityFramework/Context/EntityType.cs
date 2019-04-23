using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityType
    {
        public EntityType()
        {
            Entities = new HashSet<Entity>();
            ForwardRelationRestrictions = new HashSet<RelationRestriction>();
            RelationRestrictions = new HashSet<RelationRestriction>();
            BackwardRelationRestrictions = new HashSet<RelationRestriction>();
            EntityRelations = new HashSet<Relation>();
            EntityTypeAttributes = new HashSet<EntityTypeAttribute>();
            DerivedTypes = new HashSet<EntityType>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public bool IsAbstract { get; set; }
        public string Meta { get; set; }
        public string Type { get; set; }
        public int? ParentId { get; set; }

        public virtual EntityType Parent { get; set; }
        public virtual ICollection<EntityTypeAttribute> EntityTypeAttributes { get; set; }
        public virtual ICollection<RelationRestriction> ForwardRelationRestrictions { get; set; }
        public virtual ICollection<RelationRestriction> BackwardRelationRestrictions { get; set; }
        public virtual ICollection<Relation> EntityRelations { get; set; }
        public virtual ICollection<Entity> Entities { get; set; }
        public virtual ICollection<RelationRestriction> RelationRestrictions { get; set; }
        public virtual ICollection<EntityType> DerivedTypes { get; set; }
    }
}
