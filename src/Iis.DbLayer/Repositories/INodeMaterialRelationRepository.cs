using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.DbLayer.Repositories
{
    public interface INodeMaterialRelationRepository
    {
        Task<List<Guid>> GetExistingRelationMaterialIds(Guid nodeId, IReadOnlyCollection<Guid> candidates);
        void CreateRelations(Guid nodeId, IReadOnlyCollection<Guid> newMaterials);
    }
}