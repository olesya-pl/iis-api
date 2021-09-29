using System;
using System.Threading.Tasks;
using System.Security.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MediatR;
using Newtonsoft.Json;

using Iis.Events.Entities;
using Iis.Domain;
using IIS.Core;
using Iis.Interfaces.Roles;
using Iis.Services.Contracts.Access;
using Iis.Services.Contracts.Interfaces;

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var node = _ontologySerivice.GetNode(id) as Entity;

            if(node is null) return NotFound(id);

            _ontologySerivice.RemoveNode(node);

            await _mediator.Publish(EntityDeleteEvent.Create(node.Id, node.Type.Name));

            return Ok($"Deleted:{id:N}");
        }

        [HttpGet("GetAccess/{id}")]
        public async Task<IActionResult> GetAccess(Guid id)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                throw new AuthenticationException("Requires \"Authorization\" header to contain a token");
            }

            var tokenPayload = await TokenHelper.ValidateTokenAsync(token, _configuration, _userService);

            if (_ontologySerivice.GetNode(id) == null) return ValidationProblem("Entity is not found");

            var objectAccess = new ObjectAccess
            {
                Commenting = tokenPayload.User.IsGranted(
                    AccessKind.Entity, 
                    AccessOperation.Commenting, 
                    AccessCategory.Entity)
            };

            var json = JsonConvert.SerializeObject(objectAccess);

            return Content(json);
        }
    }
}