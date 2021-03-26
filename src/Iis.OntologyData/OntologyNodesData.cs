using AutoMapper;
using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyData.DataTypes;
using Iis.OntologyData.IisAccessLevels;
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
        IOntologyPatchSaver _saver;
        public IOntologySchema Schema { get; }

        public IEnumerable<INode> Nodes => _storage.Nodes.Values;

        public IEnumerable<IRelation> Relations => _storage.Relations.Values;

        public IEnumerable<IAttribute> Attributes => _storage.Attributes.Values;

        public IOntologyPatch Patch => _storage.Patch;

        internal ReadWriteLocker Locker { get; set; }

        public OntologyNodesData(INodesRawData rawData, IOntologySchema schema, IOntologyPatchSaver saver)
        {
            _mapper = GetMapper();
            Schema = schema;
            _storage = new DataStorage(rawData, _mapper, Schema, this);
            _saver = saver;
            Locker = new ReadWriteLocker();
            Locker.OnCommingChanges += () => _saver.SavePatch(Patch);
        }

        public void ReloadData(INodesRawData rawData) =>
            Locker.WriteLock(() => { _storage = new DataStorage(rawData, _mapper, Schema, this); });

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

        public T ReadLock<T>(Func<T> func) => Locker.ReadLock(func);

        public T WriteLock<T>(Func<T> func) => Locker.WriteLock(func);

        public void WriteLock(Action action) => Locker.WriteLock(action);

        public IReadOnlyList<INode> GetEntitiesByTypeName(string typeName)
        {
            return Locker.ReadLock(() => 
                _storage.Nodes.Values
                .Where(n => n.NodeType.Kind == Kind.Entity
                    && (typeName == null || n.NodeType.Name == typeName))
                .ToList());
        }

        public INode CreateNode(Guid nodeTypeId, Guid? id = null) =>
            Locker.WriteLock(() => _storage.CreateNode(nodeTypeId, id));

        public IRelation CreateRelation(Guid sourceNodeId, Guid targetNodeId, Guid nodeTypeId, Guid? id = null) =>
            Locker.WriteLock(() => _storage.CreateRelation(sourceNodeId, targetNodeId, nodeTypeId, id));

        public IAttribute CreateAttribute(Guid id, string value) =>
            Locker.WriteLock(() => _storage.CreateAttribute(id, value));

        public IRelation UpdateRelationTarget(Guid id, Guid targetId) =>
            Locker.WriteLock(() => _storage.UpdateRelationTarget(id, targetId));

        public IRelation CreateRelationWithAttribute(Guid sourceNodeId, Guid nodeTypeId, string value) =>
            Locker.WriteLock(() => _storage.CreateRelationWithAttribute(sourceNodeId, nodeTypeId, value));

        public IRelation CreateRelationWithAttribute(Guid sourceNodeId, string relationTypeName, string value) =>
            Locker.WriteLock(() => _storage.CreateRelationWithAttribute(sourceNodeId, relationTypeName, value));

        internal NodeData GetNodeData(Guid id)
        {
            return Locker.ReadLock(() => _storage.Nodes.GetValueOrDefault(id));
        }

        internal IReadOnlyList<NodeData> GetNodesData(IEnumerable<Guid> ids)
        {
            return Locker.ReadLock(() => ids.Where(id => _storage.Nodes.ContainsKey(id)).Select(id => _storage.Nodes[id]).ToList());
        }
        public void AddValueByDotName(Guid entityId, string value, string dotName) =>
            AddValueByDotName(entityId, value, dotName.Split('.'));

        public void AddValueByDotName(Guid entityId, string value, string[] dotNameParts)
        {
            Locker.WriteLock(() =>
            {
                var entity = GetNodeData(entityId);
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
                        _storage.CreateRelation(node.Id, Guid.Parse(value), relationType.Id);
                        break;
                    }

                    var targetNode = _storage.CreateNode(relationType.TargetTypeId);
                    _storage.CreateRelation(node.Id, targetNode.Id, relationType.Id);

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

        public void SetNodeUpdatedAt(Guid nodeId, DateTime updatedAt)
        {
            Locker.WriteLock(() => _storage.SetNodeUpdatedAt(nodeId, updatedAt));
        }

        public void RemoveNodeAndRelations(Guid id) =>
            Locker.WriteLock(() => _storage.RemoveNodeAndRelations(id));

        public void RemoveNode(Guid id) =>
            Locker.WriteLock(() => _storage.RemoveNode(id));

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

        public IReadOnlyList<IRelation> GetIncomingRelations(IEnumerable<Guid> entityIdList, IEnumerable<string> relationTypeNameList)
        {
            return GetNodes(entityIdList).SelectMany(n => n.GetIncomingRelations(relationTypeNameList)).ToList();
        }

        public IAccessLevels GetAccessLevels()
        {
            var accessLevelNodeType = Schema.GetEntityTypeByName(EntityTypeNames.AccessLevel.ToString());
            if (accessLevelNodeType == null) 
                throw new Exception("Тип сутності AccessLevel не існує");
            var nodes = GetNodesByTypeId(accessLevelNodeType.Id);
            var list = new List<AccessLevel>();
            foreach (var node in nodes)
            {
                var nameProperty = node.GetSingleProperty(Schema.GetDotName("name"));
                if (nameProperty == null)
                    throw new Exception($"Атрибут name не існує для сутності {node.Id}");
                var numericIndexProperty = node.GetSingleProperty(Schema.GetDotName("numericIndex"));
                if (numericIndexProperty == null)
                    throw new Exception($"Атрибут numericIndex не існує для сутності {node.Id}");

                list.Add(new AccessLevel(node.Id, nameProperty.Value, int.Parse(numericIndexProperty.Value)));
            }

            return new AccessLevels(list);
        }
        public void SaveAccessLevels(IAccessLevels newAccessLevels)
        {
            const string NAME = "name";
            const string NUMERIC_INDEX = "numericIndex";

            var oldAccessLevels = GetAccessLevels();
            var accessLevelType = Schema.GetEntityTypeByName(EntityTypeNames.AccessLevel.ToString());
            Locker.WriteLock(() => 
            {
                foreach (var newItem in newAccessLevels.Items)
                {
                    var oldItem = oldAccessLevels.GetItemById(newItem.Id);
                    if (oldItem == null)
                    {
                        var node = CreateNode(accessLevelType.Id, newItem.Id);
                        CreateRelationWithAttribute(node.Id, NAME, newItem.Name);
                        CreateRelationWithAttribute(node.Id, NUMERIC_INDEX, newItem.NumericIndex.ToString());
                    }
                    else
                    {
                        var node = GetNode(oldItem.Id);
                        var nameNode = node.GetSingleDirectProperty(NAME);
                        if (nameNode.Value != newItem.Name)
                        {
                            RemoveNodeAndRelations(nameNode.Id);
                            CreateRelationWithAttribute(node.Id, NAME, newItem.Name);
                        }
                        
                        var numericIndexNode = node.GetSingleDirectProperty(NUMERIC_INDEX);
                        if (numericIndexNode.Value != newItem.NumericIndex.ToString())
                        {
                            RemoveNodeAndRelations(numericIndexNode.Id);
                            CreateRelationWithAttribute(node.Id, NUMERIC_INDEX, newItem.NumericIndex.ToString());
                        }
                    }
                }

                foreach (var oldItem in oldAccessLevels.Items)
                {
                    if (!newAccessLevels.Items.Any(x => x.Id == oldItem.Id))
                    {
                        RemoveNodeAndRelations(oldItem.Id);
                    }
                }
            });
        }
    }
}