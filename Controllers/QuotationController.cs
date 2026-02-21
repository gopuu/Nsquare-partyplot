using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class QuotationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
