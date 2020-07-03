using Iis.Interfaces.Ontology.Schema;
using System;

namespace Iis.Domain
{
    public abstract class RelationType : NodeType, IRelationTypeModel
    {
        protected RelationType(Guid id, string name)
            : base(id, name)
        {

        }
    }
}
