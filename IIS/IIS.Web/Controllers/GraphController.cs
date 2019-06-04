using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using IIS.Core;
using IIS.Introspection;
using IIS.Search;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json.Linq;

namespace IIS.Web.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class GraphController : Controller
    {
        public class GraphQuery
        {
            public string OperationName { get; set; }
            public string NamedQuery { get; set; }
            public string Query { get; set; }
            public JObject Variables { get; set; }
        }

        private readonly IGraphQLSchemaProvider _schemaProvider;
        private readonly DataLoaderDocumentListener _documentListener;
        private readonly QueueReanimator _reanimator;
        private readonly OntologySearchService _searchService;
        private readonly IElasticClient _elasticClient;
        private readonly IOSchema _oSchema;

        public GraphController(IGraphQLSchemaProvider schemaProvider, DataLoaderDocumentListener documentListener, 
            QueueReanimator reanimator, OntologySearchService searchService, IElasticClient elasticClient,
            IOSchema oSchema)
        {
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _documentListener = documentListener ?? throw new ArgumentNullException(nameof(documentListener));
            _reanimator = reanimator;
            _searchService = searchService;
            //
            _elasticClient = elasticClient;
            _oSchema = oSchema;
        }

        public async Task<IActionResult> Post([FromBody] GraphQuery query)
        {
            var inputs = query.Variables.ToInputs();
            var schema = await _schemaProvider.GetSchemaAsync();
            var result = await new DocumentExecuter().ExecuteAsync(_ =>
            {
                _.Schema = schema;
                _.Query = query.Query;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;
                //_.Listeners.Add(_documentListener);
            });

            return Ok(result);
        }

        [HttpPost("/api/restore")]
        public async Task<IActionResult> Restore()
        {
            await _reanimator.RestoreOntology();
            return Ok();
        }

        [HttpPost("/api/createindex")]
        public async Task<IActionResult> CreateIndex()
        {
            var schema = await _oSchema.GetRootAsync();
            await CreateIndexAsync(schema);

            return Ok();
        }

        private async Task CreateIndexAsync(TypeEntity schema)
        {
            foreach (var constraintName in schema.ConstraintNames)
            {
                if (constraintName == "_relationInfo") continue;
                var constraint = schema.GetConstraint(constraintName);
                var type = (TypeEntity)constraint.Target;
                var indexName = type.HasParent ? (type.Parent.Name + type.Name).ToUnderscore() : type.Name.ToUnderscore();
                if (_elasticClient.IndexExists(indexName).Exists)
                    await _elasticClient.DeleteIndexAsync(indexName); // todo: use update
                await _elasticClient.CreateIndexAsync(indexName, desc => GetIndexRequest(desc, constraint));
            }
        }

        private ICreateIndexRequest GetIndexRequest(CreateIndexDescriptor descriptor, Constraint constraint)
        {
            var type = (TypeEntity)constraint.Target;
            descriptor.Map(d => GetTypeMapping(d, type));
            return descriptor;
        }

        private ITypeMapping GetTypeMapping(TypeMappingDescriptor<object> arg, TypeEntity type)
        {
            arg.Properties(d => GetPropertiesSelector(d, type, 3));
            return arg;
        }

        private IPromise<IProperties> GetPropertiesSelector(PropertiesDescriptor<object> arg, TypeEntity type, int depth)
        {
            if (depth == 0) return arg;

            foreach (var constraintName in type.ConstraintNames)
            {
                if (type.HasAttribute(constraintName))
                {
                    var isIndexed = true;//prop.Value.Value<bool>("_indexed");
                    var attribute = type.GetAttribute(constraintName);
                    if (attribute.Type == ScalarType.String) arg.Text(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Int) arg.Number(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Decimal) arg.Number(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Keyword) arg.Keyword(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Date) arg.Date(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Boolean) arg.Boolean(s => s.Name(constraintName).Index(isIndexed));
                    else if (attribute.Type == ScalarType.Geo) arg.GeoShape(s => s.Name(constraintName));
                    else arg.Text(s => s.Name(constraintName).Index(isIndexed));
                }
                else if (type.HasEntity(constraintName))
                {
                    var target = type.GetEntity(constraintName);
                    //var value = map.GetValue(constraintName) as JObject;
                    arg.Object<object>(d =>
                        d.Name(constraintName)
                        .Properties(p => GetPropertiesSelector(p, target, depth - 1))
                    );
                }
                else if (type.HasUnion(constraintName))
                {
                    var union = type.GetUnion(constraintName);
                    //var value = map.GetValue(constraintName) as JObject;
                    foreach (var target in union.Classes)
                    {
                        arg.Object<object>(d =>
                            d.Name($"{constraintName}_{target.Name}")
                            .Properties(p => GetPropertiesSelector(p, target, depth - 1))
                        );
                    }
                }
            }
            return arg;
        }
    }

}
