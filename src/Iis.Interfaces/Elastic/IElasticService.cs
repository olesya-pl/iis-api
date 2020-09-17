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
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutHistoricalNodesAsync(Guid id, Guid? requestId = null, CancellationToken cancellationToken = default);
        Task<bool> PutFeatureAsync(Guid featureId, JObject featureDocument, CancellationToken cancellation = default);
        Task<(List<Guid> ids, int count)> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<(int Count, List<JObject> Entities)> SearchEntitiesByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchResult> SearchMoreLikeThisAsync(IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchResult> SearchByImageVector(decimal[] imageVector, int page, int pageSize, CancellationToken token);
        bool TypesAreSupported(IEnumerable<string> typeNames);
        bool UseElastic { get; }

        Task<bool> PutNodesAsync(IReadOnlyCollection<INode> itemsToUpdate, CancellationToken cancellationToken);
        Task<IEnumerable<IElasticSearchResultItem>> SearchByFieldAsync(string query, string fieldName, int size, CancellationToken ct = default);
    }

    public class SearchResult
    {
        public Dictionary<Guid, SearchResultItem> Items { get; set; }
        public int Count { get; set; }
    }

    public class SearchResultItem
    {
        public JToken Highlight { get; set; }
        public JObject SearchResult { get; set; }
    }
}
