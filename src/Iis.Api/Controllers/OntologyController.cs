using Iis.Api.Controllers.Dto;
using Iis.Domain;
using Iis.Interfaces.Ontology.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetEventTypes([FromBody] SearchParam param)
        {
            var treeItems = _ontologyService.GetEventTypes(param.Suggestion);
            return new ContentResult
            {
                Content = treeItems.GetJson(),
                ContentType = "application/json"
            };
        }
    }
}
