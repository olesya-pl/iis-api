using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions.CompositeBuilders.BoolQuery
{
    public class ExistsQueryConditionBuilder : BaseQueryConditionBuilder<ExistsQueryConditionBuilder>
    {
        private string _fieldName;

        protected override ExistsQueryConditionBuilder BuilderInstance => this;

        public ExistsQueryConditionBuilder Exist(string fieldName)
        {
            _fieldName = fieldName;
            return this;
        }

        public override JObject Build()
        {
            if (string.IsNullOrWhiteSpace(_fieldName)) return null;

            var fieldProperty = CreateJObject(QueryTerms.Common.Field, _fieldName);

            return CreateJObject(QueryTerms.Conditions.Exists, fieldProperty);
        }
    }
}