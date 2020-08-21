using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyData.DataTypes
{
    public class RelationData : IRelation
    {
        public Guid Id { get; set; }
        public Guid SourceNodeId { get; set; }
        public Guid TargetNodeId { get; set; }

        internal NodeData _node;
        public INode Node => _node;

        internal NodeData _sourceNode;
        public INode SourceNode => _sourceNode;

        internal NodeData _targetNode;
        public INode TargetNode => _targetNode;
        public Kind TargetKind => _targetNode.NodeType.Kind;
        public bool IsLinkToSeparateObject => _targetNode.NodeType.IsSeparateObject;
        public string TypeName => _node.NodeType.Name;
    }
}
