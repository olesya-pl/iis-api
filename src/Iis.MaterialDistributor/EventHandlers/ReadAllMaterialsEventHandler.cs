using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.MediatR.EventHandlers
{
    public class ReadAllMaterialsEventHandler : INotificationHandler<ReadAllMaterialsEvent>
    {
        private readonly IMaterialDistributionService _materialService;
        private readonly IVariableCoefficientService _coefficientService;
        private readonly IDistributionData _distributionData;

        public ReadAllMaterialsEventHandler(
            IMaterialDistributionService materialService,
            IVariableCoefficientService coefficientService,
            IDistributionData distributionData)
        {
            _materialService = materialService;
            _coefficientService = coefficientService;
            _distributionData = distributionData;
        }

        public async Task Handle(ReadAllMaterialsEvent notification, CancellationToken cancellationToken)
        {
            await _distributionData.RefreshMaterialsAsync(cancellationToken);
            await _distributionData.Distribute(cancellationToken);
        }
    }
}