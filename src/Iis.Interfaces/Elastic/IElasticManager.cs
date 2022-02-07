using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Dtos;
using Newtonsoft.Json.Linq;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticManager
    {
        Task<bool> PutDocumentAsync(string indexName, string id, string jsonDocument, CancellationToken cancellationToken = default);
        Task<bool> PutDocumentAsync(string indexName, string documentId, string jsonDocument, bool waitForIndexing, CancellationToken cancellationToken = default);
        Task<bool> DeleteDocumentAsync(string indexName, string documentId, CancellationToken ct = default);
        Task<IElasticSearchResult> SearchAsync(IisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> BeginSearchByScrollAsync(string queryData, TimeSpan scrollLifetime, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> SearchAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> GetDocumentIdListFromIndexAsync(string indexName);
        Task<IElasticSearchResult> GetDocumentByIdAsync(IReadOnlyCollection<string> indexNames, string id, CancellationToken token = default);
        Task CreateIndexesAsync(IEnumerable<string> indexNames, JObject mappingConfiguration = null, CancellationToken token = default);
        Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);
        Task<bool> DeleteIndexesAsync(IEnumerable<string> indexNames, CancellationToken cancellationToken = default);
        Task<bool> CreateMapping(IAttributeInfoList attributesList, CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutDocumentsAsync(string indexName, string documents, bool waitForIndexing = false, CancellationToken ct = default);
        Task<int> CountAsync(IisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<int> CountAsync(string queryData, IEnumerable<string> baseIndexNameList, CancellationToken cancellationToken = default);
        Task<ElasticResponse> AddMappingPropertyToIndexAsync(string indexName, JObject mappingConfiguration, CancellationToken ct = default);
        Task<IElasticSearchResult> SearchByScrollAsync(string scrollId, TimeSpan scrollDuration, CancellationToken cancellationToken = default);
        Task<bool> CreateSecurityMappingAsync(
            List<(
                IReadOnlyCollection<string> indexNames,
                string accessLevelFieldName)> parameters,
            CancellationToken cancellationToken);
        Task<T> GetExactPayloadAsyncDictionaryAsync<T>(string path, CancellationToken cancellationToken);
        Task<bool> DeleteExactPayloadAsync(string path, CancellationToken cancellationToken);
        Task<bool> PutExactPayloadAsync(string path, string data, CancellationToken cancellationToken);
        Task<JObject> GetUsersAsync(CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> GetSecurityLevelsAsync(CancellationToken cancellationToken = default);
        IElasticManager WithUserId(Guid userId);
        IElasticManager WithDefaultUser();
    }
}
