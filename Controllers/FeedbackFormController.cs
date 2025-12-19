using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class FeedbackFormController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
