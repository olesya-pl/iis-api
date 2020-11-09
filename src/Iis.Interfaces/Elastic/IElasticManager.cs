using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticManager
    {
        Task<bool> PutDocumentAsync(string indexName, string id, string jsonDocument, CancellationToken cancellationToken = default);
        Task<bool> PutDocumentAsync(string indexName, string documentId, string jsonDocument, bool waitForIndexing, CancellationToken cancellationToken = default);
        Task<bool> DeleteDocumentAsync(string indexName, string documentId);
        Task<IElasticSearchResult> SearchAsync(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> SearchAsync(IMultiElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> SearchAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> GetDocumentIdListFromIndexAsync(string indexName);
        Task<IElasticSearchResult> GetDocumentByIdAsync(IReadOnlyCollection<string> indexNames, string id, CancellationToken token = default);
        Task CreateIndexesAsync(IEnumerable<string> indexNames, JObject mappingConfiguration = null, CancellationToken token = default);
        Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);
        Task<bool> DeleteIndexesAsync(IEnumerable<string> indexNames, CancellationToken cancellationToken = default);
        Task<bool> CreateMapping(IAttributeInfoList attributesList, CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutDocumentsAsync(string indexName, string documents, CancellationToken ct = default);
        Task<IElasticSearchResult> SearchByImageVector(decimal[] imageVector, IIisElasticSearchParams searchParams, CancellationToken token);
        Task<int> CountAsync(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<int> CountAsync(IMultiElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<int> CountAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default);
        Task<ElasticResponse> AddMappingPropertyToIndexAsync(string indexName, JObject mappingConfiguration, CancellationToken ct = default);
    }
}
