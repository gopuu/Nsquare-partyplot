using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class StaffAttendanceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
