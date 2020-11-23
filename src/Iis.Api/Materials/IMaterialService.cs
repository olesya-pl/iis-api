using System.Threading.Tasks;
using System.Collections.Generic;
using Iis.Domain.MachineLearning;
using Material = Iis.Domain.Materials.Material;
using Iis.Interfaces.Materials;
using System;
using System.Threading;
using Iis.Interfaces.Elastic;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material);
        Task<MLResponse> SaveMlHandlerResponseAsync(MLResponse response);
        Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input, Guid userId);
        Task AssignMaterialOperatorAsync(Guid materialId, Guid assigneeId);
        Task SetMachineLearningHadnlersCount(Guid materialId, int handlersCount);
        Task<List<ElasticBulkResponse>> PutAllMaterialsToElasticSearchAsync(CancellationToken cancellationToken);
    }
}
