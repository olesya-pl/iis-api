using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.DataStorage;
using System.Linq;

namespace Iis.MaterialDistributor.MediatR.EventHandlers
{
    public class ReadAllMaterialsEventHandler : INotificationHandler<ReadAllMaterialsEvent>
    {
        private readonly IMaterialDistributionService _materialDistributionService;
        private readonly IVariableCoefficientService _coefficientService;
        private readonly IDistributionData _distributionData;

        public ReadAllMaterialsEventHandler(
            IMaterialDistributionService materialDistributionService,
            IVariableCoefficientService coefficientService,
            IDistributionData distributionData)
        {
            _materialDistributionService = materialDistributionService;
            _coefficientService = coefficientService;
            _distributionData = distributionData;
        }

        public async Task Handle(ReadAllMaterialsEvent notification, CancellationToken cancellationToken)
        {
            var materials = (await _materialDistributionService
                .GetMaterialCollectionAsync(cancellationToken))
                .ToDictionary(_ => _.Id);
            var users = await _materialDistributionService.GetOperatorsAsync(cancellationToken);
            _distributionData.RefreshMaterialsAsync(materials);
            _distributionData.Distribute(users);
        }
    }
}