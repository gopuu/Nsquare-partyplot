using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
