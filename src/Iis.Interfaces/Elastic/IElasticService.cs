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
        Task<SearchByConfiguredFieldsResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchByConfiguredFieldsResult> SearchMaterialsByConfiguredFieldsAsync(IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchByConfiguredFieldsResult> SearchByImageVector(decimal[] imageVector, int page, int pageSize, CancellationToken token);
        bool TypesAreSupported(IEnumerable<string> typeNames);
        bool UseElastic { get; }
    }

    public class SearchByConfiguredFieldsResult
    {
        public Dictionary<Guid, SearchByConfiguredFieldsResultItem> Items { get; set; }
        public int Count { get; set; }
    }

    public class SearchByConfiguredFieldsResultItem
    {
        public JToken Highlight { get; set; }
        public JObject SearchResult { get; set; }
    }
}
