using Iis.Interfaces.Ontology.Data;
using Iis.Interfaces.SecurityLevels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Iis.Api.Controllers
{
    [Route("securityLevel")]
    [ApiController]
    public class SecurityLevelController : ControllerBase
    {
        private readonly IOntologyNodesData _ontologyData;
        private readonly ISecurityLevelChecker _securityLevelChecker;
        public SecurityLevelController(
            IOntologyNodesData ontologyData,
            ISecurityLevelChecker securityLevelChecker)
        {
            _ontologyData = ontologyData;
            _securityLevelChecker = securityLevelChecker;
        }

        [HttpGet("getSecurityLevels")]
        public IReadOnlyList<SecurityLevelPlain> GetSecurityLevels() => _securityLevelChecker.GetSecurityLevelsPlain();
    }
}
