using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class SupportTicketController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
