using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class WalletController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
