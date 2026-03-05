using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class AssetLedgerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
