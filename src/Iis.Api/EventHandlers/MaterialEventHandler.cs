using System.Threading;
using System.Threading.Tasks;
using Iis.Api.BackgroundServices;
using Iis.Events.Materials;
using MediatR;

namespace Iis.EventHandlers
{
    public class MaterialEventHandler : INotificationHandler<MaterialCreatedEvent>
    {
        public Task Handle(MaterialCreatedEvent notification, CancellationToken cancellationToken)
        {
            ThemeCounterBackgroundService.SignalThemeUpdateNeeded();
            return Task.CompletedTask;
        }
    }
}
