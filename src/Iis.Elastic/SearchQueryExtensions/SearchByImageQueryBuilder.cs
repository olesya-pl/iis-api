using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Iis.Elastic.SearchQueryExtensions
{
    public class SearchByImageQueryBuilder : BaseQueryBuilder<SearchByImageQueryBuilder>
    {
        private decimal[] _imageVector;

        public SearchByImageQueryBuilder(decimal[] imageVector)
        {
            _imageVector = imageVector;
        }
        
        public override JObject Build()
        {
            var jsonQuery = SearchQueryExtension.WithSearchJson(_resultFields, _from, _size);
            jsonQuery["min_score"] = 0.1;
            jsonQuery["query"] = JObject.FromObject(new
            {
                script_score = new
                {
                    query = new
                    {
                        match_all = new { }
                    },
                    script = new
                    {
                        source = "1 / (l2norm(params.queryVector, doc['ImageVector']) + 1)",
                        @params = new
                        {
                            queryVector = _imageVector
                        }
                    }
                }
            });

            return jsonQuery;
        }
    }
}
