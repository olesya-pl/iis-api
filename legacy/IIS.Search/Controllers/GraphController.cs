using System.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using IIS.Search.GraphQL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace IIS.Search.Controllers
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

        public GraphController(IGraphQLSchemaProvider schemaProvider, DataLoaderDocumentListener documentListener)
        {
            _schemaProvider = schemaProvider ?? throw new System.ArgumentNullException(nameof(schemaProvider));
            _documentListener = documentListener ?? throw new System.ArgumentNullException(nameof(documentListener));
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
    }
}
