using System.Collections.Generic;

namespace Iis.Ontology.DataRead.Concept
{
    public sealed class EntityConcept : NodeConcept
    {
        public bool IsAbstract { get; set; }

        public List<EntityRelation> ParentEntities { get; set; }

        public List<AttributeRelation> AttributeRelations { get; set; }

        public List<EntityRelation> EntityRelations { get; set; }
    }
}