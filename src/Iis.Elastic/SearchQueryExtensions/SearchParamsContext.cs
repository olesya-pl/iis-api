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
        IMultiElasticSearchParams MultiElasticSearchParams { get; }
        (string Query, List<IIisElasticField> Fields) BaseSearchParameter { get; }
        (string Query, List<IIisElasticField> Fields) HistorySearchParameter { get; }
        IReadOnlyDictionary<string, string> AggregateHistoryResultQueries { get; }
        public bool HasAdditionalParameters => HistorySearchParameter.Query != null && HistorySearchParameter.Fields != null;
        public bool IsBaseQueryExact => SearchQueryExtension.IsExactQuery(BaseSearchParameter.Query);
        public bool IsBaseQueryMatchAll => SearchQueryExtension.IsMatchAll(BaseSearchParameter.Query);

        public JArray GetAdditionalQueriesForAggregate(AggregationField aggregationField = default)
        {
            if (aggregationField == default
                || (!AggregateHistoryResultQueries.TryGetValue(aggregationField.Name, out var aggregateQuery)
                && !AggregateHistoryResultQueries.TryGetValue(aggregationField.Alias, out aggregateQuery)))
                return SearchParamsQueryHelper.CreateMultiFieldShouldSection(HistorySearchParameter.Query, HistorySearchParameter.Fields, MultiElasticSearchParams.IsLenient);

            return SearchParamsQueryHelper.CreateMultiFieldShouldSection(aggregateQuery, HistorySearchParameter.Fields, MultiElasticSearchParams.IsLenient);
        }
    }

    public class SearchParamsContext : ISearchParamsContext
    {
        private const int BaseParametersCount = 1;

        private SearchParamsContext(
            IMultiElasticSearchParams multiElasticSearchParams,
            IReadOnlyDictionary<string, string> aggregateHistoryResultQueries)
        {
            if (multiElasticSearchParams is null)
            {
                throw new ArgumentNullException(nameof(multiElasticSearchParams));
            }
            if (multiElasticSearchParams.SearchParams.Count < BaseParametersCount)
            {
                throw new ArgumentException("Must contain base query", nameof(multiElasticSearchParams));
            }

            MultiElasticSearchParams = multiElasticSearchParams;
            BaseSearchParameter = multiElasticSearchParams.SearchParams.First();
            HistorySearchParameter = multiElasticSearchParams.SearchParams
                .Skip(BaseParametersCount)
                .FirstOrDefault();
            AggregateHistoryResultQueries = aggregateHistoryResultQueries;
        }

        public IMultiElasticSearchParams MultiElasticSearchParams { get; private set; }
        public (string Query, List<IIisElasticField> Fields) BaseSearchParameter { get; private set; }
        public (string Query, List<IIisElasticField> Fields) HistorySearchParameter { get; private set; }
        public IReadOnlyDictionary<string, string> AggregateHistoryResultQueries { get; private set; }

        public static ISearchParamsContext CreateFrom(
            IMultiElasticSearchParams multiElasticSearchParams,
            IReadOnlyDictionary<string, string> aggregateHistoryResultQueries)
        {
            return new SearchParamsContext(multiElasticSearchParams, aggregateHistoryResultQueries);
        }

        public static ISearchParamsContext CreateAggregatesContextFrom(ISearchParamsContext context, ElasticFilter filter)
        {
            var searchParams = new List<(string Query, List<IIisElasticField> Fields)>(context.MultiElasticSearchParams.SearchParams.Count);
            var aggregatesElasticFilter = new ElasticFilter
            {
                CherryPickedItems = filter.CherryPickedItems,
                Limit = filter.Limit,
                Offset = filter.Offset,
                Suggestion = filter.Suggestion
            };
            var baseSearchQuery = aggregatesElasticFilter.ToQueryString(context.IsBaseQueryExact);

            searchParams.Add((baseSearchQuery, context.BaseSearchParameter.Fields));

            if (context.HasAdditionalParameters)
            {
                searchParams.Add(context.HistorySearchParameter);
            }

            var multiElasticSearchParams = new MultiElasticSearchParams
            {
                IsLenient = context.MultiElasticSearchParams.IsLenient,
                From = context.MultiElasticSearchParams.From,
                Size = context.MultiElasticSearchParams.Size,
                ResultFields = context.MultiElasticSearchParams.ResultFields.ToList(),
                BaseIndexNames = context.MultiElasticSearchParams.BaseIndexNames.ToList(),
                SearchParams = searchParams
            };

            return new SearchParamsContext(multiElasticSearchParams, context.AggregateHistoryResultQueries);
        }
    }
}