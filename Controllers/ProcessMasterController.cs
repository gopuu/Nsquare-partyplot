using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class ProcessMasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
