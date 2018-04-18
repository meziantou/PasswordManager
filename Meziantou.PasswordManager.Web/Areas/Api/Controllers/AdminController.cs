using Meziantou.PasswordManager.Web.Areas.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Meziantou.PasswordManager.Web.Areas.Api.Controllers
{
    [Area(Constants.ApiArea)]
    public class AdminController : Controller
    {
        private readonly PasswordManagerDatabase _passwordManagerDatabase;

        public AdminController(PasswordManagerDatabase passwordManagerDatabase)
        {
            _passwordManagerDatabase = passwordManagerDatabase ?? throw new System.ArgumentNullException(nameof(passwordManagerDatabase));
        }

        [HttpGet]
        public IActionResult Reload()
        {
            _passwordManagerDatabase.Load();
            return Ok();
        }
    }
}
