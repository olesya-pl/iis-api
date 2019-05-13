using System;
using System.Threading.Tasks;
using GraphQL;
using IIS.GraphQL;
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

        public GraphController(IGraphQLSchemaProvider schemaProvider)
        {
            _schemaProvider = schemaProvider ?? throw new ArgumentNullException(nameof(schemaProvider));
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
            });

            return Ok(result);
        }
    }

}
