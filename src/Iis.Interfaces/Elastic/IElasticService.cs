using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

using Iis.Interfaces.Ontology;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        IEnumerable<string> MaterialIndexes { get; }
        IEnumerable<string> OntologyIndexes { get; }
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(IExtNode extNode, CancellationToken cancellationToken = default);
        Task<bool> PutMaterialAsync(Guid materialId, JObject materialDocument, CancellationToken cancellation = default);
        Task<SearchByAllFieldsResult> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        bool TypesAreSupported(IEnumerable<string> typeNames);
        bool UseElastic { get; }
    }

    public class SearchByAllFieldsResult
    {
        public Dictionary<Guid, SearchByAllFieldsResultItem> Items { get; set; }
        public int Count { get; set; }
    }

    public class SearchByAllFieldsResultItem
    {
        public JToken Highlight { get; set; }
        public JObject SearchResult { get; set; }
    }
}
