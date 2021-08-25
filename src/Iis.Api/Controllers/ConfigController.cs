using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public Task<IActionResult> Get(CancellationToken token)
        {
            var sb = new StringBuilder();
            Format(sb, _configufation.GetChildren());
            return Task.FromResult<IActionResult>(Content(sb.ToString()));
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
