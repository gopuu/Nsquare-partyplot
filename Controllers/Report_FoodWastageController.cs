using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class Report_FoodWastageController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
