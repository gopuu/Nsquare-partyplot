using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class CustomerInvoiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
