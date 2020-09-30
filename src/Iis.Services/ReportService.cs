using AutoMapper;
using Iis.DataModel.Reports;
using Iis.DbLayer.Repositories;
using Iis.Events.Reports;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Repository;
using IIS.Repository.Factories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class ReportService<TUnitOfWork> : BaseService<TUnitOfWork>, IReportService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IMapper _mapper;
        private readonly IMediator _mediatr;
        public ReportService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory, IMapper mapper, IMediator mediatr)
            : base(unitOfWorkFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _mediatr = mediatr ?? throw new ArgumentNullException(nameof(mediatr));
        }

        public async Task<(int Count, List<ReportDto> Items)> GetReportPageAsync(int size, int offset)
        {
            var (count, items) = await RunWithoutCommitAsync(uow => uow.ReportRepository.GetReportPageAsync(size, offset));

            return (count, _mapper.Map<List<ReportDto>>(items));
        }

        public async Task<ReportDto> CreateAsync(ReportDto report)
        {
            var reportEntity = _mapper.Map<ReportEntity>(report);

            reportEntity.Id = Guid.NewGuid();
            reportEntity.CreatedAt = DateTime.Now;

            await RunAsync(uow => uow.ReportRepository.Create(reportEntity));
            try
            {
                await _mediatr.Publish(_mapper.Map<ReportCreatedEvent>(reportEntity));
            }
            catch (Exception e)
            {
                throw;
            }
            
            return _mapper.Map<ReportDto>(reportEntity);
        }

        public async Task<ReportDto> UpdateAsync(ReportDto report)
        {
            if (report.Id == Guid.Empty)
                throw new ArgumentException($"{report.Id} should not be null", nameof(report.Id));

            var reportEntity = await GetByIdAsync(report.Id);
            if (reportEntity == null)
                throw new ArgumentException($"Cannot find report with id  = {report.Id}");

            reportEntity.Recipient = report.Recipient;
            reportEntity.Title = report.Title;

            await RunAsync(uow => uow.ReportRepository.Update(reportEntity));

            await _mediatr.Publish(_mapper.Map<ReportUpdatedEvent>(reportEntity));
            return _mapper.Map<ReportDto>(reportEntity);
        }

        public async Task<ReportDto> RemoveAsync(Guid id)
        {
            var report = await GetByIdAsync(id);
            if (report == null)
                throw new ArgumentException($"Cannot find report with id  = {id}");

            var reportDto = _mapper.Map<ReportDto>(report);
            await RunAsync(uow => uow.ReportRepository.Remove(report));

            await _mediatr.Publish(_mapper.Map<ReportRemovedEvent>(report));
            return reportDto;
        }

        public async Task<ReportDto> CopyAsync(Guid sourceId, ReportDto newReport)
        {
            var sourceReport = await GetByIdAsync(sourceId);
            if (sourceReport == null)
                throw new ArgumentException($"Cannot find report with id  = {sourceId}");

            var copiedReport = new ReportEntity(sourceReport, Guid.NewGuid(), DateTime.Now);

            copiedReport.Title = newReport.Title ?? newReport.Title;
            copiedReport.Recipient = newReport.Recipient ?? newReport.Recipient;

            await RunAsync(uow => uow.ReportRepository.Create(copiedReport));

            await _mediatr.Publish(_mapper.Map<ReportCreatedEvent>(copiedReport));
            return _mapper.Map<ReportDto>(copiedReport);
        }

        public async Task<ReportDto> UpdateEventsAsync(Guid id, IEnumerable<Guid> eventIdsToAdd, IEnumerable<Guid> eventIdsToRemove)
        {
            var forAdd = new HashSet<Guid>(eventIdsToAdd);
            var forRemove = new HashSet<Guid>(eventIdsToRemove);

            forAdd.ExceptWith(eventIdsToRemove);
            forRemove.ExceptWith(eventIdsToAdd);

            var eventsToAdd = forAdd.Select(eventId => new ReportEventEntity
            {
                EventId = eventId,
                ReportId = id
            }).ToList();

            var eventsToRemove = await RunWithoutCommitAsync(uow => uow.ReportRepository.GetEventsAsync(id, forRemove));

            if (eventsToAdd.Count > 0)
                await RunAsync(uow => uow.ReportRepository.AddEvents(eventsToAdd));

            if (eventsToRemove.Count > 0)
                await RunAsync(uow => uow.ReportRepository.RemoveEvents(eventsToRemove));

            var report = await GetByIdAsync(id);

            await _mediatr.Publish(_mapper.Map<ReportUpdatedEvent>(report));
            return _mapper.Map<ReportDto>(report);
        }

        public async Task<ReportDto> GetAsync(Guid id) 
        {
            var report = await GetByIdAsync(id);

            return _mapper.Map<ReportDto>(report);
        }

        private Task<ReportEntity> GetByIdAsync(Guid id) => RunWithoutCommitAsync(uow => uow.ReportRepository.GetByIdAsync(id));
    }
}
