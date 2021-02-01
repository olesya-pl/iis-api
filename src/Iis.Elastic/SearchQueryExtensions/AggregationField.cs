namespace Iis.Elastic.SearchQueryExtensions
{
    public class AggregationField
    {
        public string Alias { get; private set; }
        public string Name {get; private set;}
        public string TermFieldName { get; private set; }
        public AggregationField(string name, string alias, string termFieldName)
        {
            Alias = alias;
            Name = name;
            TermFieldName = termFieldName;
        }
    }
}