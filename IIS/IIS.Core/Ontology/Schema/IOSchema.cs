using System.Threading.Tasks;

namespace IIS.Core
{
    public interface IOSchema
    {
        Task<TypeEntity> GetRootAsync();
    }
}
