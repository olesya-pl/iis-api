using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class MatchAllQueryBuilder : BaseQueryBuilder<MatchAllQueryBuilder>
    {
        public override JObject Build()
        {
            var jsonQuery = SearchQueryExtension.WithSearchJson(_resultFields, _from, _size);

            var matchAll = new JObject{
                new JProperty("match_all", new JObject())
            };

            jsonQuery["query"] = matchAll;

            return jsonQuery;
        }
    }
}