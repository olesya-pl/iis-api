using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class DisjunctionQueryBuilder : PaginatedQueryBuilder<DisjunctionQueryBuilder>
    {
        private readonly string _query;
        private readonly IReadOnlyCollection<string> _fieldNames;

        public DisjunctionQueryBuilder(string query, IReadOnlyCollection<string> fieldNames)
        {
            _query = query;
            _fieldNames = fieldNames;
        }

        protected override JObject CreateQuery(JObject json)
        {
            var fieldsArray = new List<JObject>();

            foreach (var field in _fieldNames)
            {
                var autoCompleteQuery = new JObject();
                autoCompleteQuery["query"] = _query;
                var queryByField = new JObject();
                queryByField[field] = autoCompleteQuery;
                var prefixQuery = new JObject();
                prefixQuery["match_phrase_prefix"] = queryByField;
                fieldsArray.Add(prefixQuery);
            }

            var query = JObject.FromObject(new
            {
                dis_max = new
                {
                    queries = fieldsArray
                }
            });

            json["query"] = query;

            return json;
        }
    }
}
