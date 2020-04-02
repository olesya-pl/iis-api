﻿using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Iis.Interfaces.Ontology;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticManager
    {
        Task<bool> PutDocumentAsync(string indexName, string id, string jsonDocument, CancellationToken cancellationToken = default);
        Task<bool> DeleteDocumentAsync(string indexName, string documentId);
        Task<IElasticSearchResult> Search(IIisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetDocumentIdListFromIndexAsync(string indexName);
        Task<string> GetDocumentByIdAsync(string indexName, string id, string[] fields);
        Task CreateIndexesAsync(IEnumerable<string> indexNames, CancellationToken token);
        Task<bool> DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);
        Task<bool> DeleteIndexesAsync(IEnumerable<string> indexNames, CancellationToken cancellationToken = default);
    }
}
