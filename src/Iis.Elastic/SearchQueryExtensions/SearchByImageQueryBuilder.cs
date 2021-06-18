using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class SearchByImageQueryBuilder : BaseQueryBuilder<SearchByImageQueryBuilder>
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
                                source = "double total = 0.0; for(vector in params.vectorList) { total += 1/(l2norm(vector, doc['ImageVectors.Vector']) + 1); } return total/params.vectorListLength;",
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

            jsonQuery["min_score"] = 1.35;
            jsonQuery["query"] = query;

            //можно будет попробовать эти формулы модифицировать
            //source = "double total = 0.0; for(vector in params.vectorList) { total += (1.0+cosineSimilarity(vector, doc['ImageVectors.Vector'])); } return total/params.vectorListLength;",
            //source = "double total = 0.0; for(vector in params.vectorList) { total += 1/(l2norm(vector, doc['ImageVectors.Vector']) + 1); } return total/params.vectorListLength;",

            return jsonQuery;
        }
    }
}
