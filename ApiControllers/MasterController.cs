using ERPAdmin.Helper;
using ERPAdmin.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        protected readonly ConnClass _connClass;
        protected readonly IConfiguration _configuration;

        public MasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            _connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        protected bool TryGetConnectionAndDomain(
            ControllerContext controllerContext,
            HttpRequest request,
            out string connString,
            out string domain,
            out IActionResult? errorResult)
        {
            connString = _connClass.GetChildConnectionString();
            domain = _connClass.GetDomainName();
            errorResult = null;

           var apiResponse = new apiResponse();

            // Validation Check
            if (string.IsNullOrEmpty(connString) || string.IsNullOrEmpty(domain))
            {
                string errorLog = _connClass.GetChildErrorLog();
               ErrorLogger.LogException(new Exception(errorLog), controllerContext, request, connString);

                // Create response
                apiResponse.message = errorLog;
                apiResponse.status = "Failed";

                errorResult = new OkObjectResult(apiResponse);

                return false;
            }

            return true;
        }

    }

}
