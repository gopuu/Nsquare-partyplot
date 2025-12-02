using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class BookingCalendarController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
