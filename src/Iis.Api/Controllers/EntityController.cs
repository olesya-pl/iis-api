using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Iis.Api.Authentication;
using IIS.Core;
using Iis.Domain;
using Iis.Events.Entities;
using Iis.Interfaces.Roles;
using Iis.Services.Contracts.Access;
using Iis.Services.Contracts.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Iis.Api.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class EntityController : Controller
    {
        private readonly IOntologyService _ontologySerivice;
        private readonly IMediator _mediator;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        public EntityController(
            IOntologyService ontologyService,
            IMediator mediator,
            IUserService userService,
            IConfiguration configuration)
        {
            _ontologySerivice = ontologyService;
            _mediator = mediator;
            _userService = userService;
            _configuration = configuration;
        }

        [Authorize(AuthenticationSchemes = AuthenticationSchemeConstants.OntologyAuthenticationScheme)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var node = _ontologySerivice.GetNode(id) as Entity;

            if (node is null) return NotFound(id);

            _ontologySerivice.RemoveNode(node);

            await _mediator.Publish(EntityDeleteEvent.Create(node.Id, node.Type.Name));

            return Ok($"Deleted:{id:N}");
        }

        [Authorize]
        [HttpGet("GetAccess/{id}")]
        public async Task<IActionResult> GetAccess(Guid id)
        {
            if (_ontologySerivice.GetNode(id) == null) return ValidationProblem("Entity is not found");

            var claim = HttpContext.User.FindFirstValue(TokenHelper.ClaimTypeUID);
            var userId = Guid.Parse(claim);
            var user = await _userService.GetUserAsync(userId, HttpContext.RequestAborted);
            var isCommentingGranted = user.IsGranted(AccessKind.Entity, AccessOperation.Commenting, AccessCategory.Entity);
            var objectAccess = new ObjectAccess
            {
                Commenting = isCommentingGranted
            };

            var json = JsonConvert.SerializeObject(objectAccess);

            return Content(json);
        }
    }
}