using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class Report_MonthlySalesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
