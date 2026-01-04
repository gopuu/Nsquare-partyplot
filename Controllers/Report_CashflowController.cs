using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class Report_CashflowController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
