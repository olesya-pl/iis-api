using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class OntologyPatchItem : IOntologyPatchItem
    {
        private Dictionary<Guid, NodeData> _nodes = new Dictionary<Guid, NodeData>();
        public IReadOnlyCollection<INodeBase> Nodes => _nodes.Values;

        private Dictionary<Guid, RelationData> _relations = new Dictionary<Guid, RelationData>();
        public IReadOnlyCollection<IRelationBase> Relations => _relations.Values;

        private Dictionary<Guid, AttributeData> _attributes = new Dictionary<Guid, AttributeData>();
        public IReadOnlyCollection<IAttributeBase> Attributes => _attributes.Values;
        internal void Add(NodeData node) => _nodes[node.Id] = node;
        internal void Add(RelationData relation) => _relations[relation.Id] = relation;
        internal void Add(AttributeData attribute) => _attributes[attribute.Id] = attribute;
        internal bool NodeExists(Guid id) => _nodes.ContainsKey(id);
        internal bool RelationExists(Guid id) => _relations.ContainsKey(id);
        internal bool AttributeExists(Guid id) => _attributes.ContainsKey(id);

        public void Clear()
        {
            _nodes.Clear();
            _relations.Clear();
            _attributes.Clear();
        }
    }
}
