using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class SearchAsYouTypeQueryBuilder : PaginatedQueryBuilder<SearchAsYouTypeQueryBuilder>
    {
        private readonly string _query;
        private readonly IReadOnlyCollection<string> _fieldNames;

        public SearchAsYouTypeQueryBuilder(string query, IReadOnlyCollection<string> fieldNames)
        {
            _query = query;
            _fieldNames = fieldNames;
        }

        protected override JObject CreateQuery(JObject json)
        {
            var fieldsArray = new List<string>();

            foreach (var field in _fieldNames)
            {
                fieldsArray.AddRange(new[] {
                    field,
                    $"{field}._2gram",
                    $"{field}._3gram"
                });
            }

            var query = JObject.FromObject(new {
                multi_match = new
                {
                    query = _query,
                    type = "bool_prefix",
                    fields = fieldsArray
                }
            });

            json["query"] = query;

            return json;
        }
    }
}
