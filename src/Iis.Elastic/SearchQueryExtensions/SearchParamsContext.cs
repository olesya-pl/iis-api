using System;
using System.Linq;
using Iis.Interfaces.Elastic;

namespace Iis.Elastic.SearchQueryExtensions
{
    public interface ISearchParamsContext
    {
        ElasticMultiSearchParams ElasticMultiSearchParams { get; }
        SearchParameter BaseSearchParameter { get; }
        public bool IsBaseQueryExact => SearchQueryExtension.IsExactQuery(BaseSearchParameter.Query);
        public bool IsBaseQueryMatchAll => SearchQueryExtension.IsMatchAll(BaseSearchParameter.Query);
    }

    public class SearchParamsContext : ISearchParamsContext
    {
        private const int BaseParametersCount = 1;

        private SearchParamsContext(ElasticMultiSearchParams elasticMultiSearchParams)
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
        }

        public ElasticMultiSearchParams ElasticMultiSearchParams { get; private set; }
        public SearchParameter BaseSearchParameter { get; private set; }

        public static ISearchParamsContext CreateFrom(ElasticMultiSearchParams multiElasticSearchParams)
        {
            return new SearchParamsContext(multiElasticSearchParams);
        }
    }
}