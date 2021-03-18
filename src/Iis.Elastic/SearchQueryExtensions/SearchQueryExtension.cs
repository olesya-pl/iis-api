using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public static class SearchQueryExtension
    {
        private const string Wildcard = "*";
        private const int MaxBucketsCount = 100;
        public const string AggregateSuffix = "Aggregate";
        public const string MissingValueKey = "__hasNoValue";
        public const string NoExistsValue = "-_exists_";

        public static bool IsExactQuery(string query)
        {
            return query.Contains(":", StringComparison.Ordinal)
                   || query.Contains(" AND ", StringComparison.Ordinal)
                   || query.Contains(" OR ", StringComparison.Ordinal)
                   || query.Contains("\"", StringComparison.Ordinal);
        }

        public static bool IsMatchAll(string query) =>
            string.IsNullOrWhiteSpace(query) || query.Trim().Equals(Wildcard);

        public static JObject WithSearchJson(IEnumerable<string> resultFieldList, int from, int size)
        {
            if (resultFieldList is null || !resultFieldList.Any()) resultFieldList = new[] {"*"};

            return new JObject(
                new JProperty("_source", new JArray(resultFieldList)),
                new JProperty("from", from),
                new JProperty("size", size),
                new JProperty("query", new JObject())
            );
        }

        public static JObject WithHighlights(this JObject jsonQuery)
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

        public static JObject SetupSorting(this JObject jsonQuery, string sortColumn, string sortOrder)
        {
            if (jsonQuery is null) return jsonQuery;

            if (string.IsNullOrWhiteSpace(sortColumn) || string.IsNullOrWhiteSpace(sortOrder)) return jsonQuery;

            if (!jsonQuery.ContainsKey("sort")) jsonQuery["sort"] = new JArray();

            if (jsonQuery["sort"].GetType() != typeof(JArray)) return jsonQuery;

            (jsonQuery["sort"] as JArray).Add(CreateSortingProperty(sortColumn, sortOrder));

            return jsonQuery;
        }

        public static JObject WithAggregation(this JObject jsonQuery,
            IEnumerable<AggregationField> aggregationFields,
            ElasticFilter filter)
        {
            if (!aggregationFields.Any()) return jsonQuery;

            var aggregations = new JObject();

            jsonQuery["aggs"] = aggregations;

            foreach (var field in aggregationFields)
            {
                if (string.IsNullOrWhiteSpace(field.TermFieldName) || string.IsNullOrWhiteSpace(field.Name)) continue;

                var fieldName = GetFieldName(field);
                var filterSection = CreateAggregationFilter(field, filter);
                var subAggsSection = CreateSubAggs("sub_aggs", field);

                aggregations[fieldName] = new JObject
                {
                    {"filter", filterSection},
                    {"aggs", subAggsSection}
                };
            }

            return jsonQuery;
        }

        private static JObject CreateSubAggs(string name, AggregationField field)
        {
            var aggregation = new JObject
            (
                new JProperty("field", field.TermFieldName),
                new JProperty("missing", MissingValueKey),
                new JProperty("size", MaxBucketsCount)
            );

            return new JObject
            {
                {name, new JObject {{"terms", aggregation}}}
            };
        }

        private static JObject CreateAggregationFilter(AggregationField field, ElasticFilter filter)
        {
            var fieldSpecificFilter = new ElasticFilter
            {
                Suggestion = filter.Suggestion,
                FilteredItems = filter.FilteredItems.Where(x => !(x.Name == field.OriginFieldName || x.Name == field.Alias)).ToList()
            };

            var possibleQuery = fieldSpecificFilter.ToQueryString();
            var query = string.IsNullOrEmpty(possibleQuery) ? Wildcard : possibleQuery;

            var boolSection = JObject.FromObject(new
            {
                should = new[]
                {
                    new
                    {
                        query_string = new
                        {
                            query
                        }
                    }
                }
            });

            return new JObject
            {
                {"bool", boolSection}
            };
        }

        private static string GetFieldName(AggregationField field)
        {
            return !string.IsNullOrWhiteSpace(field.Alias) ? field.Alias : field.Name;
        }

        public static JObject WithAggregation(this JObject jsonQuery,
            IReadOnlyCollection<AggregationField> aggregationFieldCollection)
        {
            if (!aggregationFieldCollection.Any()) return jsonQuery;

            var aggregations = new JObject();

            jsonQuery["aggs"] = aggregations;

            foreach (var field in aggregationFieldCollection)
            {
                if (string.IsNullOrWhiteSpace(field.TermFieldName) || string.IsNullOrWhiteSpace(field.Name)) continue;

                var aggregation = new JObject
                (
                    new JProperty("field", field.TermFieldName),
                    new JProperty("missing", MissingValueKey),
                    new JProperty("size", MaxBucketsCount)
                );

                var aggregationName = !string.IsNullOrWhiteSpace(field.Alias) ? field.Alias : field.Name;

                aggregations[aggregationName] = new JObject
                (
                    new JProperty("terms", aggregation)
                );
            }

            return jsonQuery;
        }

        public static string ToQueryString(this ElasticFilter filter)
        {
            var result = string.IsNullOrEmpty(filter.Suggestion) ? "" : $"({filter.Suggestion})";

            var filteredItems = filter.FilteredItems
                .GroupBy(x => x.Name, x => x.Value);
            var filteredQueries = new List<string>();
            foreach (var filteredItem in filteredItems)
            {
                var queryForOneField = string.Join(" OR ", filteredItem.Select(x => GetFieldQuery(filteredItem.Key, x)).ToArray());
                if (queryForOneField.Length > 0)
                    filteredQueries.Add($"({queryForOneField})");
            }

            if (filteredQueries.Any())
            {
                var generalFilteredQuery = string.Join(" AND ", filteredQueries);
                result = string.IsNullOrEmpty(result) ? $"({generalFilteredQuery})" : $"({result} AND ({generalFilteredQuery}))";
            }   

            var pickedQuery = new StringBuilder();
            for (var i = 0; i < filter.CherryPickedItems.Count; i++)
            {
                var item = filter.CherryPickedItems[i];
                var lastOne = i + 1 == filter.CherryPickedItems.Count;
                pickedQuery.Append($"Id:{item} OR ");
                pickedQuery.Append($"parent.Id:{item}~0.95 OR ");
                pickedQuery.Append(lastOne ? $"bePartOf.Id:{item}~0.95" : $"bePartOf.Id:{item}~0.95 OR ");
                if (lastOne)
                {
                    result = string.IsNullOrEmpty(result) ? $"({pickedQuery})" : $"({result} OR ({pickedQuery}))";
                }
            }

            return result;
        }

        private static string GetFieldQuery(string field, string value)
        {
            return value == MissingValueKey ? 
                $"{NoExistsValue}:\"{field}\"" :
                $"{field}:\"{value}\"";
        }

        private static JObject CreateSortingProperty(string sortColumn, string sortOrder)
        {
            return new JObject(
                new JProperty(sortColumn, new JObject {new JProperty("order", sortOrder)})
            );
        }
    }
}