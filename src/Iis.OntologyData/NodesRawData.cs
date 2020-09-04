using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData
{
    public class NodesRawData : INodesRawData
    {
        public IReadOnlyList<INodeBase> Nodes { get; private set; }
        public IReadOnlyList<IRelationBase> Relations { get; private set; }
        public IReadOnlyList<IAttributeBase> Attributes { get; private set; }

        public NodesRawData()
        {
            Nodes = new List<INodeBase>();
            Relations = new List<IRelationBase>();
            Attributes = new List<IAttributeBase>();
        }
        public NodesRawData(
            IEnumerable<INodeBase> nodes,
            IEnumerable<IRelationBase> relations,
            IEnumerable<IAttributeBase> attributes)
        {
            Nodes = new List<INodeBase>(nodes);
            Relations = new List<IRelationBase>(relations);
            Attributes = new List<IAttributeBase>(attributes);
        }
    }
}
