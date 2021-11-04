using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.Domain.MachineLearning;
using Iis.Domain.Users;
using Iis.Interfaces.Materials;
using Iis.Services.Contracts.Materials.Distribution;
using Material = Iis.Domain.Materials.Material;

namespace IIS.Core.Materials
{
    public interface IMaterialService
    {
        Task SaveAsync(Material material, Guid? changeRequestId = null);
        Task<MLResponse> SaveMlHandlerResponseAsync(MLResponse response);
        Task<Material> UpdateMaterialAsync(IMaterialUpdateInput input, User user);
        Task AssignMaterialOperatorAsync(ISet<Guid> materialIds, ISet<Guid> assigneeIds, User user);
        Task AssignMaterialOperatorsAsync(Guid materialId, ISet<Guid> assigneeIds, User user);
        Task SaveDistributionResult(DistributionResult distributionResult);
        Task<bool> AssignMaterialEditorAsync(Guid materialId, User user);
        Task<bool> UnassignMaterialEditorAsync(Guid materialId, User user);
        Task SetMachineLearningHadnlersCount(Guid materialId, int handlersCount);
        Task<Material> ChangeMaterialAccessLevel(Guid materialId, int accessLevel, User user);
        Task RemoveMaterials();
        Task RemoveMaterialAsync(Guid materialId, CancellationToken cancellationToken);
    }
}