using Iis.Elastic.Dictionaries;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions.CompositeBuilders.BoolQuery
{
    public interface IQueryConditionBuilder
    {
        string Occur { get; }
        JObject Build();
    }

    public abstract class BaseQueryConditionBuilder<TConditionBuilder> : IQueryConditionBuilder
        where TConditionBuilder : BaseQueryConditionBuilder<TConditionBuilder>, new()
    {
        protected BaseQueryConditionBuilder()
        {
            Occur = QueryBooleanOccurs.Should;
        }

        public string Occur { get; private set; }
        protected abstract TConditionBuilder BuilderInstance { get; }

        public TConditionBuilder Must()
        {
            Occur = QueryBooleanOccurs.Must;
            return BuilderInstance;
        }

        public TConditionBuilder MustNot()
        {
            Occur = QueryBooleanOccurs.MustNot;
            return BuilderInstance;
        }

        public TConditionBuilder Should()
        {
            Occur = QueryBooleanOccurs.Should;
            return BuilderInstance;
        }

        public abstract JObject Build();

        protected JObject CreateJObject(string propertyName, object propertyValue)
        {
            var property = new JProperty(propertyName, propertyValue);

            return new JObject(property);
        }
    }
}