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
    public class OntologyNodesData : IOntologyNodesData
    {
        DataStorage _storage;
        IMapper _mapper;
        IOntologySchema _schema;

        public IEnumerable<INode> Nodes => _storage.Nodes.Values;
        public IEnumerable<IRelation> Relations => _storage.Relations.Values;
        public IEnumerable<IAttribute> Attributes => _storage.Attributes.Values;

        public IOntologySchema Schema => _schema;
        public IOntologyPatch Patch => _storage.Patch;

        public OntologyNodesData(INodesRawData rawData, IOntologySchema schema)
        {
            _mapper = GetMapper();
            _schema = schema;
            _storage = new DataStorage(rawData, _mapper, _schema);
        }
        private IMapper GetMapper()
        {
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<INodeBase, NodeData>();
                cfg.CreateMap<IRelationBase, RelationData>();
                cfg.CreateMap<IAttributeBase, AttributeData>();
            });

            return new Mapper(configuration);
        }
        public IReadOnlyList<INode> GetEntitiesByTypeName(string typeName)
        {
            return _storage.Nodes.Values
                .Where(n => n.NodeType.Kind == Kind.Entity
                    && n.NodeType.Name == typeName)
                .ToList();
        }
        internal NodeData CreateNode(Guid nodeTypeId, Guid? id = null) =>
            _storage.CreateNode(nodeTypeId, id);

        internal RelationData CreateRelation(Guid id, Guid sourceNodeId, Guid targetNodeId) =>
            _storage.CreateRelation(id, sourceNodeId, targetNodeId);

        internal AttributeData CreateAttribute(Guid id, string value) =>
            _storage.CreateAttribute(id, value);

        internal NodeData GetNodeData(Guid id)
        {
            return _storage.Nodes[id];
        }
        internal IReadOnlyList<NodeData> GetNodesData(IEnumerable<Guid> ids)
        {
            return ids.Select(id => _storage.Nodes[id]).ToList();
        }


        internal void AddValueByDotName(NodeData entity, string value, string[] dotNameParts)
        {
            var node = entity;
            for (int i = 0; i < dotNameParts.Length; i++)
            {
                var isLastItem = i == dotNameParts.Length - 1;

                var relationType = node.NodeType.GetRelationTypeByName(dotNameParts[i]);
                if (relationType == null)
                {
                    throw new Exception($"Cannot find embedded type {dotNameParts[i]} for entity type {node.NodeType.Name}");
                }

                if (!isLastItem)
                {
                    var existingRelation = entity._outgoingRelations
                        .SingleOrDefault(r => r.Node.NodeTypeId == relationType.Id);
                    if (existingRelation != null)
                    {
                        node = existingRelation._targetNode;
                        continue;
                    }
                }

                if (isLastItem && relationType.TargetType.IsSeparateObject)
                {
                    var linkNode = _storage.CreateNode(relationType.Id);
                    _storage.CreateRelation(linkNode.Id, node.Id, Guid.Parse(value));
                    break;
                }

                var relationNode = _storage.CreateNode(relationType.Id);
                var targetNode = _storage.CreateNode(relationType.TargetTypeId);
                _storage.CreateRelation(relationNode.Id, node.Id, targetNode.Id);

                if (isLastItem)
                {
                    _storage.CreateAttribute(targetNode.Id, value);
                }

                node = targetNode;
            }
        }
        public void ClearPatch()
        {
            _storage.ClearPatch();
        }
        public void SetNodeIsArchived(Guid nodeId)
        {
            _storage.SetNodeIsArchived(nodeId);
        }
        public IReadOnlyList<INode> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            return Nodes
                .Where(r => r.NodeTypeId == nodeTypeId
                    && r.GetChildNode(valueTypeName)?.Value == value)
                .ToList();
        }
        public INode GetNode(Guid id) => GetNodeData(id);
        public IReadOnlyList<INode> GetNodes(IEnumerable<Guid> ids) => GetNodesData(ids);
    }
}