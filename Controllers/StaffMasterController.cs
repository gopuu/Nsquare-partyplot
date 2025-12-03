using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class StaffMasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
