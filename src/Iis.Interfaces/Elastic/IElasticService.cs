using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Ontology.Data;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        Task<bool> PutNodeAsync(Guid id, CancellationToken ct = default);
        Task<bool> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken ct = default);
        Task<(List<Guid> ids, int count)> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default);
        Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default);
        Task<SearchEntitiesByConfiguredFieldsResult> SearchEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default);
        Task<SearchResult> SearchSignsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default);
        bool TypesAreSupported(IEnumerable<string> typeNames);
        Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken ct);
        Task<IEnumerable<IElasticSearchResultItem>> SearchByFieldsAsync(string query, string[] fieldNames, int size, CancellationToken ct = default);
        Task<int> CountByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default);
        Task<int> CountEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken ct = default);
    }
}
