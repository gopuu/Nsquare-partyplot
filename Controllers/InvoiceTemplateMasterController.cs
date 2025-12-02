using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class InvoiceTemplateMasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
