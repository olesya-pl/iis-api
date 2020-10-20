using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;
using Iis.Interfaces.Ontology.Data;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public interface IOntologyRepository
    {
        NodeEntity GetNodeEntityById(Guid id);
        NodeEntity GetActiveNodeEntityById(Guid id);
        Task<NodeEntity> GetNodeEntityByIdAsync(Guid id);
        Task<NodeEntity> GetNodeEntityWithIncludesByIdAsync(Guid id);
        AttributeEntity GetAttributeEntityById(Guid id);
        Task<List<NodeEntity>> GetNodeEntitiesByIdsAsync(IEnumerable<Guid> ids);
        Task<List<RelationEntity>> GetSourceRelationByIdAsync(Guid id, CancellationToken cancellationToken);
        void AddNode(NodeEntity nodeEntity);
        void AddNodes(IEnumerable<NodeEntity> nodeEntities);
        void UpdateNodes(IEnumerable<NodeEntity> nodeEntities);
        Task<List<RelationEntity>> GetDirectRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds);
        Task<List<RelationEntity>> GetInversedRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds);
        Task<int> GetNodesCountWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion);
        Task<List<RelationEntity>> GetAllRelationsAsync(Guid nodeId);
        List<NodeEntity> GetAllNodes();
        Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList);
        List<RelationEntity> GetAllRelations();
        Task<List<NodeEntity>> GetNodesWithSuggestionAsync(IEnumerable<Guid> derived, ElasticFilter filter);
        List<AttributeEntity> GetAllAttributes();
        Task<List<AttributeEntity>> GetAttributesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit);
        Task<List<NodeEntity>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
        Task<NodeEntity> UpdateNodeAsync(Guid id, Action<NodeEntity> action);
        Task<List<Guid>> GetSourceNodeIdByTargetNodeId(Guid? propertyId, Guid entityId);
        Task<List<RelationEntity>> GetIncomingRelationsAsync(Guid entityId);
        Task<List<RelationEntity>> GetIncomingRelationsAsync(IReadOnlyCollection<Guid> entityIds);
        Task<List<RelationEntity>> GetIncomingRelationsAsync(IEnumerable<Guid> entityIdList, IEnumerable<string> relationTypeNameList);
        string GetAttributeValueByDotName(Guid id, string dotName);
    }
}