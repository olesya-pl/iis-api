using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        Task<bool> PutNodeAsync(Guid id, CancellationToken ct = default);
        Task<bool> PutNodeAsync(Guid id, IEnumerable<string> fieldsToExtract, CancellationToken ct = default);
        Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, Guid userId, CancellationToken ct = default);
        Task<SearchResult> AutocompleteByFieldsAsync(IEnumerable<string> enumerable, ElasticFilter filter, Guid id, CancellationToken cancellationToken);
        Task<int> CountAutocompleteByFieldsAsync(IEnumerable<string> enumerable, ElasticFilter filter, Guid userId, CancellationToken cancellationToken);
        Task<SearchEntitiesByConfiguredFieldsResult> SearchEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, Guid userId, CancellationToken ct = default);
        Task<SearchEntitiesByConfiguredFieldsResult> FilterNodeCoordinatesAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        Task<SearchResult> SearchSignsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        bool TypesAreSupported(IEnumerable<string> typeNames);
        Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken ct);
        Task<IEnumerable<IElasticSearchResultItem>> SearchByFieldsAsync(string query, IReadOnlyCollection<string> fieldNames, IReadOnlyCollection<string> typeNames, int size, Guid userId, CancellationToken ct = default);
        Task<int> CountByAllFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        Task<int> CountEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, ElasticFilter filter, CancellationToken ct = default);
        Task<bool> DeleteNodeAsync(Guid id, string typeName, CancellationToken ct = default);
        bool TypeIsAvalilable(INodeTypeLinked type, bool entitySearchGranted, bool wikiSearchGranted);
        bool ShouldReturnAllEntities(ElasticFilter filter);
        bool ShouldReturnNoEntities(ElasticFilter filter);
    }
}