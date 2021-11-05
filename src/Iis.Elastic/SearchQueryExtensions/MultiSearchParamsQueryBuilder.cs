using System.Collections.Generic;
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

        protected override JObject CreateQuery(JObject json)
        {
            json["query"]["bool"] = new JObject();
            json["query"]["bool"]["should"] = SearchParamsQueryHelper.AsQueries(_searchParams, _isLenient);

            return json;
        }
    }
}