using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class AllInOneMasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
