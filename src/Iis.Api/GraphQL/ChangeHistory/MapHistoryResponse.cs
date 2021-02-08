using System.Collections.Generic;

namespace IIS.Core.GraphQL.ChangeHistory
{
    public class MapHistoryResponse
    {
        public List<ChangeHistoryItemGroup> GeoItems { get; set; }
        public List<HistoryItemMonthlyAggregate> MonthlyAggregates { get; set; }
    }
}