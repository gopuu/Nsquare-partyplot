using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class ExpenseTransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
