using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Elastic;

namespace Iis.Domain
{
    public interface IOntologyService
    {
        Task<int> GetNodesCountAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default);
        IReadOnlyCollection<IncomingRelation> GetIncomingEntities(Guid entityId);
        IEnumerable<Node> GetEventsAssociatedWithEntity(Guid entityId);
        Dictionary<Guid, int> CountEventsAssociatedWithEntities(HashSet<Guid> entityIds);
        Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeAsync(IEnumerable<string> typeNameList, ElasticFilter filter, CancellationToken cancellationToken = default);
        (IEnumerable<Node> nodes, int count) GetNodesByIds(IEnumerable<Guid> matchList, CancellationToken cancellationToken = default);
        void SaveNode(Node node);
        void RemoveNode(Node node);
        Node LoadNodes(Guid nodeId);
        IReadOnlyCollection<Node> LoadNodes(IEnumerable<Guid> nodeIds, IEnumerable<IEmbeddingRelationTypeModel> relationTypes);
        IReadOnlyCollection<Entity> GetEntitiesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
        Node GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
        IReadOnlyList<IAttributeBase> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit);
        IReadOnlyCollection<Guid> GetNodeIdListByFeatureIdList(IEnumerable<Guid> featureIdList);
        string GetAttributeValueByDotName(Guid id, string dotName);
        Dictionary<Guid, int> GetRelationsCount(HashSet<Guid> entityIds);
        Task<SearchEntitiesByConfiguredFieldsResult> SearchEventsAsync(ElasticFilter filter);
    }
}
