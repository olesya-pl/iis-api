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
        IConfiguration _configuration;
        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet("")]
        public async Task<IActionResult> Get(CancellationToken token)
        {
            var sb = new StringBuilder();
            Format(sb, _configuration.GetChildren());
            return Content(sb.ToString());
        }

        private static void Format(StringBuilder sb, IEnumerable<IConfigurationSection> sections)
        {
            foreach (IConfigurationSection section in sections)
            {
                if (section.Value != null)
                {
                    sb.AppendLine($"{section.Path} = {section.Value}");
                }

                Format(sb, section.GetChildren());
            }
        }
    }
}
