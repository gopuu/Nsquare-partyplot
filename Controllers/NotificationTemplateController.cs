using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class NotificationTemplateController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
