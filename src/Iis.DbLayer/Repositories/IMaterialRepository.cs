using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;
using System.Threading;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;

namespace Iis.DbLayer.Repositories
{
    public interface IMaterialRepository
    {
        Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes);

        Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes);

        Task<IEnumerable<MaterialEntity>> GetAllForRelatedNodeListAsync(IEnumerable<Guid> nodeIdList);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<Guid> materialIdList, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<string> types, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId);
        Task<int> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken = default);
        Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, CancellationToken cancellationToken = default);
        Task<SearchByConfiguredFieldsResult> SearchMaterials(IElasticNodeFilter filter, CancellationToken cancellationToken = default);
        void AddMaterialEntity(MaterialEntity materialEntity);
        void AddMaterialInfos(IEnumerable<MaterialInfoEntity> materialEntities);
        void AddMaterialFeatures(IEnumerable<MaterialFeatureEntity> materialFeatureEntities);
        void EditMaterial(MaterialEntity materialEntity);
        List<MaterialEntity> GetMaterialByNodeIdQuery(IList<Guid> nodeIds);
        Task<List<MaterialsCountByType>> GetParentMaterialByNodeIdQueryAsync(IList<Guid> nodeIds);
        List<Guid> GetFeatureIdListThatRelatesToObjectId(Guid nodeId);
    }
}