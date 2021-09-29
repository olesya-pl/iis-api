using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Iis.Api.Filters;
using System.Security.Authentication;
using IIS.Core;
using Iis.Services.Contracts.Access;
using Iis.Services.Contracts.Interfaces;
using IIS.Services.Contracts.Interfaces;
using Iis.Interfaces.Roles;
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

        public MaterialController(
            IUserService userService,
            IConfiguration configuration,
            IMaterialProvider materialProvider)
        {
            _userService = userService;
            _configuration = configuration;
            _materialProvider = materialProvider;
        }

        [HttpGet("GetAccess/{id}")]
        public async Task<IActionResult> GetAccess(Guid id)
        {
            if (!Request.Headers.TryGetValue("Authorization", out var token))
                throw new AuthenticationException("Requires \"Authorization\" header to contain a token");

            var tokenPayload = TokenHelper.ValidateToken(token, _configuration, _userService);

            if (await _materialProvider.GetMaterialAsync(id, tokenPayload.User) == null)
                return ValidationProblem("Material is not found");

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
    }
}