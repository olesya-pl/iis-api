using System;
using System.Threading.Tasks;
using IIS.Storage;
using Microsoft.AspNetCore.Mvc;

namespace IIS.Web.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class GraphController : Controller
    {
        private readonly ISchemaRepository _schemaRepository;

        public GraphController(ISchemaRepository schemaRepository)
        {
            _schemaRepository = schemaRepository ?? throw new ArgumentNullException(nameof(schemaRepository));
        }

        public async Task<IActionResult> Post()
        {
            return Ok(await _schemaRepository.Ping());
        }
    }
}
