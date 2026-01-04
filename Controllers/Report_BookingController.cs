using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class Report_BookingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
