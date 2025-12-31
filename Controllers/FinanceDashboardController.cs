using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class FinanceDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
