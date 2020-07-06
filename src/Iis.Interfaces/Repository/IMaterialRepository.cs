using System;
using System.Threading.Tasks;

namespace Iis.Interfaces.Repository
{
    public interface IMaterialRepository
    {
        Task<int> GetCountOfParentMaterials(Guid[] materialIds);
    }
}
