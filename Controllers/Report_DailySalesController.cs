using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class Report_DailySalesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
