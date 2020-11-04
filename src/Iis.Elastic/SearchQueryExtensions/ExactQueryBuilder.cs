using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class ExactQueryBuilder : BaseQueryBuilder<ExactQueryBuilder>
    {
        private string Query = string.Empty;
        private bool? IsLenient;

        public ExactQueryBuilder WithQueryString(string query)
        {
            Query = query;
            return this;
        }

        public ExactQueryBuilder WithLeniency(bool lenient)
        {
            IsLenient = lenient;
            return this;
        }

        public JObject Build()
        {
            var jsonQuery = SearchQueryExtension.WithSearchJson(_resultFields, _offset, _limit);

            jsonQuery["query"] = new JObject();

            var queryStringProperty = new JObject(new JProperty("query", Query));
            if (IsLenient.HasValue) queryStringProperty.Add("lenient", IsLenient.Value);
            var queryString = new JObject(
                new JProperty("query_string", queryStringProperty)
            );
            jsonQuery["query"] = queryString;
            return jsonQuery;
        } 
    }
}
