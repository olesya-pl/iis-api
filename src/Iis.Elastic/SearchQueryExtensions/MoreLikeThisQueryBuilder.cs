using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class MoreLikeThisQueryBuilder : PaginatedQueryBuilder<MoreLikeThisQueryBuilder>
    {
        private string guid;
        public MoreLikeThisQueryBuilder WithMaterialId(string materialId)
        {
            guid = materialId;
            return this;
        }
        protected override JObject CreateQuery(JObject jsonQuery)
        {
            if(string.IsNullOrWhiteSpace(guid)) return jsonQuery;

            var parentNull = new JObject
            {
                new JProperty("term", new JObject{ new JProperty("ParentId", "NULL") })
            };

            var moreLikeContent = new JObject
            {
                new JProperty("fields", new JArray("Content")),
                new JProperty("like", new JArray(new JObject{ new JProperty("_id", guid)})),
                new JProperty("min_term_freq", 1)
            };

            var moreLike = new JObject
            {
                new JProperty("more_like_this", moreLikeContent)
            };

            var mustExprArray = new JArray{
                parentNull,
                moreLike
            };

            var mustExpr = new JObject
            {
                new JProperty("must", mustExprArray)
            };

            var query = new JObject
            {
                new JProperty("bool", mustExpr)
            };

            jsonQuery["query"] = query;

            return jsonQuery;
        }
    }
}