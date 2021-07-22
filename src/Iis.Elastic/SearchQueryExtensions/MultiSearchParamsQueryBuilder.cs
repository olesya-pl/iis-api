using System.Collections.Generic;
using System.Linq;
using Iis.Interfaces.Elastic;
using Iis.Utility;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class MultiSearchParamsQueryBuilder : BaseQueryBuilder<MultiSearchParamsQueryBuilder>
    {
        private List<(string Query, List<IIisElasticField> Fields)> _searchParams;
        private bool? _isLenient;

        public JArray ShouldSections { get; private set; }

        public MultiSearchParamsQueryBuilder(List<(string Query, List<IIisElasticField> Fields)> searchParams) 
        {
            _searchParams = searchParams;
        }

        public MultiSearchParamsQueryBuilder WithLeniency(bool lenient)
        {
            _isLenient = lenient;
            return this;
        }

        protected override JObject CreateQuery(JObject json)
        {
            json["query"]["bool"] = new JObject();
            var shouldSections = new JArray();

            foreach (var searchItem in _searchParams)
            {
                if (SearchQueryExtension.IsExactQuery(searchItem.Query))
                {
                    var shouldSection = CreateExactShouldSection(searchItem.Query);
                    shouldSections.Add(shouldSection);
                }
                else if (searchItem.Fields?.Any() == true)
                {
                    var shouldSection = CreateMultiFieldShouldSection(searchItem.Query, searchItem.Fields);
                    shouldSections.Merge(shouldSection);
                }
                else
                {
                    var shouldSection = CreateFallbackShouldSection(searchItem.Query);
                    shouldSections.Add(shouldSection);
                }
            }

            json["query"]["bool"]["should"] = shouldSections;
            ShouldSections = shouldSections;
            return json;
        }

        private JObject CreateExactShouldSection(string query)
        {
            var result = new JObject();

            var queryString = new JObject();
            queryString["query"] = query;
            queryString["lenient"] = _isLenient;
            result["query_string"] = queryString;

            return result;
        }

        private JArray CreateMultiFieldShouldSection(string query, List<IIisElasticField> searchFields)
        {
            var shouldSections = new JArray();

            foreach (var searchFieldGroup in searchFields.GroupBy(p => new { p.Fuzziness, p.Boost }))
            {
                var querySection = new JObject();
                var queryString = new JObject();
                queryString["query"] = ElasticManager.ApplyFuzzinessOperator(query
                    .RemoveSymbols(ElasticManager.RemoveSymbolsPattern)
                    .EscapeSymbols(ElasticManager.EscapeSymbolsPattern));
                queryString["fuzziness"] = searchFieldGroup.Key.Fuzziness;
                queryString["boost"] = searchFieldGroup.Key.Boost;
                queryString["lenient"] = _isLenient;
                queryString["fields"] = new JArray(searchFieldGroup.Select(p => p.Name));

                querySection["query_string"] = queryString;
                shouldSections.Add(querySection);
            }

            return shouldSections;
        }

        private JObject CreateFallbackShouldSection(string query)
        {
            var shouldSection = new JObject();
            var queryString = new JObject();
            queryString["query"] = query
                    .RemoveSymbols(ElasticManager.RemoveSymbolsPattern)
                    .EscapeSymbols(ElasticManager.EscapeSymbolsPattern);
            queryString["fields"] = new JArray("*");
            queryString["lenient"] = _isLenient;
            shouldSection["query_string"] = queryString;

            return shouldSection;
        }
    }
}
