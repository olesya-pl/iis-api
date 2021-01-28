using System;
using System.Threading.Tasks;
using Iis.Domain.Materials;

namespace Iis.MaterialLoader
{
    public interface IMaterialService
    {
        Task<Material> SaveAsync(Material material, Guid? changeRequestId = null);

        Task<Material> SaveAsync(MaterialInput materialInput);
    }
}
