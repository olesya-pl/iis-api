using System;
using Newtonsoft.Json.Linq;
using Iis.Elastic.Dictionaries;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class SimpleQueryStringQueryBuilder : BaseQueryBuilder<SimpleQueryStringQueryBuilder>
    {
        private string _queryString;
        public SimpleQueryStringQueryBuilder(string queryString)
        {
            if (string.IsNullOrWhiteSpace(queryString))
            {
                throw new ArgumentNullException(nameof(queryString));
            }
            _queryString = queryString;
        }

        protected override JObject CreateQuery(JObject json)
        {
            var queryString = new JObject(
                                new JProperty(
                                    SearchQueryPropertyName.QueryString,
                                    new JObject(
                                        new JProperty(
                                            SearchQueryPropertyName.Query, _queryString))));

            json[SearchQueryPropertyName.Query] = queryString;

            return json;
        }
    }
}
