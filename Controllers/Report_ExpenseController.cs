using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class Report_ExpenseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
