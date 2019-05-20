using System;
using System.Threading.Tasks;
using System.Transactions;
using GraphQL;
using GraphQL.DataLoader;
using IIS.GraphQL;
using IIS.OSchema;
using IIS.Search;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ISearchService _searchService;
        private readonly IOSchema _schema;

        public GraphController(IGraphQLSchemaProvider schemaProvider, DataLoaderDocumentListener documentListener, 
            ISearchService searchService, IOSchema schema)
        {
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
            _documentListener = documentListener ?? throw new ArgumentNullException(nameof(documentListener));
            _searchService = searchService;
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
            await _searchService.CreateIndexAsync(schema);

            return Ok();
        }
    }

}
