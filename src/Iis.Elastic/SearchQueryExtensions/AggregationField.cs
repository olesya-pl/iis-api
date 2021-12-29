namespace Iis.Elastic.SearchQueryExtensions
{
    public class AggregationField
    {
        public AggregationField(string name, string alias, string termFieldName, string originFieldName = null)
        {
            Alias = alias;
            Name = name;
            TermFieldName = termFieldName;
            OriginFieldName = originFieldName;
        }
        public string OriginFieldName { get; private set; }
        public string Alias { get; private set; }
        public string Name { get; private set; }
        public string TermFieldName { get; private set; }
    }
}