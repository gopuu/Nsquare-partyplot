using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class EnquiryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
