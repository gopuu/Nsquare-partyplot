using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class SupplierController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
