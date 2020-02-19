using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class ConfigController: Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var configName = "appsettings.json";
            if (!System.IO.File.Exists(configName))
            {
                return Content($"There is noconfig file with name {configName}");
            }
            var configText = System.IO.File.ReadAllText(configName);
            return Content(configText);
        }
    }
}
