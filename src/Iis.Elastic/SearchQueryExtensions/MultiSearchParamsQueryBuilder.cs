using System.Collections.Generic;
using Iis.Interfaces.Elastic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class MultiSearchParamsQueryBuilder : PaginatedQueryBuilder<MultiSearchParamsQueryBuilder>
    {
        private IReadOnlyCollection<SearchParameter> _searchParams;
        private bool? _isLenient;

        public MultiSearchParamsQueryBuilder(IReadOnlyCollection<SearchParameter> searchParams)
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
            json["query"]["bool"]["should"] = SearchParamsQueryHelper.AsQueries(_searchParams, _isLenient);

            return json;
        }
    }
}