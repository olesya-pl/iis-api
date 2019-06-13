using System.Threading.Tasks;

namespace IIS.Core.Schema
{
    public interface ISchemaProvider
    {
        Task<ComplexType> GetSchemaAsync();
    }
}
