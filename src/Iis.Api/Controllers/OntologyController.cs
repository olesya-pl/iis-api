using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Iis.Domain;
using Iis.Api.Controllers.Dto;
using Iis.Interfaces.SecurityLevels;
using Iis.Domain.TreeResult;
using System;
using Iis.Services.Contracts.Interfaces;
using System.Security.Authentication;
using IIS.Core;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class OntologyController: Controller
    {
        private readonly IOntologyService _ontologyService;
        private readonly ISecurityLevelChecker _securityLevelChecker;
        private readonly INodeJsonService _nodeJsonService;
        private readonly IAuthTokenService _authTokenService;

        public OntologyController(
            IOntologyService ontologyService,
            ISecurityLevelChecker securityLevelChecker,
            INodeJsonService nodeJsonService,
            IAuthTokenService authTokenService)
        {
            _ontologyService = ontologyService;
            _securityLevelChecker = securityLevelChecker;
            _nodeJsonService = nodeJsonService;
            _authTokenService = authTokenService;
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

        [HttpPost("GetEntity")]
        public async Task<IActionResult> GetEntity([FromBody] GetEntityParam param)
        {
            var user = await _authTokenService.GetHttpRequestUserAsync(Request);

            var node = _ontologyService.GetNode(param.Id);
            var json = _nodeJsonService.GetJson(node.OriginalNode, user, param.Options);
            var contentResult = new ContentResult
            {
                Content = json,
                ContentType = "application/json"
            };
            return contentResult;
        }
    }
}
