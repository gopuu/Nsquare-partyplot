using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class TaskMasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
