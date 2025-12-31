using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class FeedbackDashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
