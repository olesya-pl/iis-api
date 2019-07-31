using System;

namespace IIS.Core.Ontology
{
    public abstract class RelationType : Type
    {
        protected RelationType(Guid id, string name)
            : base(id, name)
        {

        }
    }
}
