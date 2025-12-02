using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class ExpenceTransactionsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
