using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IOntology
    {
        Task<IEnumerable<Entity>> GetEntitiesAsync(string typeName, CancellationToken cancellationToken);
    }
}
