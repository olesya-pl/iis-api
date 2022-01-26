using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Iis.Domain;
using Iis.Api.Controllers.Dto;
using Iis.Interfaces.SecurityLevels;
using Iis.Domain.TreeResult;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class OntologyController: Controller
    {
        IOntologyService _ontologyService;
        ISecurityLevelChecker _securityLevelChecker;

        public OntologyController(
            IOntologyService ontologyService, 
            ISecurityLevelChecker securityLevelChecker)
        {
            _ontologyService = ontologyService;
            _securityLevelChecker = securityLevelChecker;
        }

        [HttpPost("GetEventTypes")]
        public Task<IActionResult> GetEventTypes([FromBody] SearchParam param)
        {
            var treeItems = _ontologyService.GetEventTypes(param.Suggestion);

            var contentResult = new ContentResult
            {
                Content = treeItems.GetJson(),
                ContentType = "application/json"
            };

            return Task.FromResult<IActionResult>(contentResult);
        }

        [HttpPost("GetSecurityLevels")]
        public Task<IActionResult> GetSecurityLevels()
        {
            var treeItems = new TreeResultList().Init(
                new[] { _securityLevelChecker.RootLevel },
                _ => _.Name,
                _ => _.Id.ToString("N"),
                _ => _.Children);

            var contentResult = new ContentResult
            {
                Content = treeItems.GetJson(),
                ContentType = "application/json"
            };

            return Task.FromResult<IActionResult>(contentResult);
        }
    }
}
