using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.DataModel.ChangeHistory;
using Iis.DataModel.Materials;
using Iis.DbLayer.Repositories;
using Iis.Domain.Materials;
using Iis.Interfaces.Materials;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IMaterialElasticService
    {
        Task<SearchResult> SearchMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, RelationsState? materialRelationsState, CancellationToken ct = default);
        Task<SearchResult> BeginSearchByScrollAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchMaterialsAsync(Guid userId, SearchParams searchParams, IEnumerable<Guid> materialList, CancellationToken ct = default);
        Task<SearchResult> SearchMoreLikeThisAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchByImageVector(Guid userId, IReadOnlyCollection<decimal[]> imageVectorList, PaginationParams page, CancellationToken ct = default);
        Task<int> CountMaterialsByConfiguredFieldsAsync(Guid userId, SearchParams searchParams, CancellationToken ct = default);
        Task<SearchResult> SearchByScroll(Guid userId, string scrollId);
        Task<MaterialDocument> GetMaterialById(Guid userId, Guid materialId);
        bool ShouldReturnNoEntities(string queryExpression);
        Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken ct = default);
        Task<List<ElasticBulkResponse>> PutAllMaterialChangesToElasticSearchAsync(CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutMaterialChangesToElasticSearchAsync(IReadOnlyCollection<ChangeHistoryEntity> changes, bool waitForIndexing = false, CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutCreatedMaterialsToElasticSearchAsync(IReadOnlyCollection<Guid> materialIds, bool waitForIndexing = false, CancellationToken token = default);
        Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, CancellationToken ct = default, bool waitForIndexing = false);
        Task PutMaterialsToElasticSearchAsync(IEnumerable<Guid> materialIds, CancellationToken ct = default, bool waitForIndexing = false);
        Task<bool> PutMaterialToElasticSearchAsync(MaterialEntity material, CancellationToken ct = default, bool waitForIndexing = false);
        Task PutMaterialsToElasticByNodeIdsAsync(IReadOnlyCollection<Guid> nodeIds, CancellationToken ct = default, bool waitForIndexing = false);
        Task RemoveMaterialAsync(Guid materialId, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<MaterialDocument>> GetMaterialCollectionByIdCollectionAsync(IReadOnlyCollection<Guid> idCollection, Guid userId, CancellationToken cancellationToken);
    }
}