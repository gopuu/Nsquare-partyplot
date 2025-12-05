using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class StaffDocumentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
