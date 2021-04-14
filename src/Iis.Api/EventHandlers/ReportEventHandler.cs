using AutoMapper;
using Iis.Api.BackgroundServices;
using Iis.Events.Reports;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.EventHandlers
{
    public class ReportEventHandler :
        INotificationHandler<ReportCreatedEvent>,
        INotificationHandler<ReportUpdatedEvent>,
        INotificationHandler<ReportRemovedEvent>
    {
        private readonly IReportElasticService _reportElasticService;
        private readonly IMapper _mapper;

        public ReportEventHandler(IMapper mapper, IReportElasticService reportElasticService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _reportElasticService = reportElasticService ?? throw new ArgumentNullException(nameof(reportElasticService));
        }

        public Task Handle(ReportRemovedEvent notification, CancellationToken cancellationToken)
        {
            ThemeCounterBackgroundService.SignalReportUpdateNeeded();
            return _reportElasticService.RemoveAsync(notification.Id);
        }

        public Task Handle(ReportUpdatedEvent notification, CancellationToken cancellationToken)
        {
            ThemeCounterBackgroundService.SignalReportUpdateNeeded();
            return _reportElasticService.PutAsync(_mapper.Map<ReportDto>(notification));
        }

        public Task Handle(ReportCreatedEvent notification, CancellationToken cancellationToken)
        {
            ThemeCounterBackgroundService.SignalReportUpdateNeeded();
            return _reportElasticService.PutAsync(_mapper.Map<ReportDto>(notification));
        }
    }
}
