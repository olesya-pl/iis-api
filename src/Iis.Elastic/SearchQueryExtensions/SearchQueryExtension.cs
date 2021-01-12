﻿using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public static class SearchQueryExtension
    {
        private const string Wildcard = "*";
        private const int MaxBucketsCount = 100;
        public const string AggregateSuffix = "Aggregate";
        public const string MissingValueKey = "__hasNoValue";

        public static bool IsExactQuery(string query)
        {
            return query.Contains(":", System.StringComparison.Ordinal)
                || query.Contains(" AND ", System.StringComparison.Ordinal)
                || query.Contains(" OR ", System.StringComparison.Ordinal)
                || query.Contains("\"", System.StringComparison.Ordinal);
        }

        public static bool IsMatchAll(string query) => string.IsNullOrWhiteSpace(query) || query.Trim().Equals(Wildcard);

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
            if(jsonQuery is null) return jsonQuery;

            if(string.IsNullOrWhiteSpace(sortColumn) || string.IsNullOrWhiteSpace(sortOrder)) return jsonQuery;

            if (!jsonQuery.ContainsKey("sort")) jsonQuery["sort"] = new JArray();

            if(jsonQuery["sort"].GetType() != typeof(JArray)) return jsonQuery;

            (jsonQuery["sort"] as JArray).Add(CreateSortingProperty(sortColumn, sortOrder));

            return jsonQuery;
        }

        public static JObject WithAggregation(this JObject jsonQuery, IReadOnlyCollection<string> aggregationFieldNameList)
        {
            if(!aggregationFieldNameList.Any()) return jsonQuery;

            var maps = aggregationFieldNameList
                .Select(p => new AggregateFieldMap(p, $"{p}{AggregateSuffix}"))
                .ToList();
            return jsonQuery.WithExactAggregationNames(maps);            
        }

        public static JObject WithExactAggregationNames(this JObject jsonQuery, 
            IReadOnlyCollection<AggregateFieldMap> aggregationFieldMaps)
        {
            if (!aggregationFieldMaps.Any()) return jsonQuery;

            var aggregations = new JObject();

            jsonQuery["aggs"] = aggregations;

            foreach (var fieldMap in aggregationFieldMaps)
            {
                var field = new JObject
                (
                    new JProperty("field", fieldMap.FieldName),
                    new JProperty("missing", MissingValueKey),
                    new JProperty("size", MaxBucketsCount)
                );
                aggregations[fieldMap.Alias] = new JObject
                (
                    new JProperty("terms", field)
                );
            }

            return jsonQuery;
        }

        private static JObject CreateSortingProperty(string sortColumn, string sortOrder)
        {
            return new JObject(
                new JProperty(sortColumn, new JObject() { new JProperty("order", sortOrder)})
            );
        }
    }

    public class AggregateFieldMap
    {
        public string Alias { get; private set; }
        public string FieldName { get; private set; }

        public AggregateFieldMap(string alias, string fieldName)
        {
            Alias = alias;
            FieldName = fieldName;
        }
    }
}