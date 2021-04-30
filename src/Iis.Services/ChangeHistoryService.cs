using AutoMapper;
using Iis.DataModel.ChangeHistory;
using Iis.DataModel.FlightRadar;
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
        private const string DefaultSignLocationPropName = "sign.location";
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
            if (oldInfo.value == newInfo.value || IsInternalEntity(oldValue) || IsInternalEntity(newValue)) 
                return;

            var changeHistoryEntity = new ChangeHistoryEntity
            {
                Id = Guid.NewGuid(),
                TargetId = targetId,
                UserName = userName,
                PropertyName = attributeDotName,
                Date = DateTime.UtcNow,
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

        private bool IsInternalEntity(object value)
        {
            if (value == null) return false;

            if (value is Dictionary<string, object> dict && (dict.ContainsKey("target") || dict.ContainsKey("targetId")))
            {
                return true;
            }

            if (value is Guid id)
            {
                var node = _ontologyNodesData.GetNode(id);
                if (node == null) return false;

                var nt = node.NodeType;

                return !(nt.IsObject || nt.IsEvent || nt.IsEnum);
            }
            return false;
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

            if (node.NodeType.IsObjectOfStudy)
            {

            }

            return node.GetComputedValue("__title") ?? node.GetSingleProperty("name")?.Value;
        }

        private string GetTitle(string strId)
        {
            return string.IsNullOrEmpty(strId) ?
                null :
                GetTitle(Guid.Parse(strId));
        }

        public async Task SaveMaterialChanges(IReadOnlyCollection<ChangeHistoryDto> changes, string materialTitle = null)
        {
            var entities = _mapper.Map<List<ChangeHistoryEntity>>(changes);
            var mirrorEntities = new List<ChangeHistoryEntity>();

            foreach (var entity in entities)
            {
                entity.Id = Guid.NewGuid();
                entity.Type = ChangeHistoryEntityType.Material;

                if (entity.PropertyName == "MaterialFeature.NodeId")
                {
                    entity.OldTitle = GetTitle(entity.OldValue);
                    entity.NewTitle = GetTitle(entity.NewValue);
                    mirrorEntities.Add(GetMirrorChangeHistoryEntity(entity, materialTitle));
                }
            }
            entities.AddRange(mirrorEntities);
            await RunAsync(uow => uow.ChangeHistoryRepository.AddRange(entities));
        }

        private ChangeHistoryEntity GetMirrorChangeHistoryEntity(ChangeHistoryEntity entity, string materialTitle)
        {
            return new ChangeHistoryEntity
            {
                Id = Guid.NewGuid(),
                TargetId = Guid.Parse(entity.OldValue ?? entity.NewValue),
                UserName = entity.UserName,
                PropertyName = "MaterialLink",
                Date = entity.Date,
                OldValue = entity.OldValue == null ? null : entity.TargetId.ToString("N"),
                NewValue = entity.NewValue == null ? null : entity.TargetId.ToString("N"),
                RequestId = entity.RequestId,
                Type = ChangeHistoryEntityType.Node,
                ParentTypeName = null,
                OldTitle = entity.OldValue == null ? null : materialTitle,
                NewTitle = entity.NewValue == null ? null : materialTitle
            };
        }

        public async Task<List<ChangeHistoryDto>> GetChangeHistoryAsync(ChangeHistoryParams parameters)
        {
            var entityList = await RunWithoutCommitAsync(uow =>
                uow.ChangeHistoryRepository.GetManyAsync(
                    parameters.EntityIdentityList.ToArray(),
                    parameters.PropertyName,
                    parameters.DateFrom,
                    parameters.DateTo)
            );

            var dtoList = _mapper.Map<List<ChangeHistoryDto>>(entityList);

            if (parameters.ApplyAliases) 
            {
                var nodeList = _ontologyNodesData.GetNodes(parameters.EntityIdentityList);

                if (nodeList.Count == 0) return dtoList;

                foreach (var node in nodeList)
                {
                    foreach (var dto in dtoList)
                    {
                        var alias = _ontologyNodesData.Schema.GetAlias($"{node.NodeType.Name}.{dto.PropertyName}");
                        dto.PropertyName = alias ?? dto.PropertyName;
                    }
                }
            }

            return dtoList;
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

        public async Task<IReadOnlyCollection<ChangeHistoryDto>> GetLocationHistory(Guid entityId)
        {
            var locations = await RunWithoutCommitAsync(uow => uow.FlightRadarRepository.GetLocationHistoryAsync(entityId));

            return locations.Select(e => LocationHistoryToDTO(e)).ToArray();
        }

        public async Task<IReadOnlyCollection<ChangeHistoryDto>> GetLocationHistoryAsync(ChangeHistoryParams parameters)
        {
            var locations = await RunWithoutCommitAsync(uow => uow.FlightRadarRepository.GetLocationHistoryAsync(parameters.EntityIdentityList.ToArray(), parameters.DateFrom, parameters.DateTo));

            var propertyNameDict = parameters.EntityIdentityList
                .Select(id => (Id:id, PropertyName: GetSignPropertyName(id)))
                .ToDictionary(kv => kv.Id, kv => kv.PropertyName);

            return locations
                .Select(e => LocationHistoryToDTO(e, propertyNameDict[e.EntityId.Value]))
                .ToArray();
        }

        private static ChangeHistoryDto LocationHistoryToDTO(LocationHistoryEntity entity, string propertyName = DefaultSignLocationPropName)
        {
            if(entity is null) return null;

            return new ChangeHistoryDto
            {
                Date = entity.RegisteredAt,
                NewValue = GetPointValue(entity.Lat, entity.Long),
                PropertyName = propertyName,
                TargetId = entity.EntityId.Value,
                Type = 0
            };
        }
        private string GetSignPropertyName(Guid entityId, string defaultPropertyName = DefaultSignLocationPropName)
        {
            var node = _ontologyNodesData.GetNode(entityId);

            if(node is null || !node.NodeType.IsObjectSign) return defaultPropertyName;

            var nodeValue = node.GetSingleProperty("value")?.Value;

            var propertyValue = string.IsNullOrWhiteSpace(nodeValue)
                ? string.Empty
                : $"[{nodeValue}]";

            return $"{node.NodeType.Title}{propertyValue}";
        }

        private static string GetPointValue(decimal latitude, decimal longitude)
        {
            return "{\"type\":\"Point\",\"coordinates\":[" + latitude.ToString() + "," + longitude.ToString() + "]}";
        }
    }
}
