﻿using AutoMapper;
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
        }

        internal NodeData CreateNode(Guid nodeTypeId, Guid? id = null)
        {
            var node = new NodeData
            {
                Id = id ?? Guid.NewGuid(),
                NodeTypeId = nodeTypeId,
                NodeType = _schema.GetNodeTypeById(nodeTypeId),
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                IsArchived = false
            };
            CompleteNode(node);
            Nodes[node.Id] = node;
            _patch._create._nodes.Add(node);
            return node;
        }

        internal RelationData CreateRelation(Guid sourceNodeId, Guid targetNodeId, Guid nodeTypeId, Guid? id = null)
        {
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
            _patch._create._relations.Add(relation);
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
            _patch._create._attributes.Add(attribute);
            return attribute;
        }
        internal RelationData CreateRelationWithAttribute(Guid sourceNodeId, Guid nodeTypeId, string value)
        {
            var attributeTypeId = _schema.GetNodeTypeById(nodeTypeId).RelationType.TargetTypeId;
            var node = CreateNode(attributeTypeId);
            CreateAttribute(node.Id, value);
            return CreateRelation(sourceNodeId, node.Id, nodeTypeId);
        }
        internal RelationData UpdateRelationTarget(Guid id, Guid targetId)
        {
            var relation = Relations[id];
            if (relation.TargetNodeId == targetId) return relation;

            relation._targetNode._incomingRelations.Remove(relation);
            relation.TargetNodeId = targetId;
            relation._targetNode = Nodes[targetId];
            relation._targetNode._incomingRelations.Add(relation);
            _patch._update._relations.Add(relation);
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
                    _patch._update._nodes.Add(relation._targetNode);
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
                    Relations.Remove(node.Id);
                }
                Nodes.Remove(node.Id);
            }

            foreach (var node in Nodes.Values)
            {
                node._outgoingRelations.RemoveAll(r => r.Node.IsArchived);
                node._incomingRelations.RemoveAll(r => r.Node.IsArchived);
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
            node.IsArchived = true;
            _patch._update._nodes.Add(node);
        }
        public void RemoveNode(Guid id)
        {
            SetNodeIsArchived(id);
            MarkLinkedAsArchived(Nodes[id]);
            RemoveArchivedItems();
        }
    }
}
