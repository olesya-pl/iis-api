﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Domain;

namespace Iis.DbLayer.Ontology.EntityFramework
{
    public interface IOntologyRepository
    {
        NodeEntity GetNodeEntityById(Guid id);
        AttributeEntity GetAttributeEntityById(Guid id);
        Task<List<NodeEntity>> GetNodeEntitiesByIdsAsync(IEnumerable<Guid> ids);
        Task<List<RelationEntity>> GetSourceRelationByIdAsync(Guid id, CancellationToken cancellationToken);
        void AddNode(NodeEntity nodeEntity);
        void AddNodes(IEnumerable<NodeEntity> nodeEntities);
        void UpdateNodes(IEnumerable<NodeEntity> nodeEntities);
        Task<List<RelationEntity>> GetDirectRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds);
        Task<List<RelationEntity>> GetInversedRelationsQuery(IEnumerable<Guid> nodeIds, IEnumerable<Guid> relationIds);
        Task<Dictionary<Guid, NodeEntity>> GetExistingNodes(CancellationToken cancellationToken);
        Task<int> GetNodesCountAsync(IEnumerable<Guid> derived);
        Task<int> GetNodesCountWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion);
        Task<List<RelationEntity>> GetAllRelationsAsync(Guid nodeId);
        Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList);
        Task<List<NodeEntity>> GetNodesAsync(IEnumerable<Guid> derived, ElasticFilter filter);
        Task<List<NodeEntity>> GetNodesWithSuggestionAsync(IEnumerable<Guid> derived, string suggestion, ElasticFilter filter);

        Task<IEnumerable<AttributeEntity>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName,
            int limit);

        Task<List<NodeEntity>> GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
    }
}