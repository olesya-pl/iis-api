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
        public async Task<List<Guid>> GetExistingRelationMaterialIdsAsync(
            Guid nodeId, 
            IReadOnlyCollection<Guid> candidates,
            MaterialNodeLinkType? linkType) 
        {
            return await Context.MaterialFeatures
                            .Include(p => p.MaterialInfo)
                            .Where(p => p.NodeId == nodeId
                                        && candidates.Contains(p.MaterialInfo.MaterialId)
                                        && (linkType == null || linkType == p.NodeLinkType))
                            .Select(p => p.MaterialInfo.MaterialId)
                            .ToListAsync();
        }

        public void CreateRelations(Guid nodeId, IReadOnlyCollection<Guid> newMaterials, MaterialNodeLinkType linkType)
        {
            if (linkType == MaterialNodeLinkType.Caller || linkType == MaterialNodeLinkType.Receiver)
            {
                var existing = Context.MaterialFeatures
                .Include(mf => mf.MaterialInfo)
                .Where(mf => newMaterials.Contains(mf.MaterialInfo.MaterialId)
                  && mf.NodeLinkType == linkType);

                Context.MaterialInfos.RemoveRange(existing.Select(mf => mf.MaterialInfo));
                Context.MaterialFeatures.RemoveRange(existing);
            }

            Context.MaterialFeatures.AddRange(newMaterials.Select(p => MaterialFeatureEntity.CreateFrom(p, nodeId, linkType)));
        }
    }
}
