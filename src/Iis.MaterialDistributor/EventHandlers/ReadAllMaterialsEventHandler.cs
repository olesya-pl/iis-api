using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;
using Iis.MaterialDistributor.Contracts.DataStorage;

namespace Iis.MaterialDistributor.MediatR.EventHandlers
{
    public class ReadAllMaterialsEventHandler : INotificationHandler<ReadAllMaterialsEvent>
    {
        private readonly IDistributionDataMediator _distributionDataMediator;

        public ReadAllMaterialsEventHandler(
            IDistributionDataMediator distributionDataMediator)
        {
            _distributionDataMediator = distributionDataMediator;
        }

        public async Task Handle(ReadAllMaterialsEvent notification, CancellationToken cancellationToken)
        {
            await _distributionDataMediator.RefreshMaterialsAsync(cancellationToken);
            await _distributionDataMediator.DistributeAsync(cancellationToken);
        }
    }
}