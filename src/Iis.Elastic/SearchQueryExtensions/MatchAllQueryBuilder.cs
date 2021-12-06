using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class MatchAllQueryBuilder : PaginatedQueryBuilder<MatchAllQueryBuilder>
    {
        protected override JObject CreateQuery(JObject jsonQuery)
        {
            var matchAll = new JObject{
                new JProperty("match_all", new JObject())
            };

            jsonQuery["query"] = matchAll;

            return jsonQuery;
        }
    }
}