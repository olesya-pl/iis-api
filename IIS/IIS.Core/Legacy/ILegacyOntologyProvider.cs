using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;

namespace IIS.Legacy.EntityFramework
{
    public interface ILegacyOntologyProvider
    {
        Task<Ontology> GetOntologyAsync(CancellationToken cancellationToken = default);
    }
}
