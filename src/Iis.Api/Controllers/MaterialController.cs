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
        private readonly IAuthTokenService _authTokenService;

        public MaterialController(
            IUserService userService,
            IConfiguration configuration,
            IMaterialProvider materialProvider,
            IMaterialService materialService,
            IAuthTokenService authTokenService)
        {
            _userService = userService;
            _configuration = configuration;
            _materialProvider = materialProvider;
            _materialService = materialService;
            _authTokenService = authTokenService;
        }

        [HttpGet("GetAccess/{id}")]
        public async Task<IActionResult> GetAccess(Guid id)
        {
            var user = await _authTokenService.GetHttpRequestUserAsync(Request);

            if (await _materialProvider.GetMaterialAsync(id, user) == null)
            {
                return ValidationProblem("Material is not found");
            }

            var objectAccess = new ObjectAccess
            {
                Commenting = user.IsGranted(
                    AccessKind.Material,
                    AccessOperation.Commenting,
                    AccessCategory.Entity)
            };

            var json = JsonConvert.SerializeObject(objectAccess);

            return Content(json);
        }

        [HttpPost("RemoveMaterials")]
        public async Task<IActionResult> RemoveMaterials()
        {
            await _materialService.RemoveMaterials();
            return Ok();
        }

        [HttpGet("RemoveMaterial/{materialId}")]
        public async Task<IActionResult> RemoveMaterial(Guid materialId, CancellationToken cancellationToken)
        {
            await _materialService.RemoveMaterialAsync(materialId, cancellationToken);
            return Ok();
        }
    }
}