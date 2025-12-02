using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
