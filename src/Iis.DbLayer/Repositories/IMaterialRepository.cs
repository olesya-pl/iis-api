using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Iis.DbLayer.MaterialEnum;
using System.Threading;
using Iis.Domain.Materials;
using Iis.Interfaces.Elastic;
using Iis.Services.Contracts.Materials.Distribution;
using System.Linq.Expressions;

namespace Iis.DbLayer.Repositories
{
    public interface IMaterialRepository
    {
        IReadOnlyCollection<string> MaterialIndexes { get; }
        Task<MaterialEntity> GetByIdAsync(Guid id, params MaterialIncludeEnum[] includes);

        Task<MaterialEntity[]> GetByIdsAsync(ISet<Guid> ids, params MaterialIncludeEnum[] includes);

        Task<IEnumerable<MaterialEntity>> GetAllAsync(params MaterialIncludeEnum[] includes);

        Task<IEnumerable<MaterialEntity>> GetAllAsync(int limit, params MaterialIncludeEnum[] includes);
        
        Task<IEnumerable<MaterialEntity>> GetAllForRelatedNodeListAsync(IEnumerable<Guid> nodeIdList);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<Guid> materialIdList, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<(IEnumerable<MaterialEntity> Entities, int TotalCount)> GetAllAsync(IEnumerable<string> types, int limit, int offset, string sortColumnName = null, string sortOrder = null);

        Task<IReadOnlyCollection<Guid>> GetAllUnassignedIdsAsync(int limit, int offset, string sortColumnName = null, string sortOrder = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<MaterialEntity>> GetAllByAssigneeIdAsync(Guid assigneeId);

        Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken ct = default);

        Task<List<ElasticBulkResponse>> PutCreatedMaterialsToElasticSearchAsync(IReadOnlyCollection<Guid> materialIds, bool waitForIndexing = false, CancellationToken token = default);

        Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, CancellationToken ct = default, bool waitForIndexing = false);
        Task PutMaterialsToElasticSearchAsync(IEnumerable<Guid> materialIds, CancellationToken ct = default, bool waitForIndexing = false);

        void AddMaterialEntity(MaterialEntity materialEntity);

        void AddMaterialInfos(IEnumerable<MaterialInfoEntity> materialEntities);

        void AddMaterialFeatures(IEnumerable<MaterialFeatureEntity> materialFeatureEntities);

        void EditMaterial(MaterialEntity materialEntity);

        Task<List<Guid>> GetNodeIsWithMaterialsAsync(IReadOnlyCollection<Guid> nodeIdCollection);

        Task<IReadOnlyCollection<MaterialEntity>> GetMaterialCollectionByNodeIdAsync(IReadOnlyCollection<Guid> nodeIdCollection, params MaterialIncludeEnum[] includes);

        Task<IReadOnlyCollection<Guid>> GetMaterialIdCollectionByNodeIdCollectionAsync(IReadOnlyCollection<Guid> nodeIdCollection);

        Task<List<MaterialsCountByType>> GetParentMaterialByNodeIdQueryAsync(IReadOnlyCollection<Guid> nodeIdCollection);

        void AddFeatureIdList(Guid materialId, IEnumerable<Guid> featureIdList);

        Task<IEnumerable<Guid>> GetChildIdListForMaterialAsync(Guid materialId);

        Task<bool> CheckMaterialExistsAndHasContent(Guid materialId);

        Task RemoveMaterialsAndRelatedData(IReadOnlyCollection<Guid> fileIdList);

        Task<Guid?> GetParentIdByChildIdAsync(Guid materialId);
        Task<IReadOnlyList<MaterialChannelMappingEntity>> GetChannelMappingsAsync();
        Task<IReadOnlyList<MaterialDistributionItem>> GetMaterialsForDistribution(UserDistributionItem user,
            Expression<Func<MaterialEntity, bool>> filter,
            IReadOnlyList<Guid> distributedIds);
        Task SaveDistributionResult(DistributionResult distributionResult);
    }
}