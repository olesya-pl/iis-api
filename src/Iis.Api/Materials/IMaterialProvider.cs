using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iis.DataModel.Materials;
using Iis.Domain.Materials;
using Iis.Interfaces.Materials;

namespace IIS.Core.Materials
{
    public interface IMaterialProvider
    {
        Task<Material> GetMaterialAsync(Guid id);
        Task<MaterialEntity> GetMaterialEntityAsync(Guid id);
        Task<IEnumerable<Material>> GetMaterialsAsync(int limit, int offset, Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null, IEnumerable<string> types = null);
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(Guid id);
        Task<Material> MapAsync(MaterialEntity material);
        Task<MaterialEntity> UpdateMaterial(IMaterialUpdateInput input);
    }
}
