using Iis.Interfaces.Ontology.Schema;
using Iis.Services.Contracts.Interfaces.Elastic;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace Iis.Services.Elastic
{
    public class OntologyElasticResponseManager : IElasticResponseManager
    {
        private readonly IFieldToAliasMapper _aliasMapper;
        const string NODE_TYPE_NAME = "NodeTypeName";
        public OntologyElasticResponseManager(IFieldToAliasMapper aliasMapper)
        {
            _aliasMapper = aliasMapper;
        }

        public Task<JObject> GenerateHighlightsWithoutDublications(JObject source, JToken highlights)
        {
            var nodeTypeName = source[NODE_TYPE_NAME].ToString();

            var result = new JObject();
            foreach (JProperty child in highlights.Children())
            {
                var fullName = $"{nodeTypeName}.{child.Name}";
                var alias = _aliasMapper.GetAlias(fullName);
                if (alias == null)
                {
                    result[child.Name] = child.Value;
                }
            }

            return Task.FromResult(result);
        }
    }
}
