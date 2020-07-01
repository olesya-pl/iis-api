using System.Threading;
using System.Threading.Tasks;
using Iis.Domain;

namespace IIS.Domain
{
    public interface IOntologyProvider
    {
        IOntologyModel GetOntology();
        void Invalidate();
    }
}
