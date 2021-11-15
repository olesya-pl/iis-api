using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions.CompositeBuilders.BoolQuery
{
    public class MatchQueryConditionBuilder : BaseQueryConditionBuilder<MatchQueryConditionBuilder>
    {
        private string _fieldName;
        private string _fieldValue;

        protected override MatchQueryConditionBuilder BuilderInstance => this;

        public MatchQueryConditionBuilder Match(string name, string value)
        {
            _fieldName = name;
            _fieldValue = value;
            return this;
        }

        public override JObject Build()
        {
            if (string.IsNullOrWhiteSpace(_fieldName)) return null;

            var fieldProperty = CreateJObject(_fieldName, _fieldValue);

            return CreateJObject(QueryTerms.Conditions.Match, fieldProperty);
        }
    }
}