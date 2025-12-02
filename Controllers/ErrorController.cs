using Microsoft.AspNetCore.Mvc;

namespace ERPAdmin.Controllers
{
    public class ErrorController : Controller
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}
        [HttpGet]
        [Route("Error/General")]
        public IActionResult General()
        {
            return View();
        }
        [HttpGet]
        [Route("Error/404")]
        public IActionResult NotFound()
        {
            return View();
        }
        [HttpGet]
        [Route("Error/500")]
        public IActionResult InternalError()
        {
            return View();
        }
    }
}
