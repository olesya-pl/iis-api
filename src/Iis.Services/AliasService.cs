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

        public async Task<AliasDto> UpdateAsync(AliasDto aliasDto)
        {
            if (aliasDto.Id == Guid.Empty)
                throw new ArgumentException($"{aliasDto.Id} should not be null", nameof(aliasDto.Id));

            var alias = await RunWithoutCommitAsync(uow => uow.AliasRepository.GetByIdAsync(aliasDto.Id));
            if (alias == null)
                throw new ArgumentException($"Cannot find alias with id = {alias.Id}");

            alias.DotName = aliasDto.DotName;
            alias.Type = aliasDto.Type;
            alias.Value = aliasDto.Value;

            await RunAsync(uow => uow.AliasRepository.Update(alias));

            return _mapper.Map<AliasDto>(alias);
        }

        public async Task<AliasDto> RemoveAsync(Guid id) 
        {
            if (id == Guid.Empty)
                throw new ArgumentException($"{id} should not be empty", nameof(id));

            var alias = await RunWithoutCommitAsync(uow => uow.AliasRepository.GetByIdAsync(id));
            if (alias == null)
                throw new ArgumentException($"Cannot find alias with id = {id}");

            await RunAsync(uow => uow.AliasRepository.Remove(alias));

            return _mapper.Map<AliasDto>(alias);
        }
    }
}
