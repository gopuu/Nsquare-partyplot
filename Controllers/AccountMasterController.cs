using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class AccountMasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
