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
        public RelationKind RelationKind => _node.NodeType.RelationType.Kind;
        public EmbeddingOptions EmbeddingOptions => _node.NodeType.RelationType.EmbeddingOptions;
        public string RelationTypeName => _node.NodeType.Name;
        public bool IsLinkToSeparateObject => _targetNode.NodeType.IsSeparateObject;
        public bool IsLinkToExternalObject => _targetNode.NodeType.IsObject;
        public bool IsLinkToAttribute => _targetNode.NodeType.Kind == Kind.Attribute;
        public bool IsLinkToInternalObject => !IsLinkToAttribute && !IsLinkToExternalObject;
        public string TypeName => _node.NodeType.Name;
        public override string ToString() => $"{Node?.NodeType.Name} {Id}";

    }
}
