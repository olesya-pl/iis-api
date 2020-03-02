using IIS.Core.GraphQL;
using IIS.Core.GraphQL.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class AdminController: Controller
    {
        ISchemaProvider _schemaProvider;

        public AdminController(ISchemaProvider schemaProvider)
        {
            _schemaProvider = schemaProvider;
        }

        [HttpGet("reload-ontology")]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            _schemaProvider.RecreateSchema();
            return Content("Ok");
        }
    }
}
