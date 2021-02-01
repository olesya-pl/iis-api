using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Iis.DataModel;
using Iis.DataModel.ChangeHistory;
using Iis.Services.Contracts.Dtos;

namespace Iis.MaterialLoader
{
    public interface IChangeHistoryService
    {
        Task SaveMaterialChanges(IReadOnlyCollection<ChangeHistoryDto> changes);
    }

    public class ChangeHistoryService : IChangeHistoryService
    {
        private readonly IMapper _mapper;
        private readonly OntologyContext _dbContext;

        public ChangeHistoryService(IMapper mapper, OntologyContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
        }

        public Task SaveMaterialChanges(IReadOnlyCollection<ChangeHistoryDto> changes)
        {
            var entities = _mapper.Map<List<ChangeHistoryEntity>>(changes);
            foreach (var entity in entities)
            {
                entity.Id = Guid.NewGuid();
                entity.Type = ChangeHistoryEntityType.Material;
            }

            _dbContext.ChangeHistory.AddRange(entities);
            return _dbContext.SaveChangesAsync();
        }
    }
}