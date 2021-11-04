using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Iis.Interfaces.Elastic;
using Iis.Utility;
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
        private const string GroupedAggregationName = "GroupedAggregation";
        private const string GroupedAggregationNameRegex = "GroupedAggregation\\S{32}$";

        public static bool IsExactQuery(string query)
        {
            return query.Contains(":", StringComparison.Ordinal)
                   || query.Contains(" AND ", StringComparison.Ordinal)
                   || query.Contains(" OR ", StringComparison.Ordinal);
        }

        public static bool IsMatchAll(string query) =>
            string.IsNullOrWhiteSpace(query) || query.Trim().Equals(Wildcard);

        public static JObject WithSearchJson(IEnumerable<string> resultFieldList, int from, int size)
        {
            if (resultFieldList is null || !resultFieldList.Any()) resultFieldList = new[] { "*" };

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

            var queryGroups = aggregationFields
                .Where(_ => !string.IsNullOrWhiteSpace(_.TermFieldName) && !string.IsNullOrWhiteSpace(_.Name))
                .Select(_ => new AggregationFieldContext(_, filter))
                .GroupBy(_ => _.Query)
                .ToDictionary(_ => _.Key, _ => _.ToArray());
            foreach (var (query, contexts) in queryGroups)
                aggregations.ProcessQueryGroup(query, contexts);

            return jsonQuery;
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
            if (filter.CherryPickedItems.Count == 0 && filter.FilteredItems.Count == 0)
            {
                return filter.Suggestion;
            }
            return ToQueryStringWithFilterItems(filter);
        }
        public static string ToQueryStringWithForcedEscape(this ElasticFilter filter)
        {
            if (filter.CherryPickedItems.Count == 0 && filter.FilteredItems.Count == 0)
            {
                return filter.Suggestion
                    .RemoveSymbols(ElasticManager.RemoveSymbolsPattern)
                    .EscapeSymbols(ElasticManager.EscapeSymbolsPattern);
            }
            return ToQueryStringWithFilterItems(filter);
        }

        public static string CreateMaterialsQueryString(string suggestion,
            IReadOnlyCollection<Property> filteredItems,
            IReadOnlyCollection<CherryPickedItem> cherryPickedItems)
        {
            var noSuggestion = string.IsNullOrEmpty(suggestion);

            var queryString = noSuggestion ? "(ParentId:NULL)" : $"(({suggestion}) AND ParentId:NULL)";

            if (cherryPickedItems.Count == 0 && filteredItems.Count == 0)
            {
                return queryString;
            }

            queryString = PopulateFilteredItems(filteredItems, queryString);
            return PopulateCherryPickedIds(cherryPickedItems, queryString);
        }

        public static string ApplyFuzzinessOperator(string input)
        {
            input = input.RemoveSymbols(ElasticManager.RemoveSymbolsPattern)
                        .EscapeSymbols(ElasticManager.EscapeSymbolsPattern);

            if (IsWildCard(input) || IsInBrackets(input))
            {
                return input;
            }

            if (IsDoubleQuoted(input))
            {
                return $"{input} OR {input}~";
            }

            return $"\"{input}\" OR {input}~";
        }

        public static bool IsGroupedAggregateName(this string name) => Regex.IsMatch(name, GroupedAggregationNameRegex);

        private static void ProcessQueryGroup(
            this JObject aggregations,
            string query,
            IReadOnlyCollection<AggregationFieldContext> contexts)
        {
            if (contexts.Count == 1)
            {
                aggregations.ProcessQueryContext(contexts.First());
                return;
            }

            aggregations.ProcessQueryContexts(query, contexts);
        }

        private static void ProcessQueryContexts(
            this JObject aggregations,
            string query,
            IReadOnlyCollection<AggregationFieldContext> contexts)
        {
            var fieldName = GetUniqueAggregationName();
            var filterSection = CreateAggregationFilter(query);
            var subAggsSection = new JObject();
            foreach (var context in contexts)
                context.PopulateAggregation(subAggsSection);

            aggregations[fieldName] = new JObject
            {
                {"filter", filterSection},
                {"aggs", subAggsSection}
            };
        }

        private static void ProcessQueryContext(
            this JObject aggregations,
            AggregationFieldContext context)
        {
            var filterSection = CreateAggregationFilter(context.Query);
            var subAggsSection = CreateSubAggs("sub_aggs", context.Field);

            aggregations[context.FieldName] = new JObject
            {
                {"filter", filterSection},
                {"aggs", subAggsSection}
            };
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

        private static void PopulateAggregation(this AggregationFieldContext context, JObject section)
        {
            var aggregation = new JObject
            (
                new JProperty("field", context.Field.TermFieldName),
                new JProperty("missing", MissingValueKey),
                new JProperty("size", MaxBucketsCount)
            );

            section[context.FieldName] = new JObject
            {
                {"terms", aggregation}
            };
        }

        private static JObject CreateAggregationFilter(string query)
        {
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

        private static string PopulateFilteredItems(IReadOnlyCollection<Property> filter, string result)
        {
            var filteredItems = filter.GroupBy(x => x.Name, x => x.Value);
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

            return result;
        }

        private static string PopulateCherryPickedObjectsOfStudy(IList<CherryPickedItem> cherryPickedItems, string result)
        {
            var pickedQuery = new StringBuilder();
            for (var i = 0; i < cherryPickedItems.Count; i++)
            {
                var item = cherryPickedItems[i];
                var lastOne = i + 1 == cherryPickedItems.Count;
                if (item.IncludeDescendants)
                {
                    pickedQuery.Append($"Id:{item.Item} OR ");
                    pickedQuery.Append($"parent.Id:{item.Item}~0.95 OR ");
                    pickedQuery.Append(lastOne ? $"bePartOf.Id:{item.Item}~0.95" : $"bePartOf.Id:{item.Item}~0.95 OR ");
                }
                else
                {
                    pickedQuery.Append($"Id:{item.Item}");
                }
                if (lastOne)
                {
                    result = string.IsNullOrEmpty(result) ? $"({pickedQuery})" : $"({result} OR ({pickedQuery}))";
                }
            }

            return result;
        }

        private static string ConvertToHypensFormat(string strId) =>
            Guid.TryParse(strId, out Guid id) ? id.ToString() : strId;

        private static string PopulateCherryPickedIds(IReadOnlyCollection<CherryPickedItem> cherryPickedItems, string result)
        {
            if (cherryPickedItems.Count == 0) return result;

            var pickedQuery = new StringBuilder();
            
            for (var i = 0; i < cherryPickedItems.Count; i++)
            {
                var item = cherryPickedItems.ElementAt(i).Item;
                pickedQuery.Append($"\"{ConvertToHypensFormat(item)}\"");
                if (i + 1 < cherryPickedItems.Count) pickedQuery.Append(" OR ");
            }

            return string.IsNullOrEmpty(result) ? $"({pickedQuery})" : $"({result} OR ({pickedQuery}))";
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
                new JProperty(sortColumn, new JObject {
                    new JProperty("order", sortOrder),
                    new JProperty("missing", GetMissingValueSorting(sortOrder))
                })
            );
        }

        private static string GetMissingValueSorting(string sortOrder)
        {
            return sortOrder == "asc" ? "_last" : "_first";
        }

        private static string ToQueryString(this ElasticFilter filter, AggregationField field)
        {
            var fieldSpecificFilter = new ElasticFilter
            {
                Suggestion = filter.Suggestion,
                FilteredItems = filter.FilteredItems.Where(x => !(x.Name == field.Name || x.Name == field.Alias)).ToList()
            };
            var possibleQuery = fieldSpecificFilter.ToQueryStringWithForcedEscape();

            return string.IsNullOrEmpty(possibleQuery)
                ? Wildcard
                : possibleQuery;
        }

        private static string GetFieldName(this AggregationField field)
        {
            return !string.IsNullOrWhiteSpace(field.Alias) ? field.Alias : field.Name;
        }

        private static bool IsWildCard(string input) => input.Contains('*');

        private static bool IsDoubleQuoted(string input) => input.StartsWith('\"') && input.EndsWith('\"');

        private static bool IsInBrackets(string input) => input.StartsWith('(') && input.EndsWith(')');

        private static string GetUniqueAggregationName() => $"{GroupedAggregationName}{Guid.NewGuid().ToString("N")}";

        private static string ToQueryStringWithFilterItems(this ElasticFilter filter)
        {
            var result = string.IsNullOrEmpty(filter.Suggestion) ? "" : $"({ApplyFuzzinessOperator(filter.Suggestion)})";
            result = PopulateFilteredItems(filter.FilteredItems, result);
            return PopulateCherryPickedObjectsOfStudy(filter.CherryPickedItems, result);
        }

        private class AggregationFieldContext
        {
            public AggregationFieldContext(AggregationField aggregationField, ElasticFilter filter)
            {
                FieldName = aggregationField.GetFieldName();
                Field = aggregationField;
                Query = filter.ToQueryString(aggregationField);
            }

            public string FieldName { get; }
            public AggregationField Field { get; }
            public string Query { get; }
        }
    }
}