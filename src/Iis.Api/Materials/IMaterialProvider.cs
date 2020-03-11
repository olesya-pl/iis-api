using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel.Materials;
using Iis.Domain.Materials;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id);
        Task<MaterialEntity> GetMaterialEntityAsync(Guid id);
        Task<IEnumerable<Material>> GetMaterialsAsync(int limit, int offset, Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null);
        IReadOnlyCollection<MaterialSignEntity> MaterialSigns { get; }
        MaterialSignEntity GetMaterialSign(Guid id);
        Task<Material> MapAsync(MaterialEntity material);
    }
}
