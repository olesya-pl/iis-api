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

        public DataStorage(INodesRawData rawData, IMapper mapper, IOntologySchema schema)
        {
            _mapper = mapper;
            _schema = schema;
            Initialize(rawData);
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
                node.IsArchived = true;
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
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                IsArchived = false
            };
            CompleteNode(node);
            Nodes[node.Id] = node;
            _patch._create._nodes.Add(node);
            return node;
        }

        internal RelationData CreateRelation(Guid id, Guid sourceNodeId, Guid targetNodeId)
        {
            var relation = new RelationData
            {
                Id = id,
                SourceNodeId = sourceNodeId,
                TargetNodeId = targetNodeId
            };
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

        private void MarkLinkedAsArchived(NodeData node)
        {
            foreach (var relation in node._outgoingRelations)
            {
                relation._node.IsArchived = true;
                switch (relation.TargetKind)
                {
                    case Kind.Attribute:
                        relation._targetNode.IsArchived = true;
                        break;
                    case Kind.Entity:
                        if (!relation.IsLinkToSeparateObject && !relation.TargetNode.IsArchived)
                        {
                            relation._targetNode.IsArchived = true;
                            MarkLinkedAsArchived(relation._targetNode);
                        }
                        break;
                }
            }

            foreach (var relation in node._incomingRelations)
            {
                relation._node.IsArchived = true;
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
        }
        private void CompleteAttribute(AttributeData attribute)
        {
            attribute._node = Nodes[attribute.Id];
            attribute._node._attribute = attribute;
        }
        public void ClearPatch()
        {
            _patch = new OntologyPatch();
        }
        public void SetNodeIsArchived(Guid id)
        {
            var node = Nodes[id];
            node.IsArchived = true;
            _patch._update._nodes.Add(Nodes[id]);
        }
    }
}
