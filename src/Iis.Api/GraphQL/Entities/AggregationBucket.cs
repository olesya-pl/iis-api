namespace IIS.Core.GraphQL.Entities
{
    public class AggregationBucket
    {
        public string Key { get; set; }
        public string TypeName { get; set; }
        public int DocCount { get; set; }
    }
}
