using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json.Linq;

namespace IIS.Search
{
    public class OntologySearchService
    {
        private readonly IElasticClient _elasticClient;

        public OntologySearchService(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public async Task<JObject> Find(string type)
        {
            var parameters = new SearchRequestParameters
            {

            };
            var response = await _elasticClient.LowLevel.SearchGetAsync<StringResponse>(type, parameters);
            var json = JObject.Parse(response.Body);
            return json;
        }
    }
}
