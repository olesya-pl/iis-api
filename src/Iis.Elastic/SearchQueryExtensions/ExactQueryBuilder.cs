﻿using Newtonsoft.Json.Linq;

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

        public override JObject Build()
        {
            var jsonQuery = SearchQueryExtension.WithSearchJson(_resultFields, _from, _size);

            jsonQuery["query"] = new JObject();

            return BuildExactQuery(jsonQuery);
        }
        

        public JObject BuildCountQuery()
        {
            var json = new JObject(new JProperty("query", new JObject()));
            return BuildExactQuery(json);
        }

        private JObject BuildExactQuery(JObject jsonQuery)
        {
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
