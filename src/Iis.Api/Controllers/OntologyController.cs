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
        private readonly IOntologyService _ontologyService;
        private readonly ISecurityLevelChecker _securityLevelChecker;

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
                Content = treeItems.GetJson("label", "value", "options"),
                ContentType = "application/json"
            };

            return Task.FromResult<IActionResult>(contentResult);
        }

        [HttpPost("GetSecurityLevels")]
        public Task<IActionResult> GetSecurityLevels()
        {
            var treeItems = new TreeResultList().Init(
                _securityLevelChecker.RootLevel.Children,
                _ => _.Name,
                _ => _.Id.ToString("N"),
                _ => _.Children);

            var contentResult = new ContentResult
            {
                Content = treeItems.GetJson("name", "id", "options"),
                ContentType = "application/json"
            };

            return Task.FromResult<IActionResult>(contentResult);
        }
    }
}
