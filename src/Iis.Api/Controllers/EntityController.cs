using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;

using Iis.Events.Entities;
using Iis.Domain;
using Iis.Api.Filters;
namespace Iis.Api.Controllers
{
    [BasicAuth]
    [Route("api/[controller]")]
    [ApiController]
    public class EntityController : Controller
    {
        private readonly IOntologyService _ontologySerivice;
        private readonly IMediator _mediator;
        public EntityController(
            IOntologyService ontologyService,
            IMediator mediator)
        {
            _ontologySerivice = ontologyService;
            _mediator = mediator;

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Detele(Guid id)
        {
            var node = _ontologySerivice.GetNode(id) as Entity;

            if(node is null) return NotFound(id);

            _ontologySerivice.RemoveNode(node);

            await _mediator.Publish(EntityDeleteEvent.Create(node.Id, node.Type.Name));

            return Ok($"Deleted:{id:N}");
        }
    }
}