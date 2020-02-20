using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Iis.Api.Controllers
{
    [Route("{controller}")]
    [ApiController]
    public class ConfigController: Controller
    {
        IConfiguration _configufation;
        public ConfigController(IConfiguration configuration)
        {
            _configufation = configuration;
        }
        [HttpGet("")]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var sb = new StringBuilder();
            foreach (var child in _configufation.GetChildren())
            {
                sb.AppendLine($"{child.Key} = {child.Value}");
            }
            return Content(sb.ToString());
        }
    }
}
