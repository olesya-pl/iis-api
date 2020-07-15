using System.Threading;
using System.Threading.Tasks;

namespace Iis.Interfaces.OntologyEnum
{
    public interface INodeEnumService
    {
        Task<INodeEnumValues> GetEnumValues(string typeName, CancellationToken cancellationToken = default);
    }
}