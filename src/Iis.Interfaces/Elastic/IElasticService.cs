using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        IEnumerable<string> MaterialIndexes { get; }
        IEnumerable<string> OntologyIndexes { get; }
        IEnumerable<string> EventIndexes { get; }
        IEnumerable<string> FeatureIndexes { get; }
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutFeatureAsync(Guid featureId, JObject featureDocument, CancellationToken cancellation = default);
        Task<(List<Guid> ids, int count)> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchResult> SearchMoreLikeThisAsync(Guid materialId, CancellationToken cancellationToken = default);
        bool TypesAreSupported(IEnumerable<string> typeNames);
        bool UseElastic { get; }
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
