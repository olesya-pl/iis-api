using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions.CompositeBuilders.BoolQuery
{
    public class ExactQueryConditionBuilder : BaseQueryConditionBuilder<ExactQueryConditionBuilder>
    {
        private string _query = string.Empty;
        private bool? _isLenient;

        protected override ExactQueryConditionBuilder BuilderInstance => this;

        public ExactQueryConditionBuilder WithQuery(string query)
        {
            _query = query;
            return this;
        }

        public ExactQueryConditionBuilder WithLeniency(bool lenient)
        {
            _isLenient = lenient;
            return this;
        }

        public override JObject Build()
        {
            if (string.IsNullOrWhiteSpace(_query)) return null;

            var queryStringProperty = CreateJObject(QueryTerms.Conditions.Query, _query);
            if (_isLenient.HasValue)
            {
                queryStringProperty.Add(QueryTerms.Common.Lenient, _isLenient.Value);
            }

            return CreateJObject(QueryTerms.Conditions.QueryString, queryStringProperty);
        }
    }
}