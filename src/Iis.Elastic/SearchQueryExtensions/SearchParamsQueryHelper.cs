using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using Iis.Utility;

namespace Iis.Elastic.SearchQueryExtensions
{
    public static class SearchParamsQueryHelper
    {
        public static JArray AsQueries(IEnumerable<SearchParameter> searchParams, bool? isLenient)
        {
            var queries = new JArray();

            foreach (var searchItem in searchParams)
            {
                if (searchItem.IsExact)
                {
                    var shouldSection = CreateExactShouldSection(searchItem.Query, isLenient);
                    queries.Add(shouldSection);
                }
                else if (searchItem.Fields?.Any() == true)
                {
                    var shouldSection = CreateMultiFieldShouldSection(searchItem.Query, searchItem.Fields, isLenient);
                    queries.Merge(shouldSection);
                }
                else
                {
                    var shouldSection = CreateFallbackShouldSection(searchItem.Query, isLenient);
                    queries.Add(shouldSection);
                }
            }

            return queries;
        }

        public static JArray CreateMultiFieldShouldSection(string query, IReadOnlyCollection<IIisElasticField> searchFields, bool? isLenient)
        {
            var shouldSections = new JArray();

            foreach (var searchFieldGroup in searchFields.GroupBy(p => new { p.Fuzziness, p.Boost }))
            {
                var querySection = new JObject();
                var queryString = new JObject();
                queryString["query"] = SearchQueryExtension.IsExactQuery(query) || SearchQueryExtension.IsMatchAll(query)
                    ? query
                    : SearchQueryExtension.ApplyFuzzinessOperator(query);
                queryString["fuzziness"] = searchFieldGroup.Key.Fuzziness;
                queryString["boost"] = searchFieldGroup.Key.Boost;
                queryString["lenient"] = isLenient;
                queryString["fields"] = new JArray(searchFieldGroup.Select(p => p.Name));

                querySection["query_string"] = queryString;
                shouldSections.Add(querySection);
            }

            return shouldSections;
        }

        private static JObject CreateExactShouldSection(string query, bool? isLenient)
        {
            var result = new JObject();

            var queryString = new JObject();
            queryString["query"] = query;
            queryString["lenient"] = isLenient;
            result["query_string"] = queryString;

            return result;
        }

        private static JObject CreateFallbackShouldSection(string query, bool? isLenient)
        {
            var shouldSection = new JObject();
            var queryString = new JObject();
            queryString["query"] = query
                    .RemoveSymbols(ElasticManager.RemoveSymbolsPattern)
                    .EscapeSymbols(ElasticManager.EscapeSymbolsPattern);
            queryString["fields"] = new JArray("*");
            queryString["lenient"] = isLenient;
            shouldSection["query_string"] = queryString;

            return shouldSection;
        }
    }
}