using System.Threading;
using System.Threading.Tasks;
using Iis.Api.BackgroundServices;
using Iis.Events.Entities;
using MediatR;

namespace Iis.EventHandlers
{
    public class EntityEventHander : INotificationHandler<EntityCreatedEvent>,
        INotificationHandler<EntityUpdatedEvent>
    {
        public Task Handle(EntityCreatedEvent notification, CancellationToken cancellationToken)
        {
            ThemeCounterBackgroundService.SignalThemeUpdateNeeded();
            return Task.CompletedTask;
        }

        public Task Handle(EntityUpdatedEvent notification, CancellationToken cancellationToken)
        {
            ThemeCounterBackgroundService.SignalThemeUpdateNeeded();
            return Task.CompletedTask;
        }
    }
}
