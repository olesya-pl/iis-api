using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class SearchByImageQueryBuilder : PaginatedQueryBuilder<SearchByImageQueryBuilder>
    {
        private decimal[][] _imageVectorList;

        public SearchByImageQueryBuilder(IReadOnlyCollection<decimal[]> imageVectorList)
        {
            _imageVectorList = imageVectorList.ToArray();
        }

        protected override JObject CreateQuery(JObject jsonQuery)
        {

            var termObject = JObject.FromObject(new {
                term = new {ParentId = "NULL"}
            });

            var nestedObject = JObject.FromObject(new
            {
                nested = new {
                    path = "ImageVectors",
                    score_mode = "max",
                    query = new {
                        script_score = new
                        {
                            query = new
                            {
                                match_all = new { }
                            },
                            script = new
                            {
                                source = "double total = 0.0; double result = 0.0; for(vector in params.vectorList) { total += 1/(l2norm(vector, doc['ImageVectors.Vector']) + 1); } result = total/params.vectorListLength; if (result < 0.67) {result = 0.0;} return result;",
                                @params = new
                                {
                                    vectorList = _imageVectorList,
                                    vectorListLength = _imageVectorList.Length
                                }
                            }
                        }
                    }
                }
            });

            var mustArray = new JArray(termObject, nestedObject);

            var boolObject = new JObject(
                new JProperty("must", mustArray)
            );

            var query = new JObject(
                new JProperty("bool", boolObject)
            );

            jsonQuery["min_score"] = 1.0;
            jsonQuery["query"] = query;

            return jsonQuery;
        }
    }
}
