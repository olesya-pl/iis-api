using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IOntologyPatchItem
    {
        IReadOnlyCollection<INodeBase> Nodes { get; }
        IReadOnlyCollection<IRelationBase> Relations { get; }
        IReadOnlyCollection<IAttributeBase> Attributes { get; }
        void Clear();
    }
}
