using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class SupportTicketPriorityController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
