using Newtonsoft.Json.Linq;
using Iis.Elastic.Dictionaries;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class ExactQueryBuilder : PaginatedQueryBuilder<ExactQueryBuilder>
    {
        private string _query = string.Empty;
        private bool? IsLenient;

        public ExactQueryBuilder WithQueryString(string query)
        {
            _query = query;
            return this;
        }

        public ExactQueryBuilder WithLeniency(bool lenient)
        {
            IsLenient = lenient;
            return this;
        }

        protected override JObject CreateQuery(JObject jsonQuery)
        {
            var queryStringProperty = new JObject(new JProperty(SearchQueryPropertyName.Query, _query));
            if (IsLenient.HasValue) queryStringProperty.Add(SearchQueryPropertyName.Lenient, IsLenient.Value);
            var queryString = new JObject(
                new JProperty(SearchQueryPropertyName.QueryString, queryStringProperty)
            );
            jsonQuery[SearchQueryPropertyName.Query] = queryString;
            return jsonQuery;
        }
    }
}
