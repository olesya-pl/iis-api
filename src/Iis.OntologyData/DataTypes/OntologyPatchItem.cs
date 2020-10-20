using Iis.Interfaces.Ontology.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class OntologyPatchItem : IOntologyPatchItem
    {
        internal List<NodeData> _nodes = new List<NodeData>();
        public IReadOnlyCollection<INodeBase> Nodes => _nodes;

        internal List<RelationData> _relations = new List<RelationData>();
        public IReadOnlyCollection<IRelationBase> Relations => _relations;

        internal List<AttributeData> _attributes = new List<AttributeData>();
        public IReadOnlyCollection<IAttributeBase> Attributes => _attributes;

        public void Clear()
        {
            _nodes.Clear();
            _relations.Clear();
            _attributes.Clear();
        }
    }
}
