using System.Threading.Tasks;

namespace IIS.Search.Schema
{
    public interface ISchemaRepository : ISchemaProvider
    {
        Task SaveSchemaAsync(ComplexType type);
    }
}
