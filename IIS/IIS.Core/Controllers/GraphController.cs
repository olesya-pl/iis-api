using System.Threading;
using System.Threading.Tasks;
using IIS.Introspection;
using Microsoft.AspNetCore.Mvc;

namespace IIS.Core.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class GraphController : Controller
    {
        private readonly QueueReanimator _reanimator;

        public GraphController(QueueReanimator reanimator)
        {
            _reanimator = reanimator;
        }

        [HttpPost("/api/schemarestore")]
        public async Task<IActionResult> SchemaRestore()
        {
            await _reanimator.RestoreSchema();
            return Ok();
        }

        [HttpPost("/api/restore")]
        public async Task<IActionResult> Restore(CancellationToken cancellationToken)
        {
            await _reanimator.RestoreOntology(cancellationToken);
            return Ok();
        }
    }
}
