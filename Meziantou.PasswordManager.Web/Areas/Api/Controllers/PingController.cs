using Microsoft.AspNetCore.Mvc;

namespace Meziantou.PasswordManager.Web.Areas.Api.Controllers
{
    [Area(Constants.ApiArea)]
    public class PingController : Controller
    {
        public IActionResult Index()
        {
            return Ok("Pong");
        }
    }
}
