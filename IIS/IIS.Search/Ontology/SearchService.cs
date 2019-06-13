using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using IIS.Search.Schema;
using Nest;
using Newtonsoft.Json.Linq;

namespace IIS.Search.Ontology
{
    public class SearchService : ISearchService
    {
        private readonly IElasticClient _elasticClient;
        private readonly ISchemaRepository _schemaRepository;

        public SearchService(IElasticClient elasticClient, ISchemaRepository schemaRepository)
        {
            _elasticClient = elasticClient;
            _schemaRepository = schemaRepository;
        }

        public async Task<JObject> SearchAsync(string typeName, string query = null)
        {
            if (query is null) return await FindAsync(typeName);

            var schema = await _schemaRepository.GetSchemaAsync();
            var member = schema.Members.Single(m => m.Name == typeName);
            var type = (ComplexType)member.Target;
            var textFields = type.AllMembers.Select(m => m.Target).OfType<Attribute>()
                .Where(a => a.ScalarType == (string)ScalarType.String)
                .Select(a => a.Name);

            var queryString = new JValue(query);
            var json = JObject.Parse(FullTextQuery);
            var should = json["query"]["bool"]["should"];
            should[0]["multi_match"]["fields"] = new JArray(textFields);
            should[0]["multi_match"]["query"] = queryString;
            should[1]["multi_match"]["fields"] = new JArray(textFields);
            should[1]["multi_match"]["query"] = queryString;

            var queryText = json.ToString();
            var index = typeName.ToUnderscore();

            var response = await _elasticClient.LowLevel.SearchAsync<StringResponse>(index, queryText);
            var body = JObject.Parse(response.Body);
            return body;
        }

        public void IndexEntity(string message)
        {
            var json = JObject.Parse(message);
            var index = json["type"].Value<string>().ToUnderscore();
            var entity = json["entity"].ToString();
            var id = json["entity"]["id"].Value<string>();
            var postData = PostData.String(entity);
            var indexResponse = _elasticClient.LowLevel.IndexPut<StringResponse>(index, id, postData);
        }

        public async Task SaveSchemaAsync(ComplexType schema)
        {
            await _schemaRepository.SaveSchemaAsync(schema);

            foreach (var member in schema.Members)
            {
                var constraintName = member.Name;
                if (constraintName == "_relationInfo") continue;
                var type = (ComplexType)member.Target;
                if (type.Kind != Kind.Class) continue; // todo: tmp
                var indexName = type.Name.ToUnderscore();
                if (_elasticClient.IndexExists(indexName).Exists)
                    await _elasticClient.DeleteIndexAsync(indexName); // todo: use update
                await _elasticClient.CreateIndexAsync(indexName, desc => GetIndexRequest(desc, type));
            }
        }

        private ICreateIndexRequest GetIndexRequest(CreateIndexDescriptor descriptor, ComplexType type)
        {
            descriptor.Map(d => GetTypeMapping(d, type)).Settings(s => s.Setting("index.mapping.ignore_malformed", true));
            return descriptor;
        }

        private ITypeMapping GetTypeMapping(TypeMappingDescriptor<object> arg, ComplexType type)
        {
            arg.Properties(d => GetPropertiesSelector(d, type, 2));
            return arg;
        }

        private IPromise<IProperties> GetPropertiesSelector(PropertiesDescriptor<object> arg, ComplexType type, int depth)
        {
            if (depth == 0) return arg;

            foreach (var member in type.Members)
            {
                var constraintName = member.Name;
                if (member.Membership == Membership.Attribute)
                {
                    var isIndexed = true;//prop.Value.Value<bool>("_indexed");
                    var attribute = (Attribute)member.Target;
                    var attributeType = (ScalarType)attribute.ScalarType;
                    if (attributeType == ScalarType.String)
                        arg.Text(s => s.Name(constraintName).Analyzer("ukrainian").Index(isIndexed));
                    else if (attributeType == ScalarType.Int) arg.Number(s => s.Name(constraintName).Index(isIndexed));
                    else if (attributeType == ScalarType.Decimal) arg.Number(s => s.Name(constraintName).Index(isIndexed));
                    else if (attributeType == ScalarType.Keyword) arg.Keyword(s => s.Name(constraintName).Index(isIndexed));
                    else if (attributeType == ScalarType.Date) arg.Text(s => s.Name(constraintName).Index(isIndexed)); // temp
                    else if (attributeType == ScalarType.Boolean) arg.Boolean(s => s.Name(constraintName).Index(isIndexed));
                    else if (attributeType == ScalarType.Geo) arg.Text(s => s.Name(constraintName)); // temp
                    else arg.Text(s => s.Name(constraintName).Index(isIndexed));
                }
                else if (member.Membership == Membership.Type)
                {
                    var target = (ComplexType)member.Target;
                    arg.Object<object>(d =>
                        d.Name(constraintName)
                        .Properties(p => GetPropertiesSelector(p, target, depth - 1))
                    );
                }
            }
            return arg;
        }

        private async Task<JObject> FindAsync(string type)
        {
            var index = type.ToUnderscore();
            var parameters = new SearchRequestParameters { };
            var response = await _elasticClient.LowLevel.SearchGetAsync<StringResponse>(index, parameters);
            var json = JObject.Parse(response.Body);
            return json;
        }

        private const string FullTextQuery = 
        @"{
            ""query"": {
                ""bool"": {
                    ""should"": [
                        {
                            ""multi_match"": {
                                ""fields"": [],
                                ""query"": null,
                                ""type"": ""phrase_prefix""
                            }
                        },
                        {
                            ""multi_match"": {
                                ""fields"": [],
                                ""query"": null,
                                ""fuzziness"" : ""AUTO"",
                                ""prefix_length"" : 3
                            }
                        }
                    ],
                    ""minimum_should_match"": 1
                }
            }
        }";
    }
}
