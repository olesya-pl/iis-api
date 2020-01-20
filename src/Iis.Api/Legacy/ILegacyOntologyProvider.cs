using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Legacy.EntityFramework
{
    public interface ILegacyOntologyProvider
    {
        Task<OntologyModel> GetOntologyAsync(CancellationToken cancellationToken = default);
    }
}
