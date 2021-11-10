using System;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core;
using IIS.Core.Materials;
using Iis.Interfaces.Roles;
using Iis.Services.Contracts.Access;
using Iis.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Iis.Api.Controllers
{
    [Route("api/{controller}")]
    [ApiController]
    public class MaterialController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMaterialProvider _materialProvider;
        private readonly IMaterialService _materialService;

        public MaterialController(
            IUserService userService,
            IConfiguration configuration,
            IMaterialProvider materialProvider,
            IMaterialService materialService)
        {
            _userService = userService;
            _configuration = configuration;
            _materialProvider = materialProvider;
            _materialService = materialService;
        }

        [HttpGet("GetAccess/{id}")]
        public async Task<IActionResult> GetAccess(Guid id)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token))
            {
                throw new AuthenticationException("Requires \"Authorization\" header to contain a token");
            }

            var tokenPayload = await TokenHelper.ValidateTokenAsync(token, _configuration, _userService);

            if (await _materialProvider.GetMaterialAsync(id, tokenPayload.User) == null)
            {
                return ValidationProblem("Material is not found");
            }

            var objectAccess = new ObjectAccess
            {
                Commenting = tokenPayload.User.IsGranted(
                    AccessKind.Material,
                    AccessOperation.Commenting,
                    AccessCategory.Entity)
            };

            var json = JsonConvert.SerializeObject(objectAccess);

            return Content(json);
        }

        [HttpDelete("RemoveMaterials")]
        public async Task<IActionResult> RemoveMaterials()
        {
            await _materialService.RemoveMaterials();
            return Ok();
        }

        [HttpDelete("{materialId}")]
        public async Task<IActionResult> RemoveMaterial(Guid materialId, CancellationToken cancellationToken)
        {
            await _materialService.RemoveMaterialAsync(materialId, cancellationToken);
            return Ok();
        }
    }
}