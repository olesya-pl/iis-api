using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IOntologyProvider
    {
        Task<IEnumerable<Type>> GetTypesAsync(CancellationToken cancellationToken = default);
    }
}
