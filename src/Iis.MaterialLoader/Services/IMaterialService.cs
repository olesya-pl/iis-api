using System;
using System.Threading.Tasks;
using Iis.Domain.Materials;
using Iis.MaterialLoader.Models;

namespace Iis.MaterialLoader.Services
{
    public interface IMaterialService
    {
        Task<Material> SaveAsync(MaterialInput materialInput);
    }
}
