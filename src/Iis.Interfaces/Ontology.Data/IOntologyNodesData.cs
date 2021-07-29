using Iis.Interfaces.AccessLevels;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IOntologyNodesData
    {
        IEnumerable<INode> Nodes { get; }
        IEnumerable<IRelation> Relations { get; }
        IEnumerable<IAttribute> Attributes { get; }
        IOntologySchema Schema { get; }
        IOntologyPatch Patch { get; }
        void ClearPatch();
        void ReloadData(INodesRawData rawData);
        INode GetNode(Guid id);
        T ReadLock<T>(Func<T> func);
        T WriteLock<T>(Func<T> func);
        void WriteLock(Action action);
        IReadOnlyList<INode> GetAllNodes();
        IReadOnlyList<INode> GetNodes(IEnumerable<Guid> ids);
        IReadOnlyList<INode> GetNodesByTypeIds(IEnumerable<Guid> nodeTypeIds);
        IReadOnlyList<INode> GetNodesByTypeId(Guid nodeTypeId);
        IReadOnlyList<INode> GetEntitiesByTypeName(string typeName);
        IReadOnlyList<INode> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
        void RemoveNodeAndRelations(Guid id);
        void RemoveNode(Guid id);
        void RemoveNodes(IEnumerable<Guid> ids);
        INode CreateNode(Guid nodeTypeId, Guid? id = null);
        IRelation CreateRelation(Guid sourceNodeId, Guid targetNodeId, Guid nodeTypeId, Guid? id = null);
        IAttribute CreateAttribute(Guid id, string value);
        IRelation UpdateRelationTarget(Guid id, Guid targetId);
        IRelation CreateRelationWithAttribute(Guid sourceNodeId, Guid nodeTypeId, string value);
        void SetNodeIsArchived(Guid nodeId);
        void SetNodeUpdatedAt(Guid nodeId, DateTime updatedAt);
        IReadOnlyList<IRelation> GetIncomingRelations(IEnumerable<Guid> entityIdList, IEnumerable<string> relationTypeNameList);
        void AddValueByDotName(Guid entityId, string value, string dotName);
        void AddValueByDotName(Guid entityId, string value, string[] dotNameParts);
        IAccessLevels GetAccessLevels();
        void SaveAccessLevels(IAccessLevels newAccessLevels);
        event Action OnAccessLevelsChanged;
        void ChangeNodeTypeId(Guid idFrom, Guid idTo);
    }
}