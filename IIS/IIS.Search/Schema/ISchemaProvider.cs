using System.Threading.Tasks;

namespace IIS.Search.Schema
{
    public interface ISchemaProvider
    {
        Task<ComplexType> GetSchemaAsync();
    }
}
