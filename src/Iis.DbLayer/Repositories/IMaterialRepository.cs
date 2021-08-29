using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;
using System.Threading;
using Iis.Domain.Materials;

namespace Iis.DbLayer.Repositories
{
    public interface IMaterialRepository
    {
        Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes);

        Task<MaterialEntity[]> GetByIdsAsync(ISet<Guid> ids, params MaterialIncludeEnum[] includes);

        Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes);
        
        Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, params MaterialIncludeEnum[] includes);

        Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, int offset, params MaterialIncludeEnum[] includes);

        Task<IEnumerable<MaterialEntity>> GetAllForRelatedNodeListAsync(IEnumerable<Guid> nodeIdList);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<Guid> materialIdList, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<string> types, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<IReadOnlyCollection<Guid>> GetAllUnassignedIdsAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId);

        void AddMaterialEntity(MaterialEntity materialEntity);

        void AddMaterialInfos(IEnumerable<MaterialInfoEntity> materialEntities);

        void AddMaterialFeatures(IEnumerable<MaterialFeatureEntity> materialFeatureEntities);

        void EditMaterial(MaterialEntity materialEntity);

        List<MaterialEntity> GetMaterialByNodeIdQuery(IList<Guid> nodeIds, params MaterialIncludeEnum[] includes);

        Task<List<Guid>> GetNodeIsWithMaterials(IList<Guid> nodeIds);

        Task<List<MaterialEntity>> GetMaterialByNodeIdQueryAsync(IEnumerable<Guid> nodeIds);

        Task<List<MaterialsCountByType>> GetParentMaterialByNodeIdQueryAsync(IList<Guid> nodeIds);

        void AddFeatureIdList(Guid materialId, IEnumerable<Guid> featureIdList);

        Task<IEnumerable<Guid>> GetChildIdListForMaterialAsync(Guid materialId);

        Task<bool> CheckMaterialExistsAndHasContent(Guid materialId);

        Task RemoveMaterialsAndRelatedData(IReadOnlyCollection<Guid> fileIdList);

        Task<Guid?> GetParentIdByChildIdAsync(Guid materialId);

        Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
    }
}