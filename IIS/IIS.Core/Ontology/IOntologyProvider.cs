using System.Threading;
using System.Threading.Tasks;

namespace IIS.Core.Ontology
{
    public interface IOntologyProvider
    {
        Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default);
    }
}
