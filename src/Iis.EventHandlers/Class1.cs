using Iis.Events.Reports;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.EventHandlers
{
    public class ReportEventHandler :
        INotificationHandler<ReportCreatedEvent>,
        INotificationHandler<ReportUpdatedEvent>,
        INotificationHandler<ReportRemovedEvent>
    {
        public Task Handle(ReportRemovedEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Handle(ReportUpdatedEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task Handle(ReportCreatedEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
