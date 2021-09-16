using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Iis.Api.Controllers
{
    [Route("api/login")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        [Authorize]
        [HttpGet]
        public string Login()
        {
            return User.Identity.Name;
        }
    }
}