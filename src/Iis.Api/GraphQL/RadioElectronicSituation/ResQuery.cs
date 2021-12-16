using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using HotChocolate;
using Newtonsoft.Json.Linq;
using IIS.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Dtos.RadioElectronicSituation;
using Iis.Interfaces.Ontology.Data;
using Iis.Domain.Materials;
using Iis.Utility;

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

        public ResQuery(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ResResult> GetRadioElectronicSituation(
            [Service] IRadioElectronicSituationService service,
            [Service] IMaterialProvider materialProvider)
        {
            var locationItems = await service.GetSituationNodesAsync();

            var signIds = locationItems
                .Where(_ => _.Sign != null)
                .Select(_ => _.Sign.Id)
                .Distinct()
                .ToArray();

            var callItems = await materialProvider.GetCallInfoAsync(signIds);

            var result = MapToResResult(locationItems, callItems);

            return result;
        }

        private static IReadOnlyList<ResNodeExtraObject> GetNodesExtraObjects(IEnumerable<ResSourceItemDto> sourceItems)
        {
            return sourceItems
                .Where(_ => _.ObjectNode != null)
                .Select(_ => GetNodesExtraObject(_.ObjectNode)).ToArray();
        }

        private static ResNodeExtraObject GetNodesExtraObject(INode sourceItem)
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
                    Title = sourceItem.GetSingleProperty(BePartOfName)?.GetTitleValue() //?? NoValueFound
                }
            };
        }

        private static ResLink GetResLink(ResCallerReceiverDto callerReceiverDto)
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

        private ResResult MapToResResult(
            IReadOnlyCollection<ResSourceItemDto> sourceItems,
            IReadOnlyCollection<ResCallerReceiverDto> callItems)
        {
            var groupedItems = sourceItems.GroupBy(_ => _.Sign.Id).ToArray();

            var result = new ResResult
            {
                Nodes = groupedItems.Select(_ => GetResNode(_)).ToArray(),
                Links = callItems.Select(_ => GetResLink(_)).ToArray()
            };

            return result;
        }

        private ResNode GetResNode(IEnumerable<ResSourceItemDto> items)
        {
            var firstItem = items.First();

            var result = new ResNode
            {
                Id = firstItem.Sign.Id,
                Latitude = firstItem.LocationHistory.Lat,
                Longitude = firstItem.LocationHistory.Long,
                Extra = new ResNodeExtra
                {
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

        private IReadOnlyList<ResMaterial> GetNodesExtraMaterials(IEnumerable<ResSourceItemDto> sourceItems)
        {
            return sourceItems
                .Where(_ => _.Material != null)
                .Select(_ => GetNodesExtraMaterial(_.Material)).ToArray();
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