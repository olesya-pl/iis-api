using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IOntologyNodesData
    {
        IEnumerable<INode> Nodes { get; }
        IEnumerable<IRelation> Relations { get; }
        IEnumerable<IAttribute> Attributes { get; }
        IOntologySchema Schema { get; }
        IOntologyPatch Patch { get; }
        void ClearPatch();
        INode GetNode(Guid id);
        IReadOnlyList<INode> GetNodes(IEnumerable<Guid> ids);
        IReadOnlyList<INode> GetEntitiesByTypeName(string typeName);
        IReadOnlyList<INode> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
    }
}