using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class MatchAllQueryBuilder : BaseQueryBuilder<MatchAllQueryBuilder>
    {
        public JObject Build()
        {
            var jsonQuery = SearchQueryExtension.WithSearchJson(_resultFields, _offset, _limit);

            var matchAll = new JObject{
                new JProperty("match_all", new JObject())
            };

            jsonQuery["query"] = matchAll;

            return jsonQuery;
        }
    }
}