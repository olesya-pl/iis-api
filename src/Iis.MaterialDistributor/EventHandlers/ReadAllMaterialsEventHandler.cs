using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.MediatR.EventHandlers
{
    public class ReadAllMaterialsEventHandler : INotificationHandler<ReadAllMaterialsEvent>
    {
        private readonly IMaterialService _materialService;
        private readonly IVariableCoefficientService _coefficientService;

        public ReadAllMaterialsEventHandler(
            IMaterialService materialService,
            IVariableCoefficientService coefficientService)
        {
            _materialService = materialService;
            _coefficientService = coefficientService;
        }

        public async Task Handle(ReadAllMaterialsEvent notification, CancellationToken cancellationToken)
        {
            var coefficient = await _coefficientService.GetWithMaxOffsetHoursAsync(cancellationToken);

            if (coefficient is null) return;

            var materialCollection = await _materialService.GetMaterialCollectionAsync(coefficient.OffsetHours, cancellationToken);

            materialCollection = await _coefficientService.SetForMaterialsAsync(DateTime.UtcNow, materialCollection, cancellationToken);
        }
    }
}