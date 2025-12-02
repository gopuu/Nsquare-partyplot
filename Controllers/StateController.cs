using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class StateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
