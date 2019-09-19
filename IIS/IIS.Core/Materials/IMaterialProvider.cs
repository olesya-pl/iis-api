using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Materials.Material> GetMaterialAsync(Guid id);
        Task<IEnumerable<Materials.Material>> GetMaterialsAsync(int limit, int offset, Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null);
    }
}
