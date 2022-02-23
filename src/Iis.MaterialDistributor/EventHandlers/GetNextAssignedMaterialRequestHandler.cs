using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;
using Iis.MaterialDistributor.Contracts.DataStorage;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.MediatR.EventHandlers
{
    public class GetNextAssignedMaterialRequestHandler : IRequestHandler<GetNextAssignedMaterial, Guid?>
    {
        private readonly IDistributionData _distributionData;

        public GetNextAssignedMaterialRequestHandler(
            IDistributionData distributionData)
        {
            _distributionData = distributionData;
        }

        public Task<Guid?> Handle(GetNextAssignedMaterial request, CancellationToken cancellationToken)
        {
            var material = _distributionData.GetMaterialFromQueue(new UserDistributionInfo { Id = request.UserId });

            return Task.FromResult<Guid?>(material?.Id);
        }
    }
}