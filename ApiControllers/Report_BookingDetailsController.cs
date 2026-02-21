using ERPAdmin.Helper;
using ERPAdmin.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using ERPAdmin.Models;


namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Report_BookingDetailsController : MasterController
    {

        private readonly string domainUrl;
        private string connString = string.Empty;

        public Report_BookingDetailsController(IConfiguration configuration, ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor)
               : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
                        
        }

        [HttpPost]
        [Route("Report/GetBookingDetails")]
        public IActionResult GetBookingDetailsReport([FromBody] BookingReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetBookingDetails", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@EventTypeId", SqlDbType.BigInt).Value = request.EventTypeId;
                    cmd.Parameters.Add("@EventSubTypeId", SqlDbType.BigInt).Value = request.EventSubTypeId;
                    cmd.Parameters.Add("@HallId", SqlDbType.BigInt).Value = request.HallId;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(ds); // 🔹 Table[0] = Headers, Table[1] = Data
                    }
                }

                response.status = "Success";
                response.message = "Booking report fetched successfully";
                response.data = new
                {
                    Headers = ds.Tables.Count > 0 ? ds.Tables[0] : null,
                    Rows = ds.Tables.Count > 1 ? ds.Tables[1] : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                response.status = "Error";
                response.message = ex.Message;
                response.data = null;

                return StatusCode(500, response);
            }
        }

    }
}
