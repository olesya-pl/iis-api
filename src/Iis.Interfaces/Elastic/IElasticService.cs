using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        Task<bool> PutNodeAsync(Guid id, CancellationToken ct = default);
        Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExtract, CancellationToken ct = default);
        Task<List<ElasticBulkResponse>> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default);
        Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, Guid userId, CancellationToken ct = default);
        Task<int> CountByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        Task<SearchEntitiesByConfiguredFieldsResult> SearchEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, Guid userId, CancellationToken ct = default);
        Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeCoordinatesAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        Task<SearchResult> SearchSignsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        bool TypesAreSupported(IEnumerable<string> typeNames);
        Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken ct);
        Task<IEnumerable<IElasticSearchResultItem>> SearchByFieldsAsync(string query, IReadOnlyCollection<string> fieldNames, IReadOnlyCollection<string> typeNames, int size, CancellationToken ct = default);
        Task<int> CountByAllFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        Task<int> CountEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        Task<bool> DeleteNodeAsync(Guid id, string typeName, CancellationToken ct = default);
        bool TypeIsAvalilable(INodeTypeLinked type, bool entitySearchGranted, bool wikiSearchGranted);
        bool ShouldReturnAllEntities(ElasticFilter filter);
    }
}
