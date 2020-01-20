using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.Domain.Materials;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id);
        Task<IEnumerable<Material>> GetMaterialsAsync(int limit, int offset, Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null);
    }
}
