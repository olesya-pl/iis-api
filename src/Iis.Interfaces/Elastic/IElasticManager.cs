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
        Task<bool> DeleteDocumentAsync(string indexName, string documentId);
        Task<IElasticSearchResult> Search(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<IElasticSearchResult> GetDocumentIdListFromIndexAsync(string indexName);
        Task<IElasticSearchResult> GetDocumentByIdAsync(IReadOnlyCollection<string> indexNames, string id, CancellationToken token = default);
        Task CreateIndexesAsync(IEnumerable<string> indexNames, JObject mappingConfiguration = null, CancellationToken token = default);
        Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);
        Task<bool> DeleteIndexesAsync(IEnumerable<string> indexNames, CancellationToken cancellationToken = default);
        Task<bool> CreateMapping(IAttributeInfoList attributesList, CancellationToken cancellationToken = default);
        Task<bool> PutsDocumentsAsync(string indexName, string materialDocuments, CancellationToken token);
        Task<IElasticSearchResult> SearchByImageVector(decimal[] imageVector, IIisElasticSearchParams searchParams, CancellationToken token);
    }
}
