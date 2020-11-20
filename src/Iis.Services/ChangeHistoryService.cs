using AutoMapper;
using Iis.DataModel.ChangeHistory;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Ontology;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Repository;
using IIS.Repository.Factories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Services
{
    public class ChangeHistoryService<TUnitOfWork> : BaseService<TUnitOfWork>, IChangeHistoryService where TUnitOfWork : IIISUnitOfWork
    {
        private readonly IOntologyNodesData _ontologyNodesData;
        private readonly IMapper _mapper;
        public ChangeHistoryService(IUnitOfWorkFactory<TUnitOfWork> unitOfWorkFactory, IMapper mapper, IOntologyNodesData ontologyNodesData) : base(unitOfWorkFactory)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _ontologyNodesData = ontologyNodesData ?? throw new ArgumentNullException(nameof(ontologyNodesData));
        }

        public async Task SaveChange(
            string attributeDotName,
            Guid targetId,
            string userName,
            string oldValue,
            string newValue,
            Guid requestId)
        {
            var changeHistoryEntity = new ChangeHistoryEntity
            {
                Id = Guid.NewGuid(),
                TargetId = targetId,
                UserName = userName,
                PropertyName = attributeDotName,
                Date = DateTime.Now,
                OldValue = oldValue,
                NewValue = newValue,
                RequestId = requestId
            };

            await RunAsync(uow => uow.ChangeHistoryRepository.Add(changeHistoryEntity));
        }

        public async Task<List<ChangeHistoryDto>> GetChangeHistory(ChangeHistoryParams parameters)
        {
            var entities = await RunWithoutCommitAsync(uow => uow.ChangeHistoryRepository.GetManyAsync(parameters.TargetId, parameters.PropertyName, parameters.DateFrom, parameters.DateTo));
            var result = _mapper.Map<List<ChangeHistoryDto>>(entities);

            if (parameters.ApplyAliases) 
            {
                var node = _ontologyNodesData.GetNode(parameters.TargetId);
                if (node == null)
                    return result;

                foreach (var item in result)
                {
                    var alias = _ontologyNodesData.Schema.GetAlias($"{node.NodeType.Name}.{item.PropertyName}");
                    item.PropertyName = alias ?? item.PropertyName;
                }
            }

            return result;
        }

        public async Task<List<ChangeHistoryDto>> GetChangeHistory(IEnumerable<Guid> ids)
        {
            var entities = await RunWithoutCommitAsync(uow => uow.ChangeHistoryRepository.GetByIdsAsync(ids));

            return _mapper.Map<List<ChangeHistoryDto>>(entities);
        }

        public async Task<List<ChangeHistoryDto>> GetChangeHistoryByRequest(Guid requestId)
        {
            var entities = await RunWithoutCommitAsync(uow => uow.ChangeHistoryRepository.GetByRequestIdAsync(requestId));

            return _mapper.Map<List<ChangeHistoryDto>>(entities);
        }
    }
}
