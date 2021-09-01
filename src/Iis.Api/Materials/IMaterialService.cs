using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;
using Iis.Domain.MachineLearning;
using Material = Iis.Domain.Materials.Material;
using Iis.Domain.Users;
using Iis.Services.Contracts.Materials.Distribution;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material, Guid? changeRequestId = null);
        Task<MLResponse> SaveMlHandlerResponseAsync(MLResponse response);
        Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input, User user);
        Task AssignMaterialsOperatorAsync(ISet<Guid> materialIds, Guid assigneeId, User user);
        Task AssignMaterialOperatorAsync(Guid materialId, Guid assigneeId, User user = null);
        Task SaveDistributionResult(DistributionResult distributionResult);
        Task<bool> AssignMaterialEditorAsync(Guid materialId, User user);
        Task<bool> UnassignMaterialEditorAsync(Guid materialId, User user);
        Task SetMachineLearningHadnlersCount(Guid materialId, int handlersCount);
        Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken);
        Task<List<ElasticBulkResponse>> PutCreatedMaterialsToElasticSearchAsync(IReadOnlyCollection<Guid> materialIds, bool waitForIndexing, CancellationToken stoppingToken);
        Task<Material> ChangeMaterialAccessLevel(Guid materialId, int accessLevel, User user);
        Task RemoveMaterials();
    }
}
