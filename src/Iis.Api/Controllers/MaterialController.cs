using System;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Materials;
using Iis.Interfaces.Roles;
using Iis.Services.Contracts.Access;
using Iis.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        [HttpGet("GetAccess/{id}")]
        public async Task<IActionResult> GetAccess(Guid id)
        {
            var user = await _userService.GetAuthenticatedUserAsync(HttpContext);

            if (await _materialProvider.GetMaterialAsync(id, user) == null)
            {
                return ValidationProblem("Material is not found");
            }

            var isCommentingGranted = user.IsGranted(AccessKind.Material, AccessOperation.Commenting, AccessCategory.Entity);
            var objectAccess = new ObjectAccess
            {
                Commenting = isCommentingGranted
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