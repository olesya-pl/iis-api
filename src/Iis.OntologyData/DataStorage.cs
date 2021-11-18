using AutoMapper;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyData
{
    internal class DataStorage
    {
        public Dictionary<Guid, NodeData> Nodes { get; private set; }
        public Dictionary<Guid, RelationData> Relations { get; private set; }
        public Dictionary<Guid, AttributeData> Attributes { get; private set; }

        private OntologyPatch _patch = new OntologyPatch();
        public IOntologyPatch Patch => _patch;

        IMapper _mapper;
        IOntologySchema _schema;
        OntologyNodesData _data;

        public DataStorage(INodesRawData rawData, IMapper mapper, IOntologySchema schema, OntologyNodesData data)
        {
            _mapper = mapper;
            _schema = schema;
            _data = data;
            Initialize(rawData);
            _patch = new OntologyPatch();
        }
        public void Initialize(INodesRawData rawData)
        {
            Nodes = rawData.Nodes.ToDictionary(n => n.Id, n => _mapper.Map<NodeData>(n));
            Relations = rawData.Relations.ToDictionary(r => r.Id, r => _mapper.Map<RelationData>(r));
            Attributes = rawData.Attributes.ToDictionary(a => a.Id, a => _mapper.Map<AttributeData>(a));

            foreach (var relation in Relations.Values)
            {
                CompleteRelation(relation);
            }

            foreach (var attribute in Attributes.Values)
            {
                CompleteAttribute(attribute);
            }

            foreach (var node in Nodes.Values)
            {
                CompleteNode(node);
            }

            foreach (var node in Nodes.Values.Where(n => n.IsArchived || n.NodeType == null))
            {
                SetNodeIsArchived(node);
                MarkLinkedAsArchived(node);
            }

            RemoveArchivedItems();
            Patch.Clear();
        }

        internal NodeData CreateNode(Guid nodeTypeId, Guid? id = null)
        {
            var node = new NodeData
            {
                Id = id ?? Guid.NewGuid(),
                NodeTypeId = nodeTypeId,
                NodeType = _schema.GetNodeTypeById(nodeTypeId),
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            };
            CompleteNode(node);
            Nodes[node.Id] = node;
            _patch.AddAsCreated(node);
            return node;
        }

        internal RelationData CreateRelation(Guid sourceNodeId, Guid targetNodeId, Guid nodeTypeId, Guid? id = null)
        {
            var sourceNode = Nodes[sourceNodeId];
            var existing = sourceNode._outgoingRelations
                .FirstOrDefault(r => r.Node.NodeTypeId == nodeTypeId && r.TargetNodeId == targetNodeId);

            if (existing != null) return existing;

            var node = CreateNode(nodeTypeId, id);

            var relation = new RelationData
            {
                Id = node.Id,
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId,
            };
            node._relation = relation;
            CompleteRelation(relation);

            Relations[relation.Id] = relation;
            _patch.AddAsCreated(relation);
            return relation;
        }

        internal AttributeData CreateAttribute(Guid id, string value)
        {
            var attribute = new AttributeData
            {
                Id = id,
                Value = value
            };
            CompleteAttribute(attribute);
            Attributes[attribute.Id] = attribute;
            _patch.AddAsCreated(attribute);
            return attribute;
        }
        internal RelationData CreateRelationWithAttribute(Guid sourceNodeId, Guid nodeTypeId, string value)
        {
            var attributeTypeId = _schema.GetNodeTypeById(nodeTypeId).RelationType.TargetTypeId;
            var node = CreateNode(attributeTypeId);
            CreateAttribute(node.Id, value);
            return CreateRelation(sourceNodeId, node.Id, nodeTypeId);
        }
        internal RelationData CreateRelationWithAttribute(Guid sourceNodeId, string relationTypeName, string value)
        {
            var sourceNode = Nodes[sourceNodeId];
            var nodeType = _schema.GetNodeTypeById(sourceNode.NodeTypeId);
            var relationType = nodeType.GetProperty(relationTypeName);
            var node = CreateNode(relationType.RelationType.TargetTypeId);
            CreateAttribute(node.Id, value);
            return CreateRelation(sourceNodeId, node.Id, relationType.Id);
        }
        internal RelationData UpdateRelationTarget(Guid id, Guid targetId)
        {
            var relation = Relations[id];
            if (relation.TargetNodeId == targetId) return relation;

            relation._targetNode._incomingRelations.Remove(relation);
            relation.TargetNodeId = targetId;
            relation._targetNode = Nodes[targetId];
            relation._targetNode._incomingRelations.Add(relation);
            _patch.AddAsUpdated(relation);
            return relation;
        }

        private void MarkLinkedAsArchived(NodeData node)
        {
            foreach (var relation in node._outgoingRelations)
            {
                SetNodeIsArchived(relation._node);
                if (relation.TargetNode.NodeType != null && !relation.IsLinkToSeparateObject && !relation.TargetNode.IsArchived)
                {
                    relation._targetNode.IsArchived = true;
                    _patch.AddAsUpdated(relation._targetNode);
                    MarkLinkedAsArchived(relation._targetNode);
                }
            }

            foreach (var relation in node._incomingRelations)
            {
                SetNodeIsArchived(relation._node);
            }
        }

        private void RemoveArchivedItems()
        {
            foreach (var node in Nodes.Values.Where(n => n.IsArchived).ToList())
            {
                if (Relations.ContainsKey(node.Id))
                {
                    Relations.Remove(node.Id);
                }

                if (Attributes.ContainsKey(node.Id))
                {
                    Attributes.Remove(node.Id);
                }
                Nodes.Remove(node.Id);
            }

            foreach (var node in Nodes.Values)
            {
                node._outgoingRelations.RemoveAll(r => r.Node.IsArchived || r.TargetNode.IsArchived || r.SourceNode.IsArchived);
                node._incomingRelations.RemoveAll(r => r.Node.IsArchived || r.TargetNode.IsArchived || r.SourceNode.IsArchived);
            }
        }
        private void CompleteRelation(RelationData relation)
        {
            relation._node = Nodes[relation.Id];
            relation._sourceNode = Nodes[relation.SourceNodeId];
            relation._targetNode = Nodes[relation.TargetNodeId];
            relation._node._relation = relation;
            relation._sourceNode._outgoingRelations.Add(relation);
            relation._targetNode._incomingRelations.Add(relation);
        }
        private void CompleteNode(NodeData node)
        {
            node.NodeType = _schema.GetNodeTypeById(node.NodeTypeId);
            node.AllData = _data;
        }
        private void CompleteAttribute(AttributeData attribute)
        {
            attribute._node = Nodes[attribute.Id];
            attribute._node.Attribute = attribute;
        }
        public void ClearPatch()
        {
            _patch = new OntologyPatch();
        }

        public void SetNodeIsArchived(Guid id) => SetNodeIsArchived(Nodes[id]);

        public void SetNodeIsArchived(NodeData node)
        {
            UpdateNodeProperty(node, e => e.IsArchived = true);
        }

        public void SetNodeUpdatedAt(Guid id, DateTime updatedAt) => SetNodeUpdatedAt(Nodes[id], updatedAt);

        public void SetNodeUpdatedAt(NodeData node, DateTime updatedAt)
        {
            UpdateNodeProperty(node, e => e.UpdatedAt = updatedAt);
        }

        public void RemoveNodeAndRelations(Guid id)
        {
            SetNodeIsArchived(id);
            MarkLinkedAsArchived(Nodes[id]);
            RemoveArchivedItems();
        }

        public void RemoveNode(Guid id)
        {
            SetNodeIsArchived(id);
            RemoveArchivedItems();
        }

        public void RemoveNodes(IEnumerable<Guid> ids)
        {
            foreach (var id in ids)
                SetNodeIsArchived(id);
            RemoveArchivedItems();
        }

        private NodeData UpdateNodeProperty(NodeData node, Action<NodeData> nodeAction)
        {
            nodeAction(node);

            _patch.AddAsUpdated(node);

            return node;
        }

        public void ChangeNodeTypeId(Guid idFrom, Guid idTo)
        {
            var nodes = Nodes.Values.Where(n => n.NodeTypeId == idFrom);
            foreach (var node in nodes)
            {
                node.NodeTypeId = idTo;
                _patch.AddAsUpdated(node);
            }
        }
    }
}
