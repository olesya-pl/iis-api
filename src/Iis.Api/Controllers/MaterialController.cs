using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Iis.Api.Filters;

namespace Iis.Api.Controllers
{
    [BasicAuth]
    [Route("api/[controller]")]
    [ApiController]
    public class MaterialController : Controller
    {
        [HttpDelete("{id}")]
        public Task<IActionResult> Detele(Guid id)
        {
            return Task.FromResult<IActionResult>(Content($"OK {id:N}"));
        }
    }
}