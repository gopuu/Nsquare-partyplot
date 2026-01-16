using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class HelpSupportController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
