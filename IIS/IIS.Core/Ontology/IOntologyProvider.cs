using System.Threading;
using System.Threading.Tasks;
using Iis.Domain;

namespace IIS.Core.Ontology
{
    public interface IOntologyProvider
    {
        Task<OntologyModel> GetOntologyAsync(CancellationToken cancellationToken = default);
        void Invalidate();
    }
}
