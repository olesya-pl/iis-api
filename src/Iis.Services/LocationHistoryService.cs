using System;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel.FlightRadar;
using Iis.DbLayer.Repositories;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Repository;
using IIS.Repository.Factories;

namespace Iis.Services
{
    public class LocationHistoryService<TUnitOfWork> : BaseService<TUnitOfWork>, ILocationHistoryService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IMapper _mapper;
        public LocationHistoryService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory, IMapper mapper)
        : base(unitOfWorkFactory)
        {
            _mapper = mapper;
        }

        public async Task<LocationHistoryDto> GetLatestLocationHistoryAsync(Guid entityId)
        {
            var entity = await RunWithoutCommitAsync(uow => uow.LocationHistoryRepository.GetLatestLocationHistoryEntityAsync(entityId));
            return _mapper.Map<LocationHistoryDto>(entity);
        }

        public Task SaveLocationHistoryAsync(LocationHistoryDto locationHistoryDto)
        {
            if(locationHistoryDto is null) return Task.CompletedTask;;

            var entityList = new[]
            { 
                _mapper.Map<LocationHistoryEntity>(locationHistoryDto)
            };

            return RunAsync(uow => uow.LocationHistoryRepository.SaveAsync(entityList));
        }
    }
}
