using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Domain.Vocabularies;
using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Core.GraphQL.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.GraphQL.ChangeHistory
{
    public class Query
    {
        public async Task<GraphQLCollection<ChangeHistoryItemGroup>> GetChangeHistory(
            [Service] IChangeHistoryService service,
            [Service] IMapper mapper,
            [Service] IisVocabulary vocabulary,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid targetId,
            string propertyName = "",
            DateRangeFilter dateRangeFilter = null,
            bool? includeLocationHistory = null)
        {
            DateTime? dateFrom = null;
            DateTime? dateTo = null;

            if (dateRangeFilter != null)
            {
                (dateFrom, dateTo) = dateRangeFilter.ToRange();
            }

            var changeHistoryParams = new ChangeHistoryParams
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                PropertyName = propertyName,
                TargetId = targetId,
                ApplyAliases = true
            };

            var items = await service.GetChangeHistory(changeHistoryParams);

            if (!includeLocationHistory.HasValue || includeLocationHistory == true)
            {
                var locationItems = await service.GetLocationHistory(changeHistoryParams);

                items.AddRange(locationItems);
            }

            TranslatePropertyNames(vocabulary, items);

            var graphQLItems = MapToGraphQlItems(mapper, items);

            return new GraphQLCollection<ChangeHistoryItemGroup>(graphQLItems, graphQLItems.Count);
        }

        private static List<ChangeHistoryItemGroup> MapToGraphQlItems(IMapper mapper, List<ChangeHistoryDto> items)
        {
            return items.Select(item => mapper.Map<ChangeHistoryItem>(item))
                            .GroupBy(p => p.RequestId)
                            .Select(p => new ChangeHistoryItemGroup()
                            {
                                RequestId = p.Key,
                                Items = p.OrderBy(item => item.PropertyName == "lastConfirmedAt" ? 1 : 0).ToList()
                            })
                            .ToList();
        }

        private static void TranslatePropertyNames(IisVocabulary vocabulary, List<ChangeHistoryDto> items)
        {
            foreach (var item in items)
            {
                item.PropertyName = vocabulary.Translate(item.PropertyName);
            }
        }

        public async Task<MapHistoryResponse> GetMapChanges(
            [Service] IOntologySchema ontology, 
            [Service] IOntologyNodesData nodesData,
            [Service] IChangeHistoryService service,
            [Service] IMapper mapper,
            [Service] IisVocabulary vocabulary,
            [GraphQLType(typeof(NonNullType<IdType>))] Guid targetId,
            DateRangeFilter dateRangeFilter = null)
        {
            DateTime? dateFrom = null;
            DateTime? dateTo = null;

            if (dateRangeFilter != null)
            {
                (dateFrom, dateTo) = dateRangeFilter.ToRange();
            }

            var changeHistoryParams = new ChangeHistoryParams
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                PropertyName = string.Empty,
                TargetId = targetId,
                ApplyAliases = true
            };

            var node = nodesData.GetNode(targetId);

            if (node == null)
            {
                throw new ArgumentNullException(nameof(targetId));
            }

            var items = await service.GetChangeHistory(changeHistoryParams);
            TranslatePropertyNames(vocabulary, items);

            var geoHistory = PrepareGeoHistory(ontology, node, items);
            var locationItems = await service.GetLocationHistory(changeHistoryParams);
            geoHistory.AddRange(locationItems);
            var graphQLItems = MapToGraphQlItems(mapper, geoHistory);

            var aggregates = PrepareAggregatedProperties(items);

            return new MapHistoryResponse
            {
                GeoItems = graphQLItems,
                MonthlyAggregates = aggregates
            };
        }

        private static List<ChangeHistoryDto> PrepareGeoHistory(IOntologySchema ontology, INode node, List<ChangeHistoryDto> items)
        {
            var attributes = ontology.GetAttributesInfo(node.NodeType.Name);
            var geoDotNames = attributes.Items
                .Where(p => p.ScalarType == Iis.Interfaces.Ontology.Schema.ScalarType.Geo)
                .Select(p => p.DotName)
                .ToList();

            var geoHistory = items.Where(p => geoDotNames.Contains(p.PropertyName)).ToList();
            return geoHistory;
        }

        private static List<HistoryItemMonthlyAggregate> PrepareAggregatedProperties(List<ChangeHistoryDto> items)
        {
            var grouppedProperties = items.GroupBy(p => p.PropertyName);
            var aggregates = new List<HistoryItemMonthlyAggregate>();
            foreach (var prop in grouppedProperties)
            {
                var grouppedByMonth = prop.GroupBy(p => p.Date.Month);
                foreach (var group in grouppedByMonth)
                {
                    aggregates.Add(new HistoryItemMonthlyAggregate
                    {
                        PropertyName = prop.Key,
                        Month = group.First().Date.Month,
                        Year = group.First().Date.Year,
                        Count = group.Count()
                    });
                }
            }

            return aggregates;
        }
    }
}
