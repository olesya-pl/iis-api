using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Domain.Vocabularies;
using Iis.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using IIS.Core.GraphQL.Common;
using System;
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

            if(dateRangeFilter != null)
            {
                (dateFrom, dateTo) = dateRangeFilter.ToRange();
            }

            var items = await service.GetChangeHistory(new ChangeHistoryParams
            {
                DateFrom = dateFrom,
                DateTo = dateTo,
                PropertyName = propertyName,
                TargetId = targetId,
                ApplyAliases = true
            });
            
            if (!includeLocationHistory.HasValue || includeLocationHistory == true)
            {
                var locationItems = await service.GetLocationHistory(targetId);
                items.AddRange(locationItems);
            }

            foreach (var item in items)
            {
                item.PropertyName = vocabulary.Translate(item.PropertyName);
            }

            var graphQLItems = items.Select(item => mapper.Map<ChangeHistoryItem>(item))
                .GroupBy(p => p.RequestId)
                .Select(p => new ChangeHistoryItemGroup()
                {
                    RequestId = p.Key,
                    Items = p.OrderBy(item => item.PropertyName == "lastConfirmedAt" ? 1 : 0).ToList()
                })
                .ToList();

            return new GraphQLCollection<ChangeHistoryItemGroup>(graphQLItems, graphQLItems.Count);
        }
    }
}
