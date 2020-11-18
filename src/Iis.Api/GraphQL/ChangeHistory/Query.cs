using AutoMapper;
using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Ontology;
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
            [GraphQLType(typeof(NonNullType<IdType>))] Guid targetId,
            string propertyName = "",
            DateRangeFilter dateRangeFilter = null)
        {
            DateTime? dateFrom = null;
            DateTime? dateTo = null;

            if(dateRangeFilter != null)
            {
                (dateFrom, dateTo) = dateRangeFilter.ToRange();
            }

            var items = await service.GetChangeHistory(targetId, propertyName, dateFrom, dateTo);
            var graphQLItems = items.Select(item => mapper.Map<ChangeHistoryItem>(item))
                .GroupBy(p => p.RequestId)
                .Select(p => new ChangeHistoryItemGroup()
                {
                    RequestId = p.Key,
                    Items = p.ToList()
                })
                .ToList();

            return new GraphQLCollection<ChangeHistoryItemGroup>(graphQLItems, graphQLItems.Count);
        }
    }
}
