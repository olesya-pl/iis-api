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
        Task<(IEnumerable<Material> Materials, int Count)> GetMaterialsAsync(int limit, 
            int offset,
            string filterQuery,
            Guid? parentId = null,
            IEnumerable<Guid> nodeIds = null,
            IEnumerable<string> types = null);
        Task<IEnumerable<MaterialEntity>> GetMaterialEntitiesAsync();
        IReadOnlyCollection<MaterialSignEntity> GetMaterialSigns(string typeName);
        MaterialSign GetMaterialSign(Guid id);
        Task<MaterialEntity> UpdateMaterial(IMaterialUpdateInput input);
        Task<Material> MapAsync(MaterialEntity material);
    }
}
