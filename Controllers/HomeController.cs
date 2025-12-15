using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace ERPAdmin.Controllers
{
    public class HomeController : Controller
    {
        //private readonly IConfiguration _configuration;
        //private readonly ILogger<HomeController> _logger;

        public HomeController() //(ILogger<HomeController> logger, IConfiguration configuration)
        {
            //_logger = logger;
            //_configuration = configuration;
        }

        public IActionResult Index()
        {
            // Set no-cache headers to prevent navigating back to the login page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "-1";
            return View();
        }

        public IActionResult Dashboard()
        {
            //string sesUserId = HttpContext.Session.GetString("User_Id");
            //if (string.IsNullOrEmpty(sesUserId))
            //{
            //    return RedirectToAction("Index");
            //}
            // Set no-cache headers to prevent navigating back to the login page
            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "-1";
            return View();
        }

    }

}
