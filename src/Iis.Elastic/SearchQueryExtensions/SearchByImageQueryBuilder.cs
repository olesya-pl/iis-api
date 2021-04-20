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
            jsonQuery["min_score"] = 0.1;
            jsonQuery["query"] = JObject.FromObject(new
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
                                source = "double total = 0.0; for(vector in params.vectorList) { total += 1 / (l2norm(vector, doc['ImageVectors.Vector']) + 1); } return total/params.vectorListLength;",
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
            //можно будет попробовать эту формулу (1.0+cosineSimilarity(params.query_vector, 'my_vectors.vector'))
            return jsonQuery;
        }
    }
}
