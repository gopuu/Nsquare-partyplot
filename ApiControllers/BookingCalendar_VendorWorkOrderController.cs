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
    public class BookingCalendar_VendorWorkOrderController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;

        public BookingCalendar_VendorWorkOrderController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
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
        [Route("GetBookingCalendar_VendorWorkOrder")]
        public IActionResult GetBookingCalendar_VendorWorkOrder([FromBody] GetVendorWorkOrderRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetVendorWorkOrderResponse> list = new List<GetVendorWorkOrderResponse>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingCalendar_VendorWorkOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id ?? 0);
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId ?? 0);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var obj = new GetVendorWorkOrderResponse
                            {
                                Id = reader["Id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Id"]),
                                BookingId = reader["BookingId"] == DBNull.Value ? 0 : Convert.ToInt64(reader["BookingId"]),
                                ServiceId = reader["ServiceId"] == DBNull.Value ? 0 : Convert.ToInt64(reader["ServiceId"]),
                                VendorId = reader["VendorId"] == DBNull.Value ? 0 : Convert.ToInt64(reader["VendorId"]),
                                ConcernPerson = reader["ConcernPerson"] == DBNull.Value ? null : Convert.ToString(reader["ConcernPerson"]),
                                PODate = reader["PODate"] == DBNull.Value ? null : Convert.ToDateTime(reader["PODate"]).ToString("yyyy-MM-dd"),
                                EventDate = reader["EventDate"] == DBNull.Value ? null : Convert.ToDateTime(reader["EventDate"]).ToString("yyyy-MM-dd"),
                                EventTime = reader["EventTime"] == DBNull.Value ? null : reader["EventTime"].ToString(),
                                CustomerId = reader["CustomerId"] == DBNull.Value ? null : Convert.ToInt64(reader["CustomerId"]),
                                SlotId = reader["SlotId"] == DBNull.Value ? null : Convert.ToInt64(reader["SlotId"]),
                                Note = reader["Note"] == DBNull.Value ? "" : Convert.ToString(reader["Note"]),

                                // 🔑 JSON → List
                                Particulars = reader["ParticularsJson"] == DBNull.Value
                                    ? new List<GetVendorWorkOrderParticular>()
                                    : JsonConvert.DeserializeObject<List<GetVendorWorkOrderParticular>>(
                                        Convert.ToString(reader["ParticularsJson"]))
                            };

                            list.Add(obj);
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Vendor work orders fetched successfully!";
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
        [Route("SaveBookingCalendar_VendorWorkOrder")]
        public IActionResult SaveBookingCalendar_VendorWorkOrder([FromBody] SaveVendorWorkOrderRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long vendorWorkOrderId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBookingCalendar_VendorWorkOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                    cmd.Parameters.AddWithValue("@ServiceId", model.ServiceId);
                    cmd.Parameters.AddWithValue("@VendorId", model.VendorId);
                    cmd.Parameters.AddWithValue("@ConcernPerson", model.ConcernPerson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PODate", model.PODate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventDate", model.EventDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventTime", model.EventTime ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerId", model.CustomerId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SlotId", model.SlotId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Note", model.Note ?? (object)DBNull.Value);

                    // 🔑 JSON particulars
                    cmd.Parameters.AddWithValue("@ParticularsJson",
                        model.Particulars != null
                            ? JsonConvert.SerializeObject(model.Particulars)
                            : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    vendorWorkOrderId = Convert.ToInt64(cmd.ExecuteScalar());
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Vendor work order updated successfully!"
                    : "Vendor work order created successfully!";
                apiResponse.data = new { VendorWorkOrderId = vendorWorkOrderId };

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
        [Route("DeleteBookingCalendar_VendorWorkOrder")]
        public IActionResult DeleteBookingCalendar_VendorWorkOrder([FromBody] DeleteVendorWorkOrderRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteBookingCalendar_VendorWorkOrder", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Vendor work order deleted successfully!";
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
        [Route("GetBookingCalendar_VendorWorkOrder_SlotMaster")]
        public IActionResult GetBookingCalendar_VendorWorkOrder_SlotMaster([FromBody] GetSlotMasterRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<SlotMasterResponse> list = new();

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

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingCalendar_VendorWorkOrder_SlotMaster", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id ?? 0);

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new SlotMasterResponse
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                Name = Convert.ToString(dr["Name"])
                            });
                        }
                    }
                    con.Close();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Slot master fetched successfully.";
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


    }
}
