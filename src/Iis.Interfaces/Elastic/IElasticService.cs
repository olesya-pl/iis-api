using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Guid>> SearchByAllFieldsAsync(IEnumerable<string> typeNames, string suggestion, CancellationToken cancellationToken = default);
        bool TypesAreSupported(IEnumerable<string> typeNames);
    }
}
