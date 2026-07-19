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

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingCalendar_GuestController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        public BookingCalendar_GuestController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetBookingCalendar_Guest")]
        public IActionResult GetBookingCalendar_Guest([FromBody] GetBookingCalendar_GuestRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<BookingCalendarGuestModel> guests = new List<BookingCalendarGuestModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingCalendar_Guest", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);

                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    guests.Add(new BookingCalendarGuestModel
                    {
                        Id = Convert.ToInt64(row["Id"]),
                        Name = Convert.ToString(row["Name"]),
                        MobileNo = Convert.ToString(row["MobileNo"]),
                        WhatsappContact = Convert.ToString(row["WhatsappContact"]),
                        Email = Convert.ToString(row["Email"]),
                        Address = Convert.ToString(row["Address"]),

                        CityId = row["CityId"] == DBNull.Value ? 0 : Convert.ToInt64(row["CityId"]),
                        CityName = Convert.ToString(row["CityName"]),

                        Dob = Convert.ToString(row["Dob"]),
                        Dob2 = Convert.ToString(row["Dob2"]),

                        AnniversaryDate = Convert.ToString(row["AnniversaryDate"]),
                        AnniversaryDate2 = Convert.ToString(row["AnniversaryDate2"]),

                        BookingId = row["BookingId"] == DBNull.Value ? 0 : Convert.ToInt64(row["BookingId"])
                    });
                }

                apiResponse.message = "Guest list fetched successfully!";
                apiResponse.data = guests;
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null ?? "");

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveBookingCalendar_Guest")]
        public IActionResult SaveBookingCalendar_Guest([FromBody] SaveBookingCalendar_GuestRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long rowId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBookingCalendar_Guest", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WhatsappContact", model.WhatsappContact ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", model.Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", model.Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@CityId", model.CityId == 0 ? DBNull.Value : (object)model.CityId);
                    cmd.Parameters.AddWithValue("@Dob", string.IsNullOrWhiteSpace(model.Dob) ? DBNull.Value : (object)model.Dob);
                    cmd.Parameters.AddWithValue("@AnniversaryDate", string.IsNullOrWhiteSpace(model.AnniversaryDate) ? DBNull.Value : (object)model.AnniversaryDate);
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId == 0 ? DBNull.Value : (object)model.BookingId);
                    cmd.Parameters.AddWithValue("@Action_By", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.message = model.Id > 0
                    ? "Guest updated successfully!"
                    : "Guest inserted successfully!";

                apiResponse.data = new
                {
                    GuestId = rowId
                };

                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteBookingCalendar_Guest")]
        public IActionResult DeleteBookingCalendar_Guest([FromBody] DeleteBookingCalendar_GuestRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteBookingCalendar_Guest", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.message = "Guest deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

    }
}
