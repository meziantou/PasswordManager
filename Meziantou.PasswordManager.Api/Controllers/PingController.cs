using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meziantou.PasswordManager.Api.Controllers
{
    [Controller]
    public class PingController : Controller
    {
        [HttpGet]
        [Route("ping")]
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
