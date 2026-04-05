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
    public class SlotMasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IHubContext<SettingsHub> _settingsHub;

        public SlotMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IHubContext<SettingsHub> settingsHub)
            : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _settingsHub = settingsHub;
        }

        [HttpPost]
        [Route("GetSlotList")]
        public IActionResult GetSlotList([FromBody] GetSlotRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetSlotResponse> list = new List<GetSlotResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetSlotList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@IsActive", model.IsActive);

                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new GetSlotResponse
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        Name = Convert.ToString(row["Name"]),
                        Code = Convert.ToString(row["Code"]),
                        StartTime = Convert.ToString(row["StartTime"]),
                        EndTime = Convert.ToString(row["EndTime"]),
                        StartTime2 = Convert.ToString(row["StartTime2"]),
                        EndTime2 = Convert.ToString(row["EndTime2"]),

                        StartTimeHH = Convert.ToString(row["StartTimeHH"]),
                        StartTimeMM = Convert.ToString(row["StartTimeMM"]),
                        StartTimeAMPM = Convert.ToString(row["StartTimeAMPM"]),

                        EndTimeHH = Convert.ToString(row["EndTimeHH"]),
                        EndTimeMM = Convert.ToString(row["EndTimeMM"]),
                        EndTimeAMPM = Convert.ToString(row["EndTimeAMPM"]),


                        IsActive = Convert.ToBoolean(row["IsActive"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Slots fetched successfully!";
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

        [HttpPost]
        [Route("SaveSlot")]
        public IActionResult SaveSlot([FromBody] SaveSlotBulkRequest model)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveSlot", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    string jsonSlots = Newtonsoft.Json.JsonConvert.SerializeObject(model.Slots);

                    cmd.Parameters.AddWithValue("@SlotJson", jsonSlots);
                    cmd.Parameters.AddWithValue("@Action_By", model.ActionBy);

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            response.status = reader["Status"].ToString();
                            response.message = reader["Message"].ToString();
                        }
                    }
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Error";
                response.message = ex.Message;
                return StatusCode(500, response);
            }
        }
    }
}

