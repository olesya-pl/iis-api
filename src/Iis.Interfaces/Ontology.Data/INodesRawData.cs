using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Data
{
    public interface INodesRawData
    {
        IReadOnlyList<IAttributeBase> Attributes { get; }
        IReadOnlyList<INodeBase> Nodes { get; }
        IReadOnlyList<IRelationBase> Relations { get; }
    }
}