using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using Humanizer;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingCalendar_VendorWorkOrder_ServiceController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;

        public BookingCalendar_VendorWorkOrder_ServiceController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}";

            // Force HTTPS if needed
            if (!domainUrl.Contains("localhost") && domainUrl.StartsWith("http://") && !domainUrl.Contains("tenxsoftwarek-001-site1"))
            {
                domainUrl = domainUrl.Replace("http://", "https://");
            }
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetBookingCalendar_VendorWorkOrder_Service")]
        public IActionResult GetBookingCalendar_VendorWorkOrder_Service([FromBody] GetVendorWorkOrderServiceRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<VendorWorkOrderServiceResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingCalendar_VendorWorkOrder_Service", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id ?? 0);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new VendorWorkOrderServiceResponse
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                Name = Convert.ToString(dr["Name"])
                            });
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Vendor work order services fetched successfully.";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveBookingCalendar_VendorWorkOrder_Service")]
        public IActionResult SaveBookingCalendar_VendorWorkOrder_Service(
            [FromBody] SaveVendorWorkOrderServiceRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBookingCalendar_VendorWorkOrder_Service", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                apiResponse.status = Convert.ToString(dt.Rows[0]["Status"]);
                apiResponse.message = apiResponse.status == "Success"
                    ? "Service saved successfully."
                    : Convert.ToString(dt.Rows[0]["Message"]);

                apiResponse.data = dt;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteBookingCalendar_VendorWorkOrder_Service")]
        public IActionResult DeleteBookingCalendar_VendorWorkOrder_Service(
            [FromBody] DeleteVendorWorkOrderServiceRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteBookingCalendar_VendorWorkOrder_Service", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                    con.Close();
                }

                apiResponse.status = Convert.ToString(dt.Rows[0]["Status"]);
                apiResponse.message = Convert.ToString(dt.Rows[0]["Message"]);
                apiResponse.data = dt;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

    }
}
