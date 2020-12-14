using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;

using Iis.Events.Entities;
using Iis.Domain;

namespace Iis.Api.Controllers
{
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
        public async Task<IActionResult> Detele(string id, [FromHeader] string authorization)
        {
            var parseResult = Guid.TryParse(id, out Guid nodeId);

            if(!parseResult) return ValidationProblem();

            var node = _ontologySerivice.GetNode(nodeId) as Entity;

            if(node is null) return NotFound(id);

            _ontologySerivice.RemoveNode(node);

            await _mediator.Publish(EntityDeleteEvent.Create(node.Id, node.Type.Name));

            return Ok($"Deleted:{id}");
        }
    }
}