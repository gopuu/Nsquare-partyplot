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
    public class BookingCalendarController : MasterController
    {
        //private readonly IConfiguration _configuration;
        //private readonly ConnClass _connClass;
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;

        public BookingCalendarController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
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
        [Route("GetBookingCalendar")]
        public IActionResult GetBookingCalendar([FromBody] GetBookingCalendar request)
        {
            apiResponse apiResponse = new apiResponse();
            List<BookingModel> bookings = new List<BookingModel>();
            try
            {
                // 🔹 Validate and get connection string
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                // 🔹 Database setup
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingCalendar", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId == 0 ? 0 : request.BookingId);
                    cmd.Parameters.AddWithValue("@BanquetId", request.BanquetId == 0 ? 0 : request.BanquetId);
                    cmd.Parameters.AddWithValue("@EventDate", request.EventDate ?? (object)DBNull.Value);

                    con.Open();
                    DataTable dataTable = new DataTable();
                    SqlDataReader reader = cmd.ExecuteReader();
                    dataTable.Load(reader);
                    con.Close();

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        BookingModel booking = new BookingModel();
                        booking.BookingId = dr["BookingId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["BookingId"]);
                        booking.InvoiceNo = dr["InvoiceNo"] == DBNull.Value ? 0 : Convert.ToInt64(dr["InvoiceNo"]);
                        booking.EventDate = dr["EventDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["EventDate"]).ToString("yyyy-MM-dd");
                        booking.EventDate_Format = dr["EventDate_Format"] == DBNull.Value ? "" : Convert.ToString(dr["EventDate_Format"]);
                        booking.ShiftCode = dr["ShiftCode"] == DBNull.Value ? "" : Convert.ToString(dr["ShiftCode"]);
                        booking.SlotId = dr["SlotId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["SlotId"]);
                        booking.GSTAccepted = dr["GSTAccepted"] != DBNull.Value && Convert.ToBoolean(dr["GSTAccepted"]);
                        booking.BanquetId = dr["BanquetId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["BanquetId"]);
                        booking.BanquetName = dr["BanquetName"] == DBNull.Value ? "" : Convert.ToString(dr["BanquetName"]);
                        booking.BanquetPropertyName = dr["BanquetPropertyName"] == DBNull.Value ? "" : Convert.ToString(dr["BanquetPropertyName"]);
                        booking.FromDate = dr["FromDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["FromDate"]).ToString("yyyy-MM-dd");
                        booking.FromDate_Full = dr["FromDate_Full"] == DBNull.Value ? "" : Convert.ToString(dr["FromDate_Full"]);
                        booking.FromDate_Short = dr["FromDate_Short"] == DBNull.Value ? "" : Convert.ToString(dr["FromDate_Short"]);
                        booking.TillDate = dr["TillDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["TillDate"]).ToString("yyyy-MM-dd");
                        booking.TillDate_Full = dr["TillDate_Full"] == DBNull.Value ? "" : Convert.ToString(dr["TillDate_Full"]);
                        booking.TillDate_Short = dr["TillDate_Short"] == DBNull.Value ? "" : Convert.ToString(dr["TillDate_Short"]);
                        booking.StartTime = dr["StartTime"] == DBNull.Value ? "" : Convert.ToString(dr["StartTime"]);
                        booking.StartTime_Full = dr["StartTime_Full"] == DBNull.Value ? "" : Convert.ToString(dr["StartTime_Full"]);
                        booking.EndTime = dr["EndTime"] == DBNull.Value ? "" : Convert.ToString(dr["EndTime"]);
                        booking.EndTime_Full = dr["EndTime_Full"] == DBNull.Value ? "" : Convert.ToString(dr["EndTime_Full"]);
                        booking.Client_EnquiryId = dr["Client_EnquiryId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Client_EnquiryId"]);
                        booking.Client_Name = dr["Client_Name"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Name"]);
                        booking.Client_Address = dr["Client_Address"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Address"]);
                        booking.Client_Email = dr["Client_Email"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Email"]);
                        booking.Client_Contact = dr["Client_Contact"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Contact"]);
                        booking.Client_GSTIN = dr["Client_GSTIN"] == DBNull.Value ? "" : Convert.ToString(dr["Client_GSTIN"]);
                        booking.Client_StateId = dr["Client_StateId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Client_StateId"]);
                        booking.Booking_SubEventId = dr["Booking_SubEventId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_SubEventId"]);
                        booking.Booking_SubEventName = dr["Booking_SubEventName"] == DBNull.Value ? "" : Convert.ToString(dr["Booking_SubEventName"]);
                        booking.Booking_PAX = dr["Booking_PAX"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Booking_PAX"]);
                        booking.Booking_HallRent = dr["Booking_HallRent"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Booking_HallRent"]);
                        booking.Booking_GST = dr["Booking_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Booking_GST"]);
                        booking.Booking_LeadReferenceId = dr["Booking_LeadReferenceId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_LeadReferenceId"]);
                        booking.Booking_MenuType_Id = dr["Booking_MenuType_Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_MenuType_Id"]);
                        booking.Booking_TypeId = dr["Booking_TypeId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_TypeId"]);
                        booking.Booking_TypeName = dr["Booking_TypeName"] == DBNull.Value ? "" : Convert.ToString(dr["Booking_TypeName"]);
                        booking.Booking_IsAgent = dr["Booking_IsAgent"] != DBNull.Value && Convert.ToBoolean(dr["Booking_IsAgent"]);
                        booking.Payment_ModeId = dr["Payment_ModeId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Payment_ModeId"]);
                        booking.Payment_Amount = dr["Payment_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Payment_Amount"]);
                        booking.Payment_PaidTime = dr["Payment_PaidTime"] == DBNull.Value ? "" : Convert.ToString(dr["Payment_PaidTime"]);
                        booking.Payment_DueDate = dr["Payment_DueDate"] == DBNull.Value ? "" : Convert.ToString(dr["Payment_DueDate"]);
                        booking.Payment_Note = dr["Payment_Note"] == DBNull.Value ? "" : Convert.ToString(dr["Payment_Note"]);
                        booking.OtherCharges_Decoration_Amount = dr["OtherCharges_Decoration_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_Decoration_Amount"]);
                        booking.OtherCharges_Decoration_GST = dr["OtherCharges_Decoration_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_Decoration_GST"]);
                        booking.OtherCharges_RoomCharges_Amount = dr["OtherCharges_RoomCharges_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_RoomCharges_Amount"]);
                        booking.OtherCharges_RoomCharges_GST = dr["OtherCharges_RoomCharges_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_RoomCharges_GST"]);
                        booking.OtherCharges_SoundSystem_Amount = dr["OtherCharges_SoundSystem_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_SoundSystem_Amount"]);
                        booking.OtherCharges_SoundSystem_GST = dr["OtherCharges_SoundSystem_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_SoundSystem_GST"]);
                        booking.OtherCharges_ProjectorScreen_Amount = dr["OtherCharges_ProjectorScreen_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_ProjectorScreen_Amount"]);
                        booking.OtherCharges_ProjectorScreen_GST = dr["OtherCharges_ProjectorScreen_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_ProjectorScreen_GST"]);
                        booking.OtherCharges_BookingNoteForCustomer = dr["OtherCharges_BookingNoteForCustomer"] == DBNull.Value ? "" : Convert.ToString(dr["OtherCharges_BookingNoteForCustomer"]);
                        booking.OtherCharges_InternalOfficeNote = dr["OtherCharges_InternalOfficeNote"] == DBNull.Value ? "" : Convert.ToString(dr["OtherCharges_InternalOfficeNote"]);
                        booking.PaymentSummary_TaxableAmount = dr["PaymentSummary_TaxableAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_TaxableAmount"]);
                        booking.PaymentSummary_CGST = dr["PaymentSummary_CGST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_CGST"]);
                        booking.PaymentSummary_SGST = dr["PaymentSummary_SGST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_SGST"]);
                        booking.PaymentSummary_IGST = dr["PaymentSummary_IGST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_IGST"]);
                        booking.PaymentSummary_Total = dr["PaymentSummary_Total"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_Total"]);
                        booking.PaymentSummary_BookingDiscount_Type_Id = dr["PaymentSummary_BookingDiscount_Type_Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["PaymentSummary_BookingDiscount_Type_Id"]);
                        booking.PaymentSummary_BookingDiscount_Type_Amount = dr["PaymentSummary_BookingDiscount_Type_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_BookingDiscount_Type_Amount"]);
                        booking.PaymentSummary_BookingDiscount_Total = dr["PaymentSummary_BookingDiscount_Total"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_BookingDiscount_Total"]);
                        booking.PaymentSummary_PayableAmount = dr["PaymentSummary_PayableAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_PayableAmount"]);
                        booking.Booked_By_EmpId = dr["Booked_By_EmpId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booked_By_EmpId"]);
                        booking.Booked_By_EmpName = dr["Booked_By_EmpName"] == DBNull.Value ? "" : Convert.ToString(dr["Booked_By_EmpName"]);
                        booking.Booked_On = dr["Booked_On"] == DBNull.Value ? "" : Convert.ToString(dr["Booked_On"]);
                        booking.JSON_FoodPackage = JsonConvert.DeserializeObject<List<GetFoodPackage>>(Convert.ToString(dr["FoodPackageJSON"]));
                        booking.VendorWorkOrder_Count = dr["VendorWorkOrder_Count"] == DBNull.Value ? 0 : Convert.ToInt64(dr["VendorWorkOrder_Count"]);

                        bookings.Add(booking);
                    }
                }

                // 🔹 Response
                apiResponse.message = "Bookings fetched successfully!";
                apiResponse.data = bookings;
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // Log error
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.data = null;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveBookingCalendar")]
        public IActionResult SaveBookingCalendar([FromBody] SaveBookingCalendar model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                // 🔹 Validate connection setup
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                long bookingId = 0;
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBookingCalendar", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // ✅ Add parameters safely
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                    cmd.Parameters.AddWithValue("@EventDate", model.EventDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftCode", model.ShiftCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GSTAccepted", model.GSTAccepted ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@BanquetId", model.BanquetId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TillDate", model.TillDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartTime", model.StartTime ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndTime", model.EndTime ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@Client_EnquiryId", model.Client_EnquiryId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Name", model.Client_Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Address", model.Client_Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Email", model.Client_Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Contact", model.Client_Contact ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_GSTIN", model.Client_GSTIN ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_StateId", model.Client_StateId ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@Booking_SubEventId", model.Booking_SubEventId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Booking_PAX", model.Booking_PAX ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Booking_HallRent", model.Booking_HallRent ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Booking_GST", model.Booking_GST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Booking_LeadReferenceId", model.Booking_LeadReferenceId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Booking_MenuType_Id", model.Booking_MenuType_Id ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Booking_TypeId", model.Booking_TypeId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Booking_IsAgent", model.Booking_IsAgent ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@Payment_ModeId", model.Payment_ModeId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_Amount", model.Payment_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_PaidTime", model.Payment_PaidTime ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_DueDate", model.Payment_DueDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_Note", model.Payment_Note ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@OtherCharges_Decoration_Amount", model.OtherCharges_Decoration_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_Decoration_GST", model.OtherCharges_Decoration_GST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_RoomCharges_Amount", model.OtherCharges_RoomCharges_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_RoomCharges_GST", model.OtherCharges_RoomCharges_GST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_SoundSystem_Amount", model.OtherCharges_SoundSystem_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_SoundSystem_GST", model.OtherCharges_SoundSystem_GST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_ProjectorScreen_Amount", model.OtherCharges_ProjectorScreen_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_ProjectorScreen_GST", model.OtherCharges_ProjectorScreen_GST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_BookingNoteForCustomer", model.OtherCharges_BookingNoteForCustomer ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_InternalOfficeNote", model.OtherCharges_InternalOfficeNote ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@PaymentSummary_TaxableAmount", model.PaymentSummary_TaxableAmount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_CGST", model.PaymentSummary_CGST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_SGST", model.PaymentSummary_SGST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_IGST", model.PaymentSummary_IGST ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_Total", model.PaymentSummary_Total ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_BookingDiscount_Type_Id", model.PaymentSummary_BookingDiscount_Type_Id ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_BookingDiscount_Type_Amount", model.PaymentSummary_BookingDiscount_Type_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_BookingDiscount_Total", model.PaymentSummary_BookingDiscount_Total ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentSummary_PayableAmount", model.PaymentSummary_PayableAmount ?? (object)DBNull.Value);

                    //cmd.Parameters.AddWithValue("@JSON_FoodPackage", model.JSON_FoodPackage ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JSON_FoodPackage",
                        model.JSON_FoodPackage != null ? JsonConvert.SerializeObject(model.JSON_FoodPackage) : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@Action_By", model.ActionBy);

                    // 🔹 Execute SP
                    con.Open();
                    //cmd.ExecuteNonQuery();
                    bookingId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.status = "Success";
                apiResponse.data= new { BookingId = bookingId };
                apiResponse.message = model.BookingId > 0
                    ? "Booking updated successfully!"
                    : "Booking inserted successfully!";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = "Error details: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteBookingCalendar")]
        public IActionResult DeleteBookingCalendar([FromBody] DeleteBookingCalendarRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                // 🔹 Validate and get connection string
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("DeleteBookingCalendar", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@bookingId", SqlDbType.BigInt).Value = model.BookingId;
                        cmd.Parameters.Add("@actionBy", SqlDbType.BigInt).Value = model.ActionBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                // 🔹 Success response
                apiResponse.message = "Booking deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // 🔹 Log and return error
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("CancelBookingCalendar")]
        public IActionResult CancelBookingCalendar([FromBody] CancelBookingCalendarRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                // 🔹 Validate connection and domain
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("CancelBookingCalendar", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@bookingId", SqlDbType.BigInt).Value = model.BookingId;
                        cmd.Parameters.Add("@cancelReason", SqlDbType.NVarChar).Value = model.CancelReason ?? (object)DBNull.Value;
                        cmd.Parameters.Add("@actionBy", SqlDbType.BigInt).Value = model.ActionBy;

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }

                // 🔹 Success Response
                apiResponse.message = "Booking cancelled successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // 🔹 Log error
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetPackage_MenuAndFoodItemsList")]
        public IActionResult GetPackage_MenuAndFoodItemsList([FromBody] GetPackage_MenuAndFoodItemsListRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<FoodMenuModel> menuList = new List<FoodMenuModel>();

            try
            {
                // 🔹 Validate and get connection string
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPackage_MenuAndFoodItemsList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@package_id", request.Package_Id ?? (object)DBNull.Value);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    con.Close();

                    foreach (DataRow dr in dt.Rows)
                    {
                        var menu = new FoodMenuModel
                        {
                            FoodMenu_Id = Convert.ToInt64(dr["FoodMenu_Id"]),
                            FoodMenu_Name = Convert.ToString(dr["FoodMenu_Name"]),
                            FoodItemsList = new List<FoodItemModel>()
                        };

                        // Parse JSON from SQL
                        string jsonFoodItems = Convert.ToString(dr["FoodItemsList"]);
                        if (!string.IsNullOrEmpty(jsonFoodItems))
                        {
                            menu.FoodItemsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FoodItemModel>>(jsonFoodItems);
                        }
                        menuList.Add(menu);
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Package menu and food items fetched successfully!";
                apiResponse.data = menuList;
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


        [HttpPost]
        [Route("GeneratePackageReport2")]
        public IActionResult GeneratePackageReport2([FromBody] PackageReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (request == null || request.BookingId <= 0)
                    return BadRequest(new { status = "Error", message = "Invalid Request" });

                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string conn, out string domain, out IActionResult error))
                    return error;

                //bool iswithprice = Convert.ToBoolean(request.IsWithPrice);

                // Now pass the bool
                string pdfPath = GeneratePDF2(domain, domainUrl, request.IsWithPrice);

                return Ok(new
                {
                    status = "Success",
                    data = new { FilePath = pdfPath }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }

        private string GeneratePDF2(string domain, string domainUrl, bool isWithPrice)
        {
            MemoryStream stream = new();
            Document doc = new(PageSize.A4, 30, 30, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, stream);

            doc.Open();
            // After doc.Open();
            PdfContentByte cb = writer.DirectContent;

            // Set border color
            cb.SetColorStroke(BaseColor.BLACK);

            // Define padding inside the margins
            float padding = 15f;

            // Coordinates for rectangle with padding
            float llx = doc.LeftMargin - padding;              // left x
            float lly = doc.BottomMargin - padding;           // bottom y
            float urx = doc.PageSize.Width - doc.RightMargin + padding;  // right x
            float ury = doc.PageSize.Height - doc.TopMargin + padding;   // top y

            // Draw border
            cb.Rectangle(llx, lly, urx - llx, ury - lly);
            cb.Stroke();


            // ========== HEADER TABLE ==========
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 1f, 3f });

            // ---- Logo ----
            string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", "banquatlogo.png");

            if (System.IO.File.Exists(logoPath))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80, 80);

                PdfPCell logoCell = new PdfPCell(logo);
                logoCell.Border = Rectangle.NO_BORDER;
                logoCell.HorizontalAlignment = Element.ALIGN_LEFT;

                headerTable.AddCell(logoCell);
            }
            else
            {
                PdfPCell emptyCell = new PdfPCell(new Phrase(""));
                emptyCell.Border = Rectangle.NO_BORDER;
                headerTable.AddCell(emptyCell);
            }

            // ---- Company Details (Right Side) ----
            Font headerFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Create inner table (1 column)
            PdfPTable companyTable = new PdfPTable(1);
            companyTable.WidthPercentage = 100;

            // Line 1
            PdfPCell line1 = new PdfPCell(new Phrase("Ramayana Restro & Banquets", headerFontBold));
            line1.Border = Rectangle.NO_BORDER;
            line1.PaddingBottom = 6;  // <-- Extra line spacing
            line1.HorizontalAlignment = Element.ALIGN_RIGHT;
            companyTable.AddCell(line1);

            // Line 2
            PdfPCell line2 = new PdfPCell(new Phrase("1st Floor, Crossway Mall, Above Croma, Near Ramchowk, Ghoddod Road, Surat", headerFont));
            line2.Border = Rectangle.NO_BORDER;
            line2.PaddingBottom = 6;
            line2.HorizontalAlignment = Element.ALIGN_RIGHT;
            companyTable.AddCell(line2);

            // Line 3
            PdfPCell line3 = new PdfPCell(new Phrase("State: Gujarat", headerFont));
            line3.Border = Rectangle.NO_BORDER;
            line3.HorizontalAlignment = Element.ALIGN_RIGHT;
            companyTable.AddCell(line3);

            // Add table to main header cell
            PdfPCell textCell = new PdfPCell(companyTable);
            textCell.Border = Rectangle.NO_BORDER;
            textCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            textCell.VerticalAlignment = Element.ALIGN_MIDDLE;
            headerTable.AddCell(textCell);
            // Add to document  
            doc.Add(headerTable);
           // doc.Add(new Paragraph("\n"));

            // ===== FIRST FULL WIDTH LINE =====
            PdfPTable lineTop = new PdfPTable(1);
            lineTop.WidthPercentage = 100;

            PdfPCell topLineCell = new PdfPCell();
            topLineCell.Border = Rectangle.NO_BORDER;
            topLineCell.FixedHeight = 1; // thickness
            topLineCell.BackgroundColor = new BaseColor(0, 0, 0);

            lineTop.AddCell(topLineCell);
            doc.Add(lineTop);

            // ===== TITLE CENTERED IN BETWEEN =====
            Paragraph title = new Paragraph("Function Prospectus",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14));

            title.Alignment = Element.ALIGN_CENTER;
            title.SetLeading(0, 1.0f); // keeps title tight with line
            title.SpacingBefore = 3;  // pull title up closer to top line
            title.SpacingAfter = 8;  

            doc.Add(title);

            // ===== SECOND FULL WIDTH LINE =====
            PdfPTable lineBottom = new PdfPTable(1);
            lineBottom.WidthPercentage = 100;

            PdfPCell bottomLineCell = new PdfPCell();
            bottomLineCell.Border = Rectangle.NO_BORDER;
            bottomLineCell.FixedHeight = 1;
            bottomLineCell.BackgroundColor = new BaseColor(0, 0, 0);

            lineBottom.AddCell(bottomLineCell);
            doc.Add(lineBottom);

            // Extra space after title section
           // doc.Add(new Paragraph("\n"));

            // ===== EVENT DETAILS SECTION =====

            // Fonts
            Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            // Create 2-column layout table
            PdfPTable detailsTable = new PdfPTable(2);
            detailsTable.WidthPercentage = 100;
            detailsTable.SetWidths(new float[] { 1f, 1f }); // equal width

            //--------------------------------------------------
            // LEFT SIDE (Bold labels + value normal)
            //--------------------------------------------------
            PdfPTable leftTable = new PdfPTable(1);
            leftTable.WidthPercentage = 100;

            void AddLeftRow(string label, string value)
            {
                Phrase p = new Phrase();
                p.Add(new Chunk(label + " : ", labelFont));
                p.Add(new Chunk(value, valueFont));

                PdfPCell cell = new PdfPCell(p);
                cell.Border = Rectangle.NO_BORDER;
                cell.PaddingBottom = 6;
                cell.PaddingLeft = 4;

                leftTable.AddCell(cell);
            }

            // Add left side rows
            AddLeftRow("Event Details", "");
            AddLeftRow("Booking Ref#", "84517");
            AddLeftRow("Client Name", "MR. BREHAT SEHGAL");
            AddLeftRow("Contact Number", "7990754365");
            AddLeftRow("Event Date", "03-12-2025");
            AddLeftRow("Property", "Ramayana Restro & Banquets");
            AddLeftRow("Pax", "65");
            AddLeftRow("Hall", "RAMA");
            AddLeftRow("Slot", "Evening");
            AddLeftRow("Timing", "07:00 PM - 11:30 PM");

            //--------------------------------------------------
            // RIGHT SIDE (Normal text only)
            //--------------------------------------------------

            PdfPTable rightTable = new PdfPTable(1);
            rightTable.WidthPercentage = 100;

            void AddRightRow(string label, string value)
            {
                Phrase p = new Phrase();
                p.Add(new Chunk(label + " : ", valueFont));
                p.Add(new Chunk(value, valueFont));

                PdfPCell cell = new PdfPCell(p);
                cell.Border = Rectangle.NO_BORDER;
                cell.PaddingBottom = 6;
                cell.PaddingLeft = 4;

                rightTable.AddCell(cell);
            }

            // Add right side rows
            AddRightRow("Booked on", "20-07-2025");
            AddRightRow("Event", "Engagement");
            AddRightRow("Company Name", "Shreenidhi Enterprise");
            AddRightRow("Company Address", "1st Floor, Crosswaymall, Above Croma, Near Ramchowk, Ghoddod Road, Surat");
            AddRightRow("Email", "ramayanarestroandbanquet@gmail.com");
           
            //--------------------------------------------------
            // Add Left + Right table into main row
            //--------------------------------------------------

            PdfPCell leftCell = new PdfPCell(leftTable);
            leftCell.Border = Rectangle.NO_BORDER;
            leftCell.PaddingTop = 10;

            PdfPCell rightCell = new PdfPCell(rightTable);
            rightCell.Border = Rectangle.NO_BORDER;
            rightCell.PaddingTop = 10;
            rightCell.HorizontalAlignment = Element.ALIGN_RIGHT;

            detailsTable.AddCell(leftCell);
            detailsTable.AddCell(rightCell);

            // Add to document
            doc.Add(detailsTable);

            // Add spacing after block
            doc.Add(new Paragraph("\n"));

            // Fonts
            Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

            // ===== PACKAGE LINE =====
            Phrase packageLine = new Phrase();
            packageLine.Add(new Chunk("PRIMARY FOOD PACKAGE : ", normalFont));
            packageLine.Add(new Chunk("PACKAGE - 3@ ₹770 PER PLATE", boldFont));

            PdfPCell packageCell = new PdfPCell(packageLine);
            packageCell.Border = Rectangle.NO_BORDER;
            packageCell.PaddingTop = 6;
            packageCell.PaddingBottom = 6;
            packageCell.HorizontalAlignment = Element.ALIGN_LEFT;

            // Wrap in table for full width
            PdfPTable packageTable = new PdfPTable(1);
            packageTable.WidthPercentage = 100;
            packageTable.AddCell(packageCell);

            doc.Add(packageTable);

            // ===== MUKHWAS =====
            Phrase mukhwasPhrase = new Phrase();
            mukhwasPhrase.Add(new Chunk("Mukhwas  \n", boldFont));
            mukhwasPhrase.Add(new Chunk("Mukhwas", normalFont));

            PdfPCell mukhwasCell = new PdfPCell(mukhwasPhrase);
            mukhwasCell.Border = Rectangle.NO_BORDER;
            mukhwasCell.BackgroundColor = new BaseColor(230, 230, 230); // Light gray
            mukhwasCell.PaddingLeft = 10;
            mukhwasCell.PaddingTop = 8;
            mukhwasCell.PaddingBottom = 8;
            mukhwasCell.HorizontalAlignment = Element.ALIGN_LEFT;

            PdfPTable mukhwasTable = new PdfPTable(1);
            mukhwasTable.WidthPercentage = 100;
            mukhwasTable.AddCell(mukhwasCell);

            doc.Add(mukhwasTable);
            doc.Add(new Paragraph("\n"));

            // Fonts
            Font headerFont2 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);

            // Create table (9 columns as per your example)
            PdfPTable table = new PdfPTable(9);
            table.WidthPercentage = 100;

            // Optional: set relative widths
            table.SetWidths(new float[] { 1f, 2f, 2f, 2f, 1f, 1f, 2f, 2f, 2f });

            // Header titles
            string[] headers = new string[]
            {
                "Sr#", "Date", "Title", "Food Package", "Rate", "Pax OR Per", "Package Amount", "Chargeable Addon Items", "Total"
            };

            // Add header cells
            foreach (var h in headers)
            {
                PdfPCell cell = new PdfPCell(new Phrase(h, headerFont2));
                cell.BackgroundColor = new BaseColor(82, 86, 88); // Dark gray
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.Padding = 5;
                table.AddCell(cell);
            }

            // Example row
            Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            // Example table row with padding
            table.AddCell(new PdfPCell(new Phrase("1", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("03-Dec-2025", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("Food", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("PACKAGE - 3", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("770", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("65", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("50,050", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("-", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            table.AddCell(new PdfPCell(new Phrase("50,050", rowFont))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 4,
                PaddingBottom = 4,
                PaddingLeft = 6,
                PaddingRight = 6
            });

            // Add table to document
            doc.Add(table);
            doc.Add(new Paragraph("\n"));
            // Fonts
            Font summaryFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            
            // Create table for 2 rows, 3 columns each
            PdfPTable summaryTable = new PdfPTable(3);
            summaryTable.WidthPercentage = 100;
            summaryTable.SetWidths(new float[] { 0.33f, 0.33f, 0.33f }); // equal width for left/center/right

            // ===== FIRST ROW =====
            summaryTable.AddCell(new PdfPCell(new Phrase("Deal Amount: ₹50,050", summaryFont))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(82, 86, 88),
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 6
            });

            summaryTable.AddCell(new PdfPCell(new Phrase("Booking Discount: ₹0", summaryFont))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(82, 86, 88),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 6
            });

            summaryTable.AddCell(new PdfPCell(new Phrase("Payable Amount: ₹50,050", summaryFont))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(82, 86, 88),
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 6
            });

            // ===== SECOND ROW =====
            summaryTable.AddCell(new PdfPCell(new Phrase("Total Paid: ₹5,100", summaryFont))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(82, 86, 88),
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 6
            });

            summaryTable.AddCell(new PdfPCell(new Phrase("Cash Discount: ₹0", summaryFont))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(82, 86, 88),
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 6
            });

            summaryTable.AddCell(new PdfPCell(new Phrase("Outstanding Amount: ₹44,950", summaryFont))
            {
                Border = Rectangle.NO_BORDER,
                BackgroundColor = new BaseColor(82, 86, 88),
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 6
            });
            // Add table to document
            doc.Add(summaryTable);
            doc.Add(new Paragraph("\n"));

            // Create table (1 column, full width)
            PdfPTable detailsTable2 = new PdfPTable(1);
            detailsTable2.WidthPercentage = 100;

            // Add each detail as separate cell for spacing control
            detailsTable2.AddCell(new PdfPCell(new Phrase("Updated By : Mr.Jitendra Saudiyal", labelFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingBottom = 4
            });

            detailsTable2.AddCell(new PdfPCell(new Phrase("Updated On : 20-Jul-2025 08:24 PM\n", labelFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingBottom = 4
            });

            detailsTable2.AddCell(new PdfPCell(new Phrase("Comment :\n", labelFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingBottom = 4
            });

            detailsTable2.AddCell(new PdfPCell(new Phrase("How did you reached us? : Walkin\n", labelFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingBottom = 4
            });

            detailsTable2.AddCell(new PdfPCell(new Phrase("Reference : Offline\n", labelFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingBottom = 4
            });

            detailsTable2.AddCell(new PdfPCell(new Phrase("TERMS & CONDITIONS", labelFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingTop = 6,
                PaddingBottom = 6
            });

            // Add table to document
            doc.Add(detailsTable2);
            doc.Add(new Paragraph("\n"));

            // Create table with 3 columns
            PdfPTable signTable = new PdfPTable(3);
            signTable.WidthPercentage = 100;
            signTable.SetWidths(new float[] { 0.33f, 0.33f, 0.33f }); // equal width

            // ---- First Column ----
            PdfPCell cell1 = new PdfPCell();
            cell1.Border = Rectangle.NO_BORDER;
            cell1.HorizontalAlignment = Element.ALIGN_CENTER;

            // Add line and label
            cell1.AddElement(new Phrase("__________________________", labelFont));
            cell1.AddElement(new Phrase("Sales Manager's Sign.", labelFont));

            signTable.AddCell(cell1);

            // ---- Second Column ----
            PdfPCell cell2 = new PdfPCell();
            cell2.Border = Rectangle.NO_BORDER;
            cell2.HorizontalAlignment = Element.ALIGN_CENTER;

            cell2.AddElement(new Phrase("__________________________", labelFont));
            cell2.AddElement(new Phrase("Banquet Manager's Sign.", labelFont));

            signTable.AddCell(cell2);

            // ---- Third Column ----
            PdfPCell cell3 = new PdfPCell();
            cell3.Border = Rectangle.NO_BORDER;
            cell3.HorizontalAlignment = Element.ALIGN_CENTER;

            cell3.AddElement(new Phrase("__________________________", labelFont));
            cell3.AddElement(new Phrase("Guest's Sign", labelFont));

            signTable.AddCell(cell3);

            // Add table to document
            doc.Add(signTable);

            //doc.Add(new Paragraph("PACKAGE DETAILS", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));

            //if (isWithPrice)
            //    doc.Add(new Paragraph("Price: ₹ 50,000", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
            //else
            //    doc.Add(new Paragraph("Price: -----------", FontFactory.GetFont(FontFactory.HELVETICA, 12)));

            doc.Close();
            writer.Close();

            string reportsDirPath = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "PackageReports");

            if (!Directory.Exists(reportsDirPath))
                Directory.CreateDirectory(reportsDirPath);
            string fileName = $"Package_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(reportsDirPath, fileName);
            System.IO.File.WriteAllBytes(filePath, stream.ToArray());

            return $"{domainUrl}/uploads/org/{domain}/PackageReports/{fileName}";
        }


        [HttpPost]
        [Route("GeneratePackageReport")]
        public IActionResult GeneratePackageReport([FromBody] PackageReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (request == null || request.BookingId <= 0)
                    return BadRequest(new { status = "Error", message = "Invalid Request" });

                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string conn, out string domain, out IActionResult error))
                    return error;

                // Fetch booking data
                BookingReportModel report = GetBookingReport(request.BookingId, conn);

                // Generate PDF
                string pdfPath = GeneratePDF(report, domain, request.IsWithPrice);

                return Ok(new
                {
                    status = "Success",
                    data = new { FilePath = pdfPath }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = "Error", message = ex.Message });
            }
        }

        // --------------------- Fetch Booking Data ---------------------
        private BookingReportModel GetBookingReport(long bookingId, string connString)
        {
            BookingReportModel report = new BookingReportModel();
            report.PackageItems = new List<PackageItem>();

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand("Report_GetBookingReportById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BookingId", bookingId);

                conn.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    // --- First Result Set: Booking Header ---
                    if (dr.Read())
                    {
                        report.BookingId = Convert.ToInt64(dr["BookingId"]);
                        report.BookingRef = dr["BookingRef"].ToString();
                        report.ClientName = dr["ClientName"].ToString();
                        report.ContactNumber = dr["ContactNumber"].ToString();
                        report.EventDate = dr["EventDate"].ToString();
                        report.Property = dr["Property"].ToString();
                        report.Pax = Convert.ToInt32(dr["Pax"]);
                        report.Hall = dr["Hall"].ToString();
                        report.Slot = dr["Slot"].ToString();
                        report.Timing = dr["Timing"].ToString();
                        report.BookedOn = dr["BookedOn"].ToString();
                        report.EventType = dr["EventType"].ToString();
                        report.CompanyName = dr["CompanyName"].ToString();
                        report.CompanyAddress = dr["CompanyAddress"].ToString();
                        report.Email = dr["Email"].ToString();
                        report.HowReached = dr["HowReached"].ToString();
                        report.Reference = dr["Reference"].ToString();
                        report.UpdatedBy = dr["UpdatedBy"].ToString();
                        report.UpdatedOn = dr["UpdatedOn"].ToString();
                        report.Comment = dr["Comment"].ToString();
                        report.PrimaryPackage = dr["PrimaryPackage"].ToString();
                        report.PackageRate = Convert.ToDecimal(dr["RatePerPlate"]);
                        report.Mukhwas = Convert.ToString(dr["Mukhwas"]);
                        report.TotalPaid = Convert.ToDecimal(dr["TotalPaid"]);
                        report.OutstandingAmount = Convert.ToDecimal(dr["OutstandingAmount"]);
                        report.BookingDiscount = Convert.ToDecimal(dr["BookingDiscount"]);
                        report.CashDiscount = Convert.ToDecimal(dr["CashDiscount"]);
                        report.PayableAmount = Convert.ToDecimal(dr["PayableAmount"]);
                    }

                    // --- Second Result Set: Package Items ---
                    if (dr.NextResult())
                    {
                        while (dr.Read())
                        {
                            report.PackageItems.Add(new PackageItem
                            {
                                Date = dr["Date"].ToString(),
                                Title = dr["Title"].ToString(),
                                PackageName = dr["PackageName"].ToString(),
                                Rate = Convert.ToDecimal(dr["Rate"]),
                                Pax = Convert.ToInt32(dr["Pax"]),
                                Amount = Convert.ToDecimal(dr["Amount"]),
                                Addon = dr["Addon"].ToString(),
                                Total = Convert.ToDecimal(dr["Total"])
                            });
                        }
                    }
                }
            }

            return report;
        }

        // --------------------- Generate PDF ---------------------
        private string GeneratePDF(BookingReportModel report, string domain, bool isWithPrice)
        {
            MemoryStream stream = new();
            Document doc = new(PageSize.A4, 30, 30, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            PdfContentByte cb = writer.DirectContent;

            // ===== PAGE BORDER =====
            cb.SetColorStroke(BaseColor.BLACK);
            float padding = 15f;
            float llx = doc.LeftMargin - padding;
            float lly = doc.BottomMargin - padding;
            float urx = doc.PageSize.Width - doc.RightMargin + padding;
            float ury = doc.PageSize.Height - doc.TopMargin + padding;
            cb.Rectangle(llx, lly, urx - llx, ury - lly);
            cb.Stroke();

            // ===== HEADER =====
            PdfPTable headerTable = new PdfPTable(2) { WidthPercentage = 100 };
            headerTable.SetWidths(new float[] { 1f, 3f });

            // Logo
            string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", "banquatlogo.png");
            if (System.IO.File.Exists(logoPath))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80, 80);
                headerTable.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT });
            }
            else headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });

            // Company details
            Font headerFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            PdfPTable companyTable = new PdfPTable(1) { WidthPercentage = 100 };
            companyTable.AddCell(new PdfPCell(new Phrase("Ramayana Restro & Banquets", headerFontBold)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, HorizontalAlignment = Element.ALIGN_RIGHT });
            companyTable.AddCell(new PdfPCell(new Phrase("1st Floor, Crossway Mall, Above Croma, Near Ramchowk, Ghoddod Road, Surat", headerFont)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, HorizontalAlignment = Element.ALIGN_RIGHT });
            companyTable.AddCell(new PdfPCell(new Phrase("State: Gujarat", headerFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
            PdfPCell textCell = new PdfPCell(companyTable) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE };
            headerTable.AddCell(textCell);
            doc.Add(headerTable);

            // ===== TITLE LINES =====
            PdfPTable lineTop = new PdfPTable(1) { WidthPercentage = 100 };
            lineTop.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, FixedHeight = 1, BackgroundColor = BaseColor.BLACK });
            doc.Add(lineTop);

            Paragraph title = new Paragraph("Function Prospectus", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 3,
                SpacingAfter = 8
            };
            doc.Add(title);

            PdfPTable lineBottom = new PdfPTable(1) { WidthPercentage = 100 };
            lineBottom.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, FixedHeight = 1, BackgroundColor = BaseColor.BLACK });
            doc.Add(lineBottom);

            // ===== EVENT DETAILS =====
            Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            Font labelFontnormal = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            PdfPTable detailsTable = new PdfPTable(2) { WidthPercentage = 100, SpacingBefore = 5f };
            detailsTable.SetWidths(new float[] { 1f, 1f });

            PdfPTable leftTable = new PdfPTable(1) { WidthPercentage = 100 };
            void AddLeftRow(string label, string value)
            {
                Phrase p = new Phrase();
                p.Add(new Chunk(label + " : ", labelFont));
                p.Add(new Chunk(value, valueFont));
                leftTable.AddCell(new PdfPCell(p) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, PaddingLeft = 4 });
            }

            AddLeftRow("Booking Ref#", report.BookingRef);
            AddLeftRow("Client Name", report.ClientName);
            AddLeftRow("Contact Number", report.ContactNumber);
            AddLeftRow("Event Date", report.EventDate);
            AddLeftRow("Property", report.Property);
            AddLeftRow("Pax", report.Pax.ToString());
            AddLeftRow("Hall", report.Hall);
            AddLeftRow("Slot", report.Slot);
            AddLeftRow("Timing", report.Timing);

            PdfPTable rightTable = new PdfPTable(1) { WidthPercentage = 100 };
            void AddRightRow(string label, string value)
            {
                Phrase p = new Phrase();
                p.Add(new Chunk(label + " : ", valueFont));
                p.Add(new Chunk(value, valueFont));
                rightTable.AddCell(new PdfPCell(p) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, PaddingLeft = 4 });
            }

            AddRightRow("Booked on", report.BookedOn);
            AddRightRow("Event", report.EventType);
            AddRightRow("Company Name", report.CompanyName);
            AddRightRow("Company Address", report.CompanyAddress);
            AddRightRow("Email", report.Email);

            detailsTable.AddCell(new PdfPCell(leftTable) { Border = Rectangle.NO_BORDER, PaddingTop = 10 });
            detailsTable.AddCell(new PdfPCell(rightTable) { Border = Rectangle.NO_BORDER, PaddingTop = 10, HorizontalAlignment = Element.ALIGN_RIGHT });
            doc.Add(detailsTable);
            doc.Add(new Paragraph("\n"));

            // ===== PRIMARY FOOD PACKAGE =====
            Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            PdfPTable packageTable = new PdfPTable(1) { WidthPercentage = 100 };
            packageTable.AddCell(new PdfPCell(new Phrase($"PRIMARY FOOD PACKAGE : {report.PrimaryPackage}@ ₹{report.RatePerPlate:N0} PER PLATE", boldFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingTop = 6,
                PaddingBottom = 6,
                HorizontalAlignment = Element.ALIGN_LEFT
            });
            doc.Add(packageTable);

            // ===== MUKHWAS =====
            //PdfPTable mukhwasTable = new PdfPTable(1) { WidthPercentage = 100 };
            //mukhwasTable.AddCell(new PdfPCell(new Phrase(report.Mukhwas, normalFont))
            //{
            //    Border = Rectangle.NO_BORDER,
            //    BackgroundColor = new BaseColor(230, 230, 230),
            //    PaddingLeft = 10,
            //    PaddingTop = 8,
            //    PaddingBottom = 8,
            //    HorizontalAlignment = Element.ALIGN_LEFT
            //});
            //doc.Add(mukhwasTable);
            // ===== MUKHWAS =====
            Phrase mukhwasPhrase = new Phrase();
            mukhwasPhrase.Add(new Chunk("Mukhwas  \n", boldFont));
            mukhwasPhrase.Add(new Chunk(report.Mukhwas, normalFont));

            PdfPCell mukhwasCell = new PdfPCell(mukhwasPhrase);
            mukhwasCell.Border = Rectangle.NO_BORDER;
            mukhwasCell.BackgroundColor = new BaseColor(230, 230, 230); // Light gray
            mukhwasCell.PaddingLeft = 10;
            mukhwasCell.PaddingTop = 8;
            mukhwasCell.PaddingBottom = 8;
            mukhwasCell.HorizontalAlignment = Element.ALIGN_LEFT;

            PdfPTable mukhwasTable = new PdfPTable(1);
            mukhwasTable.WidthPercentage = 100;
            mukhwasTable.AddCell(mukhwasCell);

            doc.Add(mukhwasTable);
            //doc.Add(new Paragraph("\n"));

            Paragraph paymentSummaryTitle = new Paragraph("PAYMENT SUMMARY", boldFont)
            {
                Alignment = Element.ALIGN_LEFT
                
            };
            doc.Add(paymentSummaryTitle);
            doc.Add(new Paragraph("\n"));

            // ===== PACKAGE ITEMS TABLE =====
            Font headerFont2 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            PdfPTable table = new PdfPTable(9) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1f, 2f, 2f, 2f, 1f, 1f, 2f, 2f, 2f });

            string[] headers = { "Sr#", "Date", "Title", "Food Package", "Rate", "Pax OR Per", "Package Amount", "Chargeable Addon Items", "Total" };
            foreach (var h in headers)
                table.AddCell(new PdfPCell(new Phrase(h, headerFont2)) { BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });

            Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
            int sr = 1;
            foreach (var item in report.PackageItems)
            {
                table.AddCell(new PdfPCell(new Phrase(sr.ToString(), rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(report.EventDate, rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.Title, rowFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.PackageName, rowFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 4 });

                if (isWithPrice)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("N0"), rowFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Pax.ToString(), rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("N0"), rowFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Addon, rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Total.ToString("N0"), rowFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                }
                else
                {
                    for (int i = 0; i < 5; i++)
                        table.AddCell(new PdfPCell(new Phrase("-", rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                }
                sr++;
            }
            doc.Add(table);
            doc.Add(new Paragraph("\n"));

            // ===== SUMMARY SECTION =====
            Font summaryFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            PdfPTable summaryTable = new PdfPTable(3) { WidthPercentage = 100 };
            summaryTable.SetWidths(new float[] { 0.33f, 0.33f, 0.33f });

            summaryTable.AddCell(new PdfPCell(new Phrase($"Deal Amount: ₹{report.PackageAmount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_LEFT, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Booking Discount: ₹{report.BookingDiscount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Payable Amount: ₹{report.PayableAmount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 6 });

            summaryTable.AddCell(new PdfPCell(new Phrase($"Total Paid: ₹{report.TotalPaid:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_LEFT, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Cash Discount: ₹{report.CashDiscount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Outstanding Amount: ₹{report.OutstandingAmount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 6 });

            doc.Add(summaryTable);
            doc.Add(new Paragraph("\n"));

            // ===== UPDATED INFO & TERMS =====
            PdfPTable detailsTable2 = new PdfPTable(1) { WidthPercentage = 100 };
            detailsTable2.AddCell(new PdfPCell(new Phrase($"Updated By : {report.UpdatedBy}", labelFontnormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            detailsTable2.AddCell(new PdfPCell(new Phrase($"Updated On : {report.UpdatedOn}", labelFontnormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            detailsTable2.AddCell(new PdfPCell(new Phrase($"Comment : {report.Comment}", labelFontnormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            detailsTable2.AddCell(new PdfPCell(new Phrase($"How did you reach us? : {report.ReachUs}", labelFontnormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            detailsTable2.AddCell(new PdfPCell(new Phrase($"Reference : {report.Reference}", labelFontnormal)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            detailsTable2.AddCell(new PdfPCell(new Phrase("TERMS & CONDITIONS", labelFont)) { Border = Rectangle.NO_BORDER, PaddingTop = 6, PaddingBottom = 6 });
            doc.Add(detailsTable2);
            doc.Add(new Paragraph("\n"));

            // ===== SIGNATURES =====
            PdfPTable signTable = new PdfPTable(3) { WidthPercentage = 100 };
            signTable.SetWidths(new float[] { 0.33f, 0.33f, 0.33f });
            foreach (var sign in new[] { "Sales Manager's Sign.", "Banquet Manager's Sign.", "Guest's Sign" })
            {
                PdfPCell cell = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
                cell.AddElement(new Phrase("__________________________", labelFont));
                cell.AddElement(new Phrase(sign, labelFont));
                signTable.AddCell(cell);
            }
            doc.Add(signTable);
            doc.Close();
            writer.Close();

            // ===== SAVE PDF =====
            string reportsDirPath = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "PackageReports");
            if (!Directory.Exists(reportsDirPath))
                Directory.CreateDirectory(reportsDirPath);

            string fileName = $"Package_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(reportsDirPath, fileName);
            System.IO.File.WriteAllBytes(filePath, stream.ToArray());

            return $"{domainUrl}/uploads/org/{domain}/PackageReports/{fileName}";

            //return $"{Request.Scheme}://{Request.Host}/uploads/org/{domain}/PackageReports/{fileName}";
        }

        private string GeneratePDF3(BookingReportModel report, string domain, bool isWithPrice)
        {
            MemoryStream stream = new();
            Document doc = new(PageSize.A4, 30, 30, 30, 30);
            PdfWriter writer = PdfWriter.GetInstance(doc, stream);
            doc.Open();
            PdfContentByte cb = writer.DirectContent;

            // Draw border
            cb.SetColorStroke(BaseColor.BLACK);
            float padding = 15f;
            float llx = doc.LeftMargin - padding;
            float lly = doc.BottomMargin - padding;
            float urx = doc.PageSize.Width - doc.RightMargin + padding;
            float ury = doc.PageSize.Height - doc.TopMargin + padding;
            cb.Rectangle(llx, lly, urx - llx, ury - lly);
            cb.Stroke();

            // ========== HEADER ==========
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.WidthPercentage = 100;
            headerTable.SetWidths(new float[] { 1f, 3f });

            // ---- Logo ----
            string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", "banquatlogo.png");
            if (System.IO.File.Exists(logoPath))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                logo.ScaleAbsolute(80, 80);
                PdfPCell logoCell = new PdfPCell(logo) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT };
                headerTable.AddCell(logoCell);
            }
            else headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });

            // ---- Company Details ----
            Font headerFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            PdfPTable companyTable = new PdfPTable(1);
            companyTable.WidthPercentage = 100;

            companyTable.AddCell(new PdfPCell(new Phrase("Ramayana Restro & Banquets", headerFontBold)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, HorizontalAlignment = Element.ALIGN_RIGHT });
            companyTable.AddCell(new PdfPCell(new Phrase("1st Floor, Crossway Mall, Above Croma, Near Ramchowk, Ghoddod Road, Surat", headerFont)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, HorizontalAlignment = Element.ALIGN_RIGHT });
            companyTable.AddCell(new PdfPCell(new Phrase("State: Gujarat", headerFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            PdfPCell textCell = new PdfPCell(companyTable) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, VerticalAlignment = Element.ALIGN_MIDDLE };
            headerTable.AddCell(textCell);
            doc.Add(headerTable);

            // ===== TITLE =====
            Paragraph title = new Paragraph("Function Prospectus", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 3,
                SpacingAfter = 8
            };
            doc.Add(title);

            // ===== EVENT DETAILS SECTION =====
            Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            PdfPTable detailsTable = new PdfPTable(2);
            detailsTable.WidthPercentage = 100;
            detailsTable.SetWidths(new float[] { 1f, 1f });

            // Left table
            PdfPTable leftTable = new PdfPTable(1);
            void AddLeftRow(string label, string value)
            {
                leftTable.AddCell(new PdfPCell(new Phrase(label + " : " + value, labelFont)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6 });
            }
            AddLeftRow("Booking Ref#", report.BookingRef);
            AddLeftRow("Client Name", report.ClientName);
            AddLeftRow("Contact Number", report.ContactNumber);
            AddLeftRow("Event Date", report.EventDate);
            AddLeftRow("Property", report.Property);
            AddLeftRow("Pax", report.Pax.ToString());
            AddLeftRow("Hall", report.Hall);
            AddLeftRow("Slot", report.Slot);
            AddLeftRow("Timing", report.Timing);

            // Right table
            PdfPTable rightTable = new PdfPTable(1);
            void AddRightRow(string label, string value)
            {
                rightTable.AddCell(new PdfPCell(new Phrase(label + " : " + value, valueFont)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6 });
            }
            AddRightRow("Booked on", report.BookedOn);
            AddRightRow("Event", report.EventType);
            AddRightRow("Company Name", report.CompanyName);
            AddRightRow("Company Address", report.CompanyAddress);
            AddRightRow("Email", report.Email);

            detailsTable.AddCell(new PdfPCell(leftTable) { Border = Rectangle.NO_BORDER });
            detailsTable.AddCell(new PdfPCell(rightTable) { Border = Rectangle.NO_BORDER });
            doc.Add(detailsTable);
            doc.Add(new Paragraph("\n"));

            // ===== PACKAGE TABLE =====
            Font headerFont2 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            PdfPTable table = new PdfPTable(9);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1f, 2f, 2f, 2f, 1f, 1f, 2f, 2f, 2f });

            string[] headers = { "Sr#", "Date", "Title", "Food Package", "Rate", "Pax OR Per", "Package Amount", "Chargeable Addon Items", "Total" };
            foreach (var h in headers)
                table.AddCell(new PdfPCell(new Phrase(h, headerFont2)) { BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });

            Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            int sr = 1;
            foreach (var item in report.PackageItems)
            {
                table.AddCell(new PdfPCell(new Phrase(sr.ToString(), rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(report.EventDate, rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.Title, rowFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 4 });
                table.AddCell(new PdfPCell(new Phrase(item.PackageName, rowFont)) { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 4 });

                if (isWithPrice)
                {
                    table.AddCell(new PdfPCell(new Phrase(item.Rate.ToString("N0"), rowFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Pax.ToString(), rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Amount.ToString("N0"), rowFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Addon, rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase(item.Total.ToString("N0"), rowFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 4 });
                }
                else
                {
                    // Hide prices
                    table.AddCell(new PdfPCell(new Phrase("-", rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase("-", rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase("-", rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase("-", rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                    table.AddCell(new PdfPCell(new Phrase("-", rowFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 4 });
                }

                sr++;
            }
            doc.Add(table);

            // ===== SUMMARY SECTION =====
            Font summaryFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            PdfPTable summaryTable = new PdfPTable(3);
            summaryTable.WidthPercentage = 100;
            summaryTable.SetWidths(new float[] { 0.33f, 0.33f, 0.33f });

            summaryTable.AddCell(new PdfPCell(new Phrase($"Deal Amount: ₹{report.PackageAmount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_LEFT, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Booking Discount: ₹{report.BookingDiscount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Payable Amount: ₹{report.PayableAmount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 6 });

            summaryTable.AddCell(new PdfPCell(new Phrase($"Total Paid: ₹{report.TotalPaid:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_LEFT, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Cash Discount: ₹{report.CashDiscount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_CENTER, Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase($"Outstanding Amount: ₹{report.OutstandingAmount:N0}", summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 6 });

            doc.Add(summaryTable);

            doc.Close();
            writer.Close();

            // Save PDF
            string reportsDirPath = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "PackageReports");
            if (!Directory.Exists(reportsDirPath))
                Directory.CreateDirectory(reportsDirPath);

            string fileName = $"Package_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(reportsDirPath, fileName);
            System.IO.File.WriteAllBytes(filePath, stream.ToArray());

            return $"{Request.Scheme}://{Request.Host}/uploads/org/{domain}/PackageReports/{fileName}";
        }

        //unUsed - will do later with this template
        [HttpPost]
        [Route("GeneratePackageReport_template")]
        public IActionResult GeneratePackageReport_template([FromBody] PackageReportRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                string baseDir = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "PackageReports"); //here
                if (!Directory.Exists(baseDir)) Directory.CreateDirectory(baseDir);

                string fileName = $"Package_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                string pdfPath = Path.Combine(baseDir, fileName);

                // Generate PDF
                var builder = new BookingReportTemplateBuilder(_env);
                builder.Generate_BookingReport_A4_Portrait(pdfPath);

                string fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/org/{domain}/PackageReports/{fileName}";

                apiResponse.status = "Success";
                apiResponse.data = fileUrl;
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
            }

            return Ok(apiResponse);
        }

        //Get Calendar
        [HttpPost]
        [Route("GetEnquiryCalendarMock")]
        public IActionResult GetEnquiryCalendarMock([FromBody] GetEnquiryCalendarMock_Request request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EnquiryCalendarMock_Model> list = new List<EnquiryCalendarMock_Model>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetEnquiryCalendarMock", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Year", SqlDbType.Int).Value = request.Year;
                    cmd.Parameters.Add("@Month", SqlDbType.Int).Value = request.Month;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    // Deserialize JSON list
                    List<EnquiryCalendarMock_Customer> enquiryList = new List<EnquiryCalendarMock_Customer>();

                    if (row["EnquiryList"] != DBNull.Value)
                    {
                        try
                        {
                            enquiryList = JsonConvert.DeserializeObject<List<EnquiryCalendarMock_Customer>>(
                                row["EnquiryList"].ToString()
                            );
                        }
                        catch
                        {
                            enquiryList = new List<EnquiryCalendarMock_Customer>();
                        }
                    }

                    list.Add(new EnquiryCalendarMock_Model
                    {
                        Date = Convert.ToString(row["Date"]),
                        Day = Convert.ToString(row["Day"]),
                        Enquiries = Convert.ToInt32(row["Enquiries"]),
                        EnquiryList = enquiryList
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Calendar loaded successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.status = "Error";
                apiResponse.message = "Error: " + ex.Message;
                return StatusCode(500, apiResponse);
            }
        }


    }
}
// --------------------- Request / Models ---------------------
public class PackageReportRequest
{
    public long BookingId { get; set; }
    public bool IsWithPrice { get; set; }
}

public class BookingReportModel
{
    public long BookingId { get; set; }
    public string BookingRef { get; set; }
    public string ClientName { get; set; }
    public string ContactNumber { get; set; }
    public string EventDate { get; set; }
    public string Property { get; set; }
    public int Pax { get; set; }
    public string Hall { get; set; }
    public string Slot { get; set; }
    public string Timing { get; set; }
    public string BookedOn { get; set; }
    public string EventType { get; set; }
    public string CompanyName { get; set; }
    public string CompanyAddress { get; set; }
    public string Email { get; set; }
    public string HowReached { get; set; }
    public string ReachUs { get; set; }
    public string Reference { get; set; }
    public string UpdatedBy { get; set; }
    public string UpdatedOn { get; set; }
    public string Comment { get; set; }
    public string PrimaryPackage { get; set; }
    public string RatePerPlate { get; set; }
    
    public string Mukhwas { get; set; }
    public decimal PackageRate { get; set; }
    public decimal PackageAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal BookingDiscount { get; set; }
    public decimal CashDiscount { get; set; }
    public decimal PayableAmount { get; set; }

    public List<PackageItem> PackageItems { get; set; }
}

public class PackageItem
{
    public string? Date { get; set; }
    public string? Title { get; set; }
    public string? PackageName { get; set; }
    public decimal Rate { get; set; }
    public decimal Pax { get; set; }
    public decimal Amount { get; set; }
    public string? Addon { get; set; }
    public decimal Total { get; set; }
}
