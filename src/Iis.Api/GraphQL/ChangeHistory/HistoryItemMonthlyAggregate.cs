namespace IIS.Core.GraphQL.ChangeHistory
{
    public class HistoryItemMonthlyAggregate
    {
        public string PropertyName { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Count { get; set; }
    }
}