﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class MultiSearchParamsQueryBuilder : BaseQueryBuilder<MultiSearchParamsQueryBuilder>
    {
        private List<(string Query, List<IIisElasticField> Fields)> _searchParams;
        private bool? _isLenient;

        public MultiSearchParamsQueryBuilder(List<(string Query, List<IIisElasticField> Fields)> searchParams) 
        {
            _searchParams = searchParams;
        }

        public MultiSearchParamsQueryBuilder WithLeniency(bool lenient)
        {
            _isLenient = lenient;
            return this;
        }

        public override JObject Build()
        {
            var json = SearchQueryExtension.WithSearchJson(_resultFields, _from, _size);
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
                queryString["query"] = ElasticManager.ApplyFuzzinessOperator(
                    ElasticManager.EscapeElasticSpecificSymbols(ElasticManager.RemoveSymbols(query, ElasticManager.RemoveSymbolsPattern),
                    ElasticManager.EscapeSymbolsPattern));
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
            queryString["query"] = ElasticManager.EscapeElasticSpecificSymbols(
                ElasticManager.RemoveSymbols(query, ElasticManager.RemoveSymbolsPattern), ElasticManager.EscapeSymbolsPattern);
            queryString["fields"] = new JArray("*");
            queryString["lenient"] = _isLenient;
            shouldSection["query_string"] = queryString;

            return shouldSection;
        }
    }
}
