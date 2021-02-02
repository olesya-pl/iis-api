using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iis.DataModel;
using Iis.DataModel.Materials;
using IIS.Repository;
using Microsoft.EntityFrameworkCore;

namespace Iis.DbLayer.Repositories
{
    internal class NodeMaterialRelationRepository : RepositoryBase<OntologyContext>, INodeMaterialRelationRepository
    {
        public async Task<List<Guid>> GetExistingRelationMaterialIds(Guid nodeId, IReadOnlyCollection<Guid> candidates) 
        {
            return await Context.MaterialFeatures
                            .Include(p => p.MaterialInfo)
                            .Where(p => p.NodeId == nodeId
                                        && candidates.Contains(p.MaterialInfo.MaterialId))
                            .Select(p => p.MaterialInfo.MaterialId)
                            .ToListAsync();
        }

        public void CreateRelations(Guid nodeId, IReadOnlyCollection<Guid> newMaterials)
        {
            Context.MaterialFeatures.AddRange(newMaterials.Select(p => new MaterialFeatureEntity
            {
                NodeId = nodeId,
                MaterialInfo = new MaterialInfoEntity
                {
                    MaterialId = p
                }
            }));
        }
    }
}
