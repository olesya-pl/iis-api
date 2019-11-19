using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Controllers
{
    [Route("/api/server-health")]
    [ApiController]
    public class HealthcheckController : Controller
    {
        private DbContext _dbCtx;

        public HealthcheckController(OntologyContext ctx)
        {
            _dbCtx = ctx;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            Response.Headers.Add("Content-Type", "application/health+json");

            return Json(new {
                version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion,
                availability = new {
                    db = _dbCtx.Database.CanConnect(),
                    queue = true, // TODO: the app should reuse single connection to rabbitmq
                    search = true // TODO: elastic is not used right now
                }
            });
        }
    }
}
