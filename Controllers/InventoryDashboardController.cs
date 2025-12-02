using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class InventoryDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
