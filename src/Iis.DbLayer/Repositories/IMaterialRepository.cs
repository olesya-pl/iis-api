using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Iis.DataModel.Materials;
using Iis.DbLayer.Common;
using Iis.DbLayer.MaterialEnum;
using Iis.Domain.Materials;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;
using Iis.Services.Contracts.Materials.Distribution;

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

        Task<PaginatedCollection<MaterialEntity>> GetAllAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<PaginatedCollection<MaterialEntity>> GetAllAsync(IEnumerable<Guid> materialIdList, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<PaginatedCollection<MaterialEntity>> GetAllAsync(IEnumerable<string> types, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId);

        Task<MaterialAccessEntity> GetMaterialAccessByIdAsync(Guid materialId, CancellationToken cancellationToken = default);

        void AddMaterialEntity(MaterialEntity materialEntity);

        void AddMaterialInfos(IEnumerable<MaterialInfoEntity> materialEntities);

        void AddMaterialFeatures(IEnumerable<MaterialFeatureEntity> materialFeatureEntities);

        void AddMaterialAssignees(IEnumerable<MaterialAssigneeEntity> entities);

        void RemoveMaterialAssignees(IEnumerable<MaterialAssigneeEntity> entities);

        Task EditMaterialAsync(Guid id, Action<MaterialEntity> editAction, params MaterialIncludeEnum[] includes);

        Task<List<Guid>> GetNodeIsWithMaterialsAsync(IReadOnlyCollection<Guid> nodeIdCollection);

        Task<IReadOnlyCollection<MaterialEntity>> GetMaterialCollectionByNodeIdAsync(IReadOnlyCollection<Guid> nodeIdCollection, params MaterialIncludeEnum[] includes);

        Task<IReadOnlyCollection<Guid>> GetMaterialIdCollectionByNodeIdCollectionAsync(IReadOnlyCollection<Guid> nodeIdCollection);

        Task<List<MaterialsCountByType>> GetParentMaterialByNodeIdQueryAsync(IReadOnlyCollection<Guid> nodeIdCollection);

        void AddFeatureIdList(Guid materialId, IEnumerable<Guid> featureIdList);

        Task<IEnumerable<Guid>> GetChildIdListForMaterialAsync(Guid materialId);

        Task<bool> CheckMaterialExistsAndHasContent(Guid materialId);

        Task RemoveMaterialsAndRelatedData(IReadOnlyCollection<Guid> fileIdList);

        Task<Guid?> GetParentIdByChildIdAsync(Guid materialId);

        Task<int> GetTotalCountAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<MaterialChannelMappingEntity>> GetChannelMappingsAsync();
        Task<IReadOnlyList<MaterialDistributionItem>> GetMaterialsForDistribution(
            UserDistributionItem user,
            Expression<Func<MaterialEntity, bool>> filter);
        Task SaveDistributionResult(DistributionResult distributionResult);
        void RemoveMaterialAndRelatedData(Guid materialId);
        Task<IReadOnlyList<ResCallerReceiverDto>> GetCallInfoAsync(IReadOnlyList<Guid> nodeIds, CancellationToken cancellationToken = default);
    }
}