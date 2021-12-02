using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using HotChocolate;
using IIS.Core.GraphQL.Common;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;
using System;
using System.Linq;
using Iis.Interfaces.Ontology.Data;
using IIS.Services.Contracts.Interfaces;
using Iis.Domain.Materials;
using Iis.Utility;
using Newtonsoft.Json.Linq;

namespace Iis.Api.GraphQL.RadioElectronicSituation
{
    public class ResQuery
    {
        private const string ValuePropertyName = "value";
        private const string SidcPropertyName = "affiliation.sidc";
        private const string BePartOfName = "bePartOf";
        private const string AmountName = "amount.code";
        private const string NoValueFound = "значення відсутне";
        private IMapper _mapper;
        public async Task<ResResult> GetRadioElectronicSituation(
            [Service] IRadioElectronicSituationService service,
            [Service] IMaterialProvider materialProvider,
            [Service] IMapper mapper
        )
        {
            _mapper = mapper;
            var locationItems = await service.GetSituationNodesAsync();
            var nodeIds = locationItems
                .Where(_ => _.ObjectNode != null)
                .Select(_ => _.ObjectNode.Id)
                .ToList();

            var callItems = await materialProvider.GetCallInfoAsync(nodeIds);
            var result = MapToResResult(locationItems, callItems);
            return result;
        }

        private ResResult MapToResResult(
            IReadOnlyCollection<ResSourceItemDto> sourceItems, 
            IReadOnlyCollection<ResCallerReceiverDto> callItems)
        {
            var groupedItems = sourceItems.GroupBy(_ => _.Sign.Id).ToArray();

            var result = new ResResult
            {
                Nodes = groupedItems.Select(_ => GetResNode(_)).ToList(),
                Links = callItems.Select(_ => GetResLink(_)).ToList()
            };

            return result;
        }

        private ResNode GetResNode(IEnumerable<ResSourceItemDto> items)
        {
            var firstItem = items.First();
            var result = new ResNode
            {
                Id = firstItem.Sign.Id,
                Extra = new ResNodeExtra
                {
                    Lattitude = firstItem.LocationHistory.Lat,
                    Longitude = firstItem.LocationHistory.Long,
                    RegisteredAt = firstItem.LocationHistory.RegisteredAt,
                    Sign = new ResNodeExtraSign
                    {
                        Value = firstItem.Sign.GetSingleDirectProperty(ValuePropertyName)?.Value ?? NoValueFound 
                    },
                    Objects = GetNodesExtraObjects(items),
                    Materials = GetNodesExtraMaterials(items)
                }
            };
            return result;
        }

        private IReadOnlyList<ResNodeExtraObject> GetNodesExtraObjects(IEnumerable<ResSourceItemDto> sourceItems)
        {
            return sourceItems
                .Where(_ => _.ObjectNode != null)
                .Select(_ => GetNodesExtraObject(_.ObjectNode)).ToList();
        }

        private ResNodeExtraObject GetNodesExtraObject(INode sourceItem)
        {
            return new ResNodeExtraObject
            {
                Id = sourceItem.Id,
                Type = sourceItem.NodeType.Name,
                Title = sourceItem.GetTitleValue() ?? NoValueFound,
                Affiliation = new ResNodeExtraObjectAffiliation
                {
                    Sidc = sourceItem.GetSingleProperty(SidcPropertyName)?.Value ?? NoValueFound
                },
                Amount = new ResNodeExtraObjectAmount
                {
                    Code = sourceItem.GetSingleProperty(AmountName)?.Value ?? NoValueFound
                },
                BePartOf = new ResNodeExtraObjectBePartOf
                {
                    Title = sourceItem.GetSingleProperty(BePartOfName)?.GetTitleValue() ?? NoValueFound
                }
            };
        }

        private ResLink GetResLink(ResCallerReceiverDto callerReceiverDto)
        {
            return new ResLink
            {
                Id = Guid.NewGuid(),
                From = callerReceiverDto.CallerId,
                To = callerReceiverDto.ReceiverId,
                Extra = new ResLinkExtra
                {
                    Type = "call",
                    Count = callerReceiverDto.Count
                }
            };
        }
        private IReadOnlyList<ResMaterial> GetNodesExtraMaterials(IEnumerable<ResSourceItemDto> sourceItems)
        {
            return sourceItems
                .Where(_ => _.Material != null)
                .Select(_ => GetNodesExtraMaterial(_.Material)).ToList();
        }
        private ResMaterial GetNodesExtraMaterial(Material sourceItem)
        {
            var result = _mapper.Map<ResMaterial>(sourceItem);
            result.File = new ResMaterialFile
            {
                Id = sourceItem.Id,
                Url = FileUrlGetter.GetFileUrl(sourceItem.Id)
            };
            result.Metadata = new ResMaterialMetadata
            {
                Type = sourceItem.Metadata.GetValue("type", StringComparison.InvariantCultureIgnoreCase)?.Value<string>(),
                Source = sourceItem.Metadata.GetValue("source", StringComparison.InvariantCultureIgnoreCase)?.Value<string>(),
            };
            return result;
        }
    }
}