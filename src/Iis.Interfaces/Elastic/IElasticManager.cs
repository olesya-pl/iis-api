using Iis.Interfaces.Ontology;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticManager
    {
        Task<bool> PutExtNodeAsync(IExtNode extNode, CancellationToken cancellationToken = default);
        Task<List<string>> Search(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<bool> DeleteAllIndexes(CancellationToken cancellationToken = default);
        List<string> SupportedIndexes { get; }
        bool IndexIsSupported(string indexName);
        bool IndexesAreSupported(IEnumerable<string> indexNames);
        Task<List<string>> GetIndexIdsAsync(string indexName);
        Task<string> GetByIdAsync(string indexName, string id, string[] fields);
        Task<bool> DeleteAsync(string indexName, string id);
        Task CreateSupportedIndexesAsync(CancellationToken token);
    }
}
