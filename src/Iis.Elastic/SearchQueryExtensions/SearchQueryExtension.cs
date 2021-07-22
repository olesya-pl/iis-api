﻿using System;
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
            JArray shouldSections)
        {
            if (!aggregationFields.Any()) return jsonQuery;

            var aggregations = new JObject();

            jsonQuery["aggs"] = aggregations;

            foreach (var field in aggregationFields)
            {
                if (string.IsNullOrWhiteSpace(field.TermFieldName) || string.IsNullOrWhiteSpace(field.Name)) continue;

                var fieldName = GetFieldName(field);
                var filterSection = CreateAggregationFilter(shouldSections);
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

        private static JObject CreateAggregationFilter(JArray shouldSections)
        {
            var boolSection = JObject.FromObject(new
            {
                should = shouldSections
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
            if (filter.CherryPickedItems.Count == 0 && filter.FilteredItems.Count == 0)
            {
                return filter.Suggestion;
            }

            var result = string.IsNullOrEmpty(filter.Suggestion) ? "" : $"({filter.Suggestion})";
            result = PopulateFilteredItems(filter.FilteredItems, result);
            return PopulateCherryPickedObjectsOfStudy(filter.CherryPickedItems, result);
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
            return PopulateCherryPickedMaterials(cherryPickedItems, queryString);
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

        private static string PopulateCherryPickedMaterials(IReadOnlyCollection<CherryPickedItem> cherryPickedItems, string result)
        {
            var pickedQuery = new StringBuilder();
            for (var i = 0; i < cherryPickedItems.Count; i++)
            {
                var item = cherryPickedItems.ElementAt(i);
                var lastOne = i + 1 == cherryPickedItems.Count;                
                pickedQuery.Append($"_id:{item.Item}");
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
                new JProperty(sortColumn, new JObject {
                    new JProperty("order", sortOrder),
                    new JProperty("missing", GetMissingValueSorting(sortOrder))
                })
            );
        }

        private static string GetMissingValueSorting(string sortOrder)
        {
            return sortOrder == "asc" ?  "_last" : "_first";
        }
    }
}