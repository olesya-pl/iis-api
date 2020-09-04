using System.Threading.Tasks;

namespace Iis.Interfaces.Ontology.Data
{
    public interface IOntologyPatchSaver
    {
        Task SavePatchAsync(IOntologyPatch patch);
    }
}