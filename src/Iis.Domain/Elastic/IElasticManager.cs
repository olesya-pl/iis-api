using Iis.Domain.ExtendedData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Domain.Elastic
{
    public interface IElasticManager
    {
        Task<bool> PutExtNodeAsync(ExtNode extNode, CancellationToken cancellationToken = default);
        Task<List<string>> Search(IisElasticSearchParams searchParams, CancellationToken cancellationToken = default);
        void SetSupportedIndexes(IEnumerable<string> indexNames);
        bool IndexIsSupported(string indexName);
        bool IndexesAreSupported(IEnumerable<string> indexNames);
    }
}
