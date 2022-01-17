using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.DataModel.ChangeHistory;
using Iis.DataModel.Materials;
using Iis.Domain.Materials;
using Iis.Interfaces.Common;
using Iis.Interfaces.Materials;
using Iis.Elastic.Entities;

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
        Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutAllMaterialChangesToElasticSearchAsync(CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutMaterialChangesToElasticSearchAsync(IReadOnlyCollection<ChangeHistoryEntity> changes, bool waitForIndexing = false, CancellationToken cancellationToken = default);
        Task<List<ElasticBulkResponse>> PutCreatedMaterialsToElasticSearchAsync(IReadOnlyCollection<Guid> materialIds, bool waitForIndexing = false, CancellationToken cancellationToken = default);
        Task<bool> PutMaterialToElasticSearchAsync(Guid materialId, bool waitForIndexing = false, CancellationToken cancellationToken = default);
        Task PutMaterialsToElasticSearchAsync(ISet<Guid> materialIdSet, bool waitForIndexing = false, CancellationToken cancellationToken = default);
        Task<bool> PutMaterialToElasticSearchAsync(MaterialEntity material, bool waitForIndexing = false, CancellationToken cancellationToken = default);
        Task PutMaterialsToElasticByNodeIdsAsync(IReadOnlyCollection<Guid> nodeIds, bool waitForIndexing = false, CancellationToken cancellationToken = default);
        Task RemoveMaterialAsync(Guid materialId, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<MaterialDocument>> GetMaterialCollectionByIdCollectionAsync(IReadOnlyCollection<Guid> idCollection, Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<MaterialDocument>> GetMaterialCollectionRelatedToNodeAsync(Guid nodeId, Guid userId, CancellationToken cancellationToken);
        Task<OutputCount<Guid>> CountMaterialCollectionRelatedToNodeAsync(Guid nodeId, Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyDictionary<string, int>> CountMaterialsByTypeAndNodeAsync(Guid nodeId, Guid userId, CancellationToken cancellationToken = default);
    }
}