using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using IIS.Ontology.GraphQL;
using IIS.Replication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using IIS.Core;
using System.Linq;

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
        private readonly IReplicationService _replicationService;
        private readonly IOSchema _schema;

        public GraphController(IGraphQLSchemaProvider schemaProvider, DataLoaderDocumentListener documentListener, 
            IReplicationService replicationService, IOSchema schema)
        {
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _documentListener = documentListener ?? throw new ArgumentNullException(nameof(documentListener));
            _replicationService = replicationService;
            _schema = schema;
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
                _.Listeners.Add(_documentListener);
            });

            return Ok(result);
        }

        [HttpPost("/api/search")]
        public async Task<IActionResult> CreateIndex()
        {
            var schema = await _schema.GetRootAsync();
            await _replicationService.CreateIndexAsync(schema);

            return Ok();
        }

        [HttpPost("/api/index")]
        public async Task<IActionResult> Index()
        {
            var endpoints = await _schema.GetEntitiesAsync(new[] { "Person" });
            var person = endpoints["person"];
            var people = await _schema.GetEntitiesByAsync(person.Relations.Select(r => (((Entity)r.Target).Id,"person")));
            foreach (var relation in people.Values)
            {
                //relation.Schema.Resolver.ResolveAsync(new ResolveContext {  })
                //var entity = (Entity)relation.Target;
                //await _replicationService.IndexEntityAsync(entity);
            }

            return Ok();
        }
    }

}
