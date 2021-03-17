using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Services.Contracts;
using Iis.Interfaces.Elastic;
using Iis.Interfaces.Materials;
using Iis.Domain.MachineLearning;
using Material = Iis.Domain.Materials.Material;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material, Guid? changeRequestId = null);
        Task<MLResponse> SaveMlHandlerResponseAsync(MLResponse response);
        Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input, Guid userId, string username);
        Task AssignMaterialOperatorAsync(Guid materialId, Guid assigneeId);
        Task SetMachineLearningHadnlersCount(Guid materialId, int handlersCount);
        Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken);
        Task<List<ElasticBulkResponse>> PutCreatedMaterialsToElasticSearchAsync(IReadOnlyCollection<Guid> materialIds, CancellationToken stoppingToken);
        Task<Material> ChangeMaterialAccessLevel(Guid materialId, byte accessLevel, User user);
    }
}
