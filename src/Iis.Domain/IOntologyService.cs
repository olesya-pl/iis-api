using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.Domain
{
    public interface IOntologyService
    {
        Task<int> GetNodesCountAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> GetNodesAsync(IEnumerable<INodeTypeModel> types, ElasticFilter filter, CancellationToken cancellationToken = default);
        Task<List<IncomingRelation>> GetIncomingEntities(Guid entityId);
        Task<IEnumerable<Node>> GetEventsAssociatedWithEntity(Guid entityId);
        Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeAsync(IEnumerable<string> typeNameList, ElasticFilter filter, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Node> nodes, int count)> GetNodesAsync(IEnumerable<Guid> matchList, CancellationToken cancellationToken = default);
        Task SaveNodeAsync(Node node, CancellationToken cancellationToken = default);
        Task SaveNodeAsync(Node source, Guid? requestId, CancellationToken cancellationToken = default);
        Task RemoveNodeAsync(Node node, CancellationToken cancellationToken = default);
        Task<Node> LoadNodesAsync(Guid nodeId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Node>> LoadNodesAsync(IEnumerable<Guid> nodeIds, IEnumerable<IEmbeddingRelationTypeModel> relationTypes, CancellationToken cancellationToken = default);
        Task<List<Entity>> GetEntitiesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
        Task<Node> GetNodeByUniqueValue(Guid nodeTypeId, string value, string valueTypeName);
        Task<IReadOnlyList<IAttributeBase>> GetNodesByUniqueValue(Guid nodeTypeId, string value, string valueTypeName, int limit);
        Task<List<Guid>> GetNodeIdListByFeatureIdListAsync(IEnumerable<Guid> featureIdList);
        string GetAttributeValueByDotName(Guid id, string dotName);
        int GetRelationsCount(Guid entityId);
    }
}
