using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;

namespace ERPAdmin.Controllers
{
    public class DesignationController : Controller
    {
        public IActionResult Index()
        {
            //string sesUserId = HttpContext.Session.GetString("User_Id");
            //if (String.IsNullOrEmpty(sesUserId) || sesUserId == "0")
            //{
            //    return RedirectToAction("Index", "Home");
            //}
            return View();
        }
    }
}
