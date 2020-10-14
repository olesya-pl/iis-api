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
    public class OntologyNodesData : IOntologyNodesData
    {
        DataStorage _storage;
        IMapper _mapper;
        public IOntologySchema Schema { get; }

        public IEnumerable<INode> Nodes => _storage.Nodes.Values;
        public IEnumerable<IRelation> Relations => _storage.Relations.Values;
        public IEnumerable<IAttribute> Attributes => _storage.Attributes.Values;

        public IOntologyPatch Patch => _storage.Patch;

        internal ReadWriteLocker Locker { get; set; }  = new ReadWriteLocker();

        public OntologyNodesData(INodesRawData rawData, IOntologySchema schema)
        {
            _mapper = GetMapper();
            Schema = schema;
            _storage = new DataStorage(rawData, _mapper, Schema, this);
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
            return Locker.ReadLock(() => 
                _storage.Nodes.Values
                .Where(n => n.NodeType.Kind == Kind.Entity
                    && n.NodeType.Name == typeName)
                .ToList());
        }
        internal NodeData CreateNode(Guid nodeTypeId, Guid? id = null) =>
            _storage.CreateNode(nodeTypeId, id);

        internal RelationData CreateRelation(Guid id, Guid sourceNodeId, Guid targetNodeId) =>
            _storage.CreateRelation(id, sourceNodeId, targetNodeId);

        internal AttributeData CreateAttribute(Guid id, string value) =>
            _storage.CreateAttribute(id, value);

        internal NodeData GetNodeData(Guid id)
        {
            return Locker.ReadLock(() => _storage.Nodes[id]);
        }
        internal IReadOnlyList<NodeData> GetNodesData(IEnumerable<Guid> ids)
        {
            return Locker.ReadLock(() => ids.Select(id => _storage.Nodes[id]).ToList());
        }
        internal void AddValueByDotName(NodeData entity, string value, string[] dotNameParts)
        {
            Locker.WriteLock(() =>
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
            });
        }
        public void ClearPatch()
        {
            _storage.ClearPatch();
        }
        public void SetNodeIsArchived(Guid nodeId)
        {
            Locker.WriteLock(() => _storage.SetNodeIsArchived(nodeId));
        }
        public void DeleteEntity(Guid id, bool deleteOutcomingRelations, bool deleteIncomingRelations) =>
            Locker.WriteLock(() => _storage.DeleteEntity(id, deleteOutcomingRelations, deleteIncomingRelations));
        public void SetNodesIsArchived(IEnumerable<Guid> nodeIds)
        {
            Locker.WriteLock(() =>
            {
                foreach (var nodeId in nodeIds)
                {
                    _storage.SetNodeIsArchived(nodeId);
                }
            });
        }
        public IReadOnlyList<INode> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName)
        {
            return Locker.ReadLock(() => Nodes
                .Where(r => r.NodeTypeId == nodeTypeId
                    && r.GetChildNode(valueTypeName)?.Value == value)
                .ToList());
        }
        public INode GetNode(Guid id) => GetNodeData(id);
        public IReadOnlyList<INode> GetNodes(IEnumerable<Guid> ids) => GetNodesData(ids);
        public IReadOnlyList<INode> GetNodesByTypeIds(IEnumerable<Guid> nodeTypeIds)
        {
            return Locker.ReadLock(() => Nodes.Where(n => nodeTypeIds.Contains(n.NodeTypeId)).ToList());
        }
        public IReadOnlyList<INode> GetNodesByTypeId(Guid nodeTypeId)
        {
            return Locker.ReadLock(() => Nodes.Where(n => n.NodeTypeId == nodeTypeId).ToList());
        }
    }
}