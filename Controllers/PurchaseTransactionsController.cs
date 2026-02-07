using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class PurchaseTransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
