using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class ExactQueryBuilder
    {
        private IReadOnlyCollection<string> ResultFields = new[] { "*" };
        private int Offset;
        private int Limit;
        private string Query = string.Empty;
        private bool? IsLenient;

        public ExactQueryBuilder WithResultFields(IReadOnlyCollection<string> resultFields)
        {
            ResultFields = resultFields;
            return this;
        }

        public ExactQueryBuilder WithPagination(int offset, int limit)
        {
            Offset = offset;
            Limit = limit;
            return this;
        }

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
            var jsonQuery = SearchQueryExtension.WithSearchJson(ResultFields, Offset, Limit);

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
