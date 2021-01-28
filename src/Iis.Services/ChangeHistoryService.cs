using AutoMapper;
using Iis.DataModel.ChangeHistory;
using Iis.DbLayer.Repositories;
using Iis.Interfaces.Ontology.Data;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Repository;
using IIS.Repository.Factories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task SaveNodeChange(
            string attributeDotName,
            Guid targetId,
            string userName,
            object oldValue,
            object newValue,
            string parentTypeName,
            Guid requestId)
        {
            var oldInfo = ParseValue(oldValue);
            var newInfo = ParseValue(newValue);
            if (oldInfo.value == newInfo.value) return;

            var changeHistoryEntity = new ChangeHistoryEntity
            {
                Id = Guid.NewGuid(),
                TargetId = targetId,
                UserName = userName,
                PropertyName = attributeDotName,
                Date = DateTime.Now,
                OldValue = oldInfo.value,
                NewValue = newInfo.value,
                RequestId = requestId,
                Type = ChangeHistoryEntityType.Node,
                ParentTypeName = parentTypeName,
                OldTitle = oldInfo.title,
                NewTitle = newInfo.title
            };

            await RunAsync(uow => uow.ChangeHistoryRepository.Add(changeHistoryEntity));
        }

        private (string value, string title) ParseValue(object valueObj)
        {
            if (valueObj == null) return (null, null);

            if (valueObj is string) return ((string)valueObj, null);

            if (valueObj is Guid id) return (id.ToString("N"), GetTitle(id));

            return (JsonConvert.SerializeObject(valueObj), null);
        }

        private string GetTitle(Guid id)
        {
            var node = _ontologyNodesData.GetNode(id);
            if (node == null) return null;

            return node.GetComputedValue("__title") ?? node.GetSingleProperty("name")?.Value;

        }

        public async Task SaveMaterialChanges(IReadOnlyCollection<ChangeHistoryDto> changes)
        {
            var entities = _mapper.Map<List<ChangeHistoryEntity>>(changes);
            foreach (var entity in entities)
            {
                entity.Id = Guid.NewGuid();
                entity.Type = ChangeHistoryEntityType.Material;
            }
            await RunAsync(uow => uow.ChangeHistoryRepository.AddRange(entities));
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

        public async Task<List<ChangeHistoryDto>> GetLocationHistory(Guid entityId)
        {
            var locations = await RunWithoutCommitAsync(uow => uow.FlightRadarRepository.GetLocationHistory(entityId));
            return locations.Select(l =>
                new ChangeHistoryDto
                {
                    Date = l.RegisteredAt,
                    NewValue = "{\"type\":\"Point\",\"coordinates\":[" + l.Lat.ToString() + "," + l.Long.ToString() + "]}",
                    PropertyName = "sign.location",
                    TargetId = entityId,
                    Type = 0
                }).ToList();
        }
    }
}
