using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Iis.Interfaces.Ontology;
using Iis.Interfaces.Materials;
namespace Iis.Interfaces.Elastic
{
    public interface IElasticService
    {
        IEnumerable<string> MaterialIndexes { get; }
        IEnumerable<string> OntologyIndexes { get; }
        Task<bool> PutNodeAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> PutNodeAsync(IExtNode extNode, CancellationToken cancellationToken = default);
        Task<bool> PutMaterialAsync(IMaterialEntity material, CancellationToken cancellation = default);
        Task<(List<Guid> ids, int count)> SearchByAllFieldsAsync(IEnumerable<string> typeNames, IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        bool TypesAreSupported(IEnumerable<string> typeNames);

    }
}
