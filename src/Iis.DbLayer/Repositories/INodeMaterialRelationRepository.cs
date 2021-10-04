using Iis.DataModel.Materials;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.DbLayer.Repositories
{
    public interface INodeMaterialRelationRepository
    {
        Task<List<Guid>> GetExistingRelationMaterialIdsAsync(
            Guid nodeId, 
            IReadOnlyCollection<Guid> candidates, 
            MaterialNodeLinkType? linkType);
        void CreateRelations(Guid nodeId, IReadOnlyCollection<Guid> newMaterials, MaterialNodeLinkType linkType);
    }
}