using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.SecurityLevels;
using Iis.Services.Contracts.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("securityLevel")]
    [ApiController]
    public class SecurityLevelController : ControllerBase
    {
        private readonly IOntologyNodesData _ontologyData;
        private readonly ISecurityLevelChecker _securityLevelChecker;
        private readonly IUserService _userService;
        public SecurityLevelController(
            IOntologyNodesData ontologyData,
            ISecurityLevelChecker securityLevelChecker,
            IUserService userService)
        {
            _ontologyData = ontologyData;
            _securityLevelChecker = securityLevelChecker;
            _userService = userService;
        }

        [HttpGet("getSecurityLevels")]
        public IReadOnlyList<SecurityLevelPlain> GetSecurityLevels() => _securityLevelChecker.GetSecurityLevelsPlain();

        [HttpGet("getUserSecurityDtos")]
        public async Task<IReadOnlyList<UserSecurityDto>> GetUserSecurityDtos()
            => await _userService.GetUserSecurityDtosAsync();

        [HttpPost("saveUserSecurityDtos")]
        public async Task SaveUserSecurityDtos(UserSecurityDto userSecurityDto)
            => await _userService.SaveUserSecurityAsync(userSecurityDto);
    }
}
