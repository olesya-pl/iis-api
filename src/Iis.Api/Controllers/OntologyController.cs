using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using Iis.Domain;
using Iis.Api.Controllers.Dto;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class OntologyController: Controller
    {
        IOntologyService _ontologyService;
        public OntologyController(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
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
    }
}
