using AutoMapper;
using Iis.DataModel;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Enums;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using IIS.Repository;
using IIS.Repository.Factories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services
{

    public class AliasService<TUnitOfWork> : BaseService<TUnitOfWork>, IAliasService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IMapper _mapper;

        public AliasService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory, IMapper mapper)
            : base(unitOfWorkFactory)
        {
            _mapper = mapper;
        }

        public async Task<List<AliasDto>> GetByTypeAsync(AliasType type)
        {
            var aliases = await RunWithoutCommitAsync(uow => uow.AliasRepository.GetByTypeAsync(type));

            return _mapper.Map<List<AliasDto>>(aliases);
        }

        public async Task<List<AliasDto>> GetAllAsync()
        {
            var aliases = await RunWithoutCommitAsync(uow => uow.AliasRepository.GetAllAsync());

            return _mapper.Map<List<AliasDto>>(aliases);
        }

        public async Task<AliasDto> CreateAsync(AliasDto aliasDto)
        {
            var alias = _mapper.Map<AliasEntity>(aliasDto);
            alias.Id = Guid.NewGuid();

            await RunAsync(uow => uow.AliasRepository.Create(alias));

            return _mapper.Map<AliasDto>(alias);
        }
    }
}
