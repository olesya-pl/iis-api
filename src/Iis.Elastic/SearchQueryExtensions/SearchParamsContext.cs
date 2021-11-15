using Iis.Domain.Elastic;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public interface ISearchParamsContext
    {
        IElasticMultiSearchParams ElasticMultiSearchParams { get; }
        SearchParameter BaseSearchParameter { get; }
        SearchParameter HistorySearchParameter { get; }
        IReadOnlyDictionary<string, string> AggregateHistoryResultQueries { get; }
        public bool HasAdditionalParameters => HistorySearchParameter != null;
        public bool IsBaseQueryExact => SearchQueryExtension.IsExactQuery(BaseSearchParameter.Query);
        public bool IsBaseQueryMatchAll => SearchQueryExtension.IsMatchAll(BaseSearchParameter.Query);

        public JArray GetAdditionalQueriesForAggregate(AggregationField aggregationField = default)
        {
            if (aggregationField == default
                || (!AggregateHistoryResultQueries.TryGetValue(aggregationField.Name, out var aggregateQuery)
                && !AggregateHistoryResultQueries.TryGetValue(aggregationField.Alias, out aggregateQuery)))
                return SearchParamsQueryHelper.CreateMultiFieldShouldSection(HistorySearchParameter.Query, HistorySearchParameter.Fields, ElasticMultiSearchParams.IsLenient);

            return SearchParamsQueryHelper.CreateMultiFieldShouldSection(aggregateQuery, HistorySearchParameter.Fields, ElasticMultiSearchParams.IsLenient);
        }
    }

    public class SearchParamsContext : ISearchParamsContext
    {
        private const int BaseParametersCount = 1;

        private SearchParamsContext(
            IElasticMultiSearchParams elasticMultiSearchParams,
            IReadOnlyDictionary<string, string> aggregateHistoryResultQueries)
        {
            if (elasticMultiSearchParams is null)
            {
                throw new ArgumentNullException(nameof(elasticMultiSearchParams));
            }
            if (elasticMultiSearchParams.SearchParams.Count < BaseParametersCount)
            {
                throw new ArgumentException("Must contain base query", nameof(elasticMultiSearchParams));
            }

            ElasticMultiSearchParams = elasticMultiSearchParams;
            BaseSearchParameter = elasticMultiSearchParams.SearchParams.First();
            HistorySearchParameter = elasticMultiSearchParams.SearchParams
                .Skip(BaseParametersCount)
                .FirstOrDefault();
            AggregateHistoryResultQueries = aggregateHistoryResultQueries;
        }

        public IElasticMultiSearchParams ElasticMultiSearchParams { get; private set; }
        public SearchParameter BaseSearchParameter { get; private set; }
        public SearchParameter HistorySearchParameter { get; private set; }
        public IReadOnlyDictionary<string, string> AggregateHistoryResultQueries { get; private set; }

        public static ISearchParamsContext CreateFrom(
            IElasticMultiSearchParams multiElasticSearchParams,
            IReadOnlyDictionary<string, string> aggregateHistoryResultQueries)
        {
            return new SearchParamsContext(multiElasticSearchParams, aggregateHistoryResultQueries);
        }

        public static ISearchParamsContext CreateAggregatesContextFrom(ISearchParamsContext context, ElasticFilter filter)
        {
            var searchParams = new List<SearchParameter>(context.ElasticMultiSearchParams.SearchParams.Count);
            var aggregatesElasticFilter = new ElasticFilter
            {
                CherryPickedItems = filter.CherryPickedItems,
                Limit = filter.Limit,
                Offset = filter.Offset,
                Suggestion = filter.Suggestion
            };
            var baseSearchQuery = aggregatesElasticFilter.ToQueryString(context.IsBaseQueryExact);
            var searchParameter = new SearchParameter(baseSearchQuery, context.BaseSearchParameter.Fields, aggregatesElasticFilter.IsExact);

            searchParams.Add(searchParameter);

            if (context.HasAdditionalParameters)
            {
                searchParams.Add(context.HistorySearchParameter);
            }

            var elasticMultiSearchParams = new ElasticMultiSearchParams
            {
                IsLenient = context.ElasticMultiSearchParams.IsLenient,
                From = context.ElasticMultiSearchParams.From,
                Size = context.ElasticMultiSearchParams.Size,
                ResultFields = context.ElasticMultiSearchParams.ResultFields.ToList(),
                BaseIndexNames = context.ElasticMultiSearchParams.BaseIndexNames.ToList(),
                SearchParams = searchParams
            };

            return new SearchParamsContext(elasticMultiSearchParams, context.AggregateHistoryResultQueries);
        }
    }
}