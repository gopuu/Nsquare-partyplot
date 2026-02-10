using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Web.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IHubContext<SettingsHub> _settingsHub;

        public SettingsController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHubContext<SettingsHub> settingsHub) 
            : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _settingsHub = settingsHub;
        }

        [HttpPost("GetSettings")]
        public IActionResult GetSettings(GetSettingsRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetSettingsResponse> list = new List<GetSettingsResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSettings", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new GetSettingsResponse
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        Name = Convert.ToString(row["Name"]),
                        Value = Convert.ToString(row["Value"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Settings fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("SaveSettings")]
        public async Task<IActionResult> SaveSettings([FromBody] SaveSettingsRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveSettings", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Value", model.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                }

                apiResponse.status = Convert.ToString(dt.Rows[0]["Status"]);
                apiResponse.message = Convert.ToString(dt.Rows[0]["Message"]);
                apiResponse.data = null;

                // 🔔 Notify only on SUCCESS
                if (apiResponse.status == "Success")
                {
                    await _settingsHub.Clients.All.SendAsync("OnSettingsChanged");
                }
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost("DeleteSettings")]
        public IActionResult DeleteSettings([FromBody] DeleteSettingsRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteSettings", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                }

                apiResponse.status = Convert.ToString(dt.Rows[0]["Status"]);
                apiResponse.message = Convert.ToString(dt.Rows[0]["Message"]);
                apiResponse.data = null;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

    }
}
