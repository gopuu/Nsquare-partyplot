using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class IncomeTransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
