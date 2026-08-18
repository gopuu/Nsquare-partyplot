using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class Report_Expense_SummaryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
