using Iis.Interfaces.Elastic;
using System;
using System.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public interface ISearchParamsContext
    {
        IElasticMultiSearchParams ElasticMultiSearchParams { get; }
        SearchParameter BaseSearchParameter { get; }
        SearchParameter HistorySearchParameter { get; }
        public bool HasAdditionalParameters => HistorySearchParameter != null;
        public bool IsBaseQueryExact => SearchQueryExtension.IsExactQuery(BaseSearchParameter.Query);
        public bool IsBaseQueryMatchAll => SearchQueryExtension.IsMatchAll(BaseSearchParameter.Query);
    }

    public class SearchParamsContext : ISearchParamsContext
    {
        private const int BaseParametersCount = 1;

        private SearchParamsContext(IElasticMultiSearchParams elasticMultiSearchParams)
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
        }

        public IElasticMultiSearchParams ElasticMultiSearchParams { get; private set; }
        public SearchParameter BaseSearchParameter { get; private set; }
        public SearchParameter HistorySearchParameter { get; private set; }

        public static ISearchParamsContext CreateFrom(IElasticMultiSearchParams multiElasticSearchParams)
        {
            return new SearchParamsContext(multiElasticSearchParams);
        }
    }
}