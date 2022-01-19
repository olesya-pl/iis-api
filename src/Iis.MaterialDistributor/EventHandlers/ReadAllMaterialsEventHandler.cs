using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Iis.MaterialDistributor.Contracts.Events;
using Iis.MaterialDistributor.Contracts.Services;
namespace Iis.MaterialDistributor.MediatR.EventHandlers
{
    public class ReadAllMaterialsEventHandler : INotificationHandler<ReadAllMaterialsEvent>
    {
        private IMaterialService _materialService;
        public ReadAllMaterialsEventHandler(
            IMaterialService materialService)
        {
            _materialService = materialService;
        }
        public async Task Handle(ReadAllMaterialsEvent notification, CancellationToken cancellationToken)
        {
            var materialCollection = await _materialService.GetMaterialCollectionAsync(notification.HourOffset, cancellationToken);
        }
    }
}