using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Iis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : Controller
    {
        [HttpDelete("{id}")]
        public Task<IActionResult> Detele(string id, [FromHeader]string authorization)
        {
            return Task.FromResult<IActionResult>(Content($"OK {id} {authorization}"));
        }
    }
}