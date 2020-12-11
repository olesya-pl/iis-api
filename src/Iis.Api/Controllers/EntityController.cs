using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;

using Iis.Events.Entities;

namespace Iis.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : Controller
    {
        private readonly IMediator _mediator;
        public EntityController(
            IMediator mediator)
        {
            _mediator = mediator;

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Detele(string id, [FromHeader] string authorization)
        {
            await _mediator.Publish(new EntityDeleteEvent { Id = Guid.NewGuid(), Type = "OLOLO" });

            return Content($"OK {id} {authorization}");
        }
    }
}