using System.Threading;
using System.Threading.Tasks;
using IIS.Core.GraphQL;
using Microsoft.AspNetCore.Mvc;
//using Type = IIS.Core.Ontology.EntityFramework.Context.Type;
//using RelationType = IIS.Core.Ontology.EntityFramework.Context.RelationType;
using IIS.Legacy.EntityFramework;

namespace IIS.Core.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class ReanimatorController : Controller
    {
        private readonly ILegacyOntologyProvider _legacyOntologyProvider;
        //private readonly QueueReanimator _reanimator;
        //private OntologyContext _context;

        public ReanimatorController(ILegacyOntologyProvider legacyOntologyProvider)//, OntologyContext context
        {
            _legacyOntologyProvider = legacyOntologyProvider;
            //_schemaProvider = schemaProvider;
            //_reanimator = reanimator;
            //_context = context;
        }

        public async Task<IActionResult> Get()
        {
            _legacyOntologyProvider.GetTypes();
            return null;
        }


        [HttpPost("/api/schemarestore")]
        public async Task<IActionResult> SchemaRestore()
        {
            //await _reanimator.RestoreSchema();
            return Ok();
        }

        [HttpPost("/api/restore")]
        public async Task<IActionResult> Restore(CancellationToken cancellationToken)
        {
            //await _reanimator.RestoreOntology(cancellationToken);
            return Ok();
        }
    }
}
