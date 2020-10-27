using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public static class SearchQueryExtension
    {
        public static JObject WithSearchJson(IEnumerable<string> resultFieldList, int from, int size)
        {
            if(resultFieldList is null || !resultFieldList.Any()) resultFieldList = new [] {"*"};

            return new JObject(
                new JProperty("_source", new JArray(resultFieldList)),
                new JProperty("from", from),
                new JProperty("size", size),
                new JProperty("query", new JObject())
            );
        }

        public static JObject SetupHighlights(this JObject jsonQuery)
        {
            if (jsonQuery is null) return jsonQuery;

            if (!jsonQuery.ContainsKey("highlight")) jsonQuery["highlight"] = new JObject();

            var allProperty = new JProperty("*", JObject.Parse("{\"type\" : \"unified\"}"));

            var fieldsProperty = new JObject(
                new JProperty("fields", new JObject(allProperty))
            );

            jsonQuery["highlight"] = fieldsProperty;

            return jsonQuery;
        }

        public static bool IsExactQuery(string query)
        {
            return query.Contains(":", System.StringComparison.Ordinal)
                || query.Contains(" AND ", System.StringComparison.Ordinal)
                || query.Contains(" OR ", System.StringComparison.Ordinal)
                || query.Contains("\"", System.StringComparison.Ordinal);
        }

        public static JObject SetupExactQuery(this JObject jsonQuery, string query, bool? lenient = null)
        {
            if (jsonQuery is null) return jsonQuery;

            if (!jsonQuery.ContainsKey("query")) jsonQuery["query"] = new JObject();

            var queryStringProperty = new JObject(new JProperty("query", query));

            if(lenient.HasValue) queryStringProperty.Add("lenient", lenient.Value);

            var queryString = new JObject(
                new JProperty("query_string", queryStringProperty)
            );

            jsonQuery["query"] = queryString;

            return jsonQuery;
        }

        public static JObject SetupSorting(this JObject jsonQuery, string sortColumn, string sortOrder)
        {
            if(jsonQuery is null) return jsonQuery;

            if(string.IsNullOrWhiteSpace(sortColumn) || string.IsNullOrWhiteSpace(sortOrder)) return jsonQuery;

            if (!jsonQuery.ContainsKey("sort")) jsonQuery["sort"] = new JArray();

            if(jsonQuery["sort"].GetType() != typeof(JArray)) return jsonQuery;

            (jsonQuery["sort"] as JArray).Add(CreateSortingProperty(sortColumn, sortOrder));

            return jsonQuery;
        }

        private static JObject CreateSortingProperty(string sortColumn, string sortOrder)
        {
            return new JObject(
                new JProperty(sortColumn, new JObject() { new JProperty("order", sortOrder)})
            );
        }
    }
}