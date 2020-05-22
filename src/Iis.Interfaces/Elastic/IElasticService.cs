using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Iis.Interfaces.Ontology;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        IEnumerable<string> MaterialIndexes { get; }
        IEnumerable<string> OntologyIndexes { get; }
        IEnumerable<string> EventIndexes { get; }
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(IExtNode extNode, CancellationToken cancellationToken = default);
        Task<bool> PutMaterialAsync(Guid materialId, JObject materialDocument, CancellationToken cancellation = default);
        Task<(List<Guid> ids, int count)> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        Task<SearchByConfiguredFieldsResult> SearchByConfiguredFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
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
