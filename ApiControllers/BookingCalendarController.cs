using DocumentFormat.OpenXml.Office.CoverPageProps;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                // 🔹 Database setup
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBookingCalendar", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId == 0 ? 0 : request.BookingId);
                    cmd.Parameters.AddWithValue("@BanquetId", request.BanquetId == 0 ? 0 : request.BanquetId);
                    cmd.Parameters.AddWithValue("@EventDate", request.EventDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@YearId", request.YearId == 0 ? 0 : request.YearId);
                    cmd.Parameters.AddWithValue("@MonthId", request.MonthId == 0 ? 0 : request.MonthId);

                    con.Open();
                    DataTable dataTable = new DataTable();
                    SqlDataReader reader = cmd.ExecuteReader();
                    dataTable.Load(reader);
                    con.Close();

                    foreach (DataRow dr in dataTable.Rows)
                    {
                        BookingModel booking = new BookingModel();
                        booking.BookingCalendar_IsBlocked = dr["BookingCalendar_IsBlocked"] == DBNull.Value ? false : Convert.ToBoolean(dr["BookingCalendar_IsBlocked"]);
                        booking.BookingId = dr["BookingId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["BookingId"]);
                        booking.EventDate = dr["EventDate"] == DBNull.Value ? "" : Convert.ToDateTime(dr["EventDate"]).ToString("yyyy-MM-dd");
                        booking.EventDate_Format = dr["EventDate_Format"] == DBNull.Value ? "" : Convert.ToString(dr["EventDate_Format"]);
                        booking.ShiftCode = dr["ShiftCode"] == DBNull.Value ? "" : Convert.ToString(dr["ShiftCode"]);
                        booking.ShiftFullName = dr["ShiftFullName"] == DBNull.Value ? "" : Convert.ToString(dr["ShiftFullName"]);
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
                        booking.PaymentDetails_AccountId = dr["PaymentDetails_AccountId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["PaymentDetails_AccountId"]);
                        booking.PaymentDetails_TransactionId = dr["PaymentDetails_TransactionId"] == DBNull.Value ? "" : Convert.ToString(dr["PaymentDetails_TransactionId"]);
                        booking.PaymentDetails_ChequeBank = dr["PaymentDetails_ChequeBank"] == DBNull.Value ? "" : Convert.ToString(dr["PaymentDetails_ChequeBank"]);
                        booking.PaymentDetails_ChequeNo = dr["PaymentDetails_ChequeBank"] == DBNull.Value ? "" : Convert.ToString(dr["PaymentDetails_ChequeBank"]);
                        booking.PaymentDetails_ChequeDate = dr["PaymentDetails_ChequeDate"] == DBNull.Value ? "" : Convert.ToString(dr["PaymentDetails_ChequeDate"]);

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

                        booking.InvoiceNo = dr["InvoiceNo"] == DBNull.Value ? 0 : Convert.ToInt64(dr["InvoiceNo"]);
                        booking.ProformaInvoiceNo = dr["ProformaInvoiceNo"] == DBNull.Value ? 0 : Convert.ToInt64(dr["ProformaInvoiceNo"]);

                        var paymentJson = dr["PaymentTrailList"] == DBNull.Value ? null : dr["PaymentTrailList"].ToString();
                        booking.PaymentTrailList = !string.IsNullOrWhiteSpace(paymentJson)
                            ? JsonConvert.DeserializeObject<List<PaymentTrailModel>>(paymentJson)
                            : new List<PaymentTrailModel>();
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

                apiResponse.message = ex.Message;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long bookingId = 0, paymentId = 0;
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
                    cmd.Parameters.AddWithValue("@PaymentDetails_AccountId", model.PaymentDetails_AccountId ?? 0);

                    cmd.Parameters.AddWithValue("@PaymentDetails_TransactionId", model.PaymentDetails_TransactionId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentDetails_ChequeBank", model.PaymentDetails_ChequeBank ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentDetails_ChequeNo", model.PaymentDetails_ChequeNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PaymentDetails_ChequeDate", model.PaymentDetails_ChequeDate ?? (object)DBNull.Value);

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

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bookingId = reader["BookingId"] != DBNull.Value
                                ? Convert.ToInt64(reader["BookingId"])
                                : 0;

                            paymentId = reader["PaymentId"] != DBNull.Value
                                ? Convert.ToInt64(reader["PaymentId"])
                                : 0;
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.data= new { BookingId = bookingId, PaymentId = paymentId };
                apiResponse.message = model.BookingId > 0
                    ? "Booking updated successfully!"
                    : "Booking inserted successfully!";

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
        [Route("DeleteBookingCalendar")]
        public IActionResult DeleteBookingCalendar([FromBody] DeleteBookingCalendarRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                // 🔹 Validate and get connection string
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
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
                apiResponse.message = ex.Message;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
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

                apiResponse.message = ex.Message;
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
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
                apiResponse.message = ex.Message;
                apiResponse.data = null;
                return StatusCode(500, apiResponse);
            }
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
                        report.CompanyState = dr["CompanyState"].ToString();
                        report.CompanyLogo = dr["CompanyLogo"].ToString();
                        report.Email = dr["Email"].ToString();
                        report.HowReached = dr["HowReached"].ToString();
                        report.Reference = dr["Reference"].ToString();
                        report.UpdatedBy = dr["UpdatedBy"].ToString();
                        report.UpdatedOn = dr["UpdatedOn"].ToString();
                        report.Comment = dr["Comment"].ToString();
                        report.PrimaryPackage = dr["PrimaryPackage"].ToString();
                        report.RatePerPlate = dr["RatePerPlate"].ToString();

                        report.Mukhwas = dr["Mukhwas"].ToString();
                        report.TotalPaid = dr["TotalPaid"].ToString();
                        report.OutstandingAmount = dr["OutstandingAmount"].ToString();
                        report.BookingDiscount = dr["BookingDiscount"].ToString();
                        report.PayableAmount = dr["PayableAmount"].ToString();
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
            string companyLogo = report.CompanyLogo;
            string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", companyLogo);
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
            string propertyName = report.Property;
            string companyAddress = report.CompanyAddress;
            string companyState = report.CompanyState;

            companyTable.AddCell(new PdfPCell(new Phrase(propertyName, headerFontBold)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, HorizontalAlignment = Element.ALIGN_RIGHT });
            companyTable.AddCell(new PdfPCell(new Phrase(companyAddress, headerFont)) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, HorizontalAlignment = Element.ALIGN_RIGHT });
            companyTable.AddCell(new PdfPCell(new Phrase($"State: {companyState}", headerFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
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

            AddLeftRow("Booking Ref.", '#'+report.BookingRef);
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
            packageTable.AddCell(new PdfPCell(new Phrase($"PRIMARY FOOD PACKAGE : {report.PrimaryPackage}@ Rs.{report.RatePerPlate:N0} /Per Plate", boldFont))
            {
                Border = Rectangle.NO_BORDER,
                PaddingTop = 6,
                PaddingBottom = 6,
                HorizontalAlignment = Element.ALIGN_LEFT
            });
            doc.Add(packageTable);

            // ===== SUMMARY SECTION =====
            // ===== LEFT + RIGHT SECTION (Details + Summary) =====

            Font labelFontLeft = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
            Font labelFontBoldLeft = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.BLACK);

            Font summaryLabelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
            Font summaryValueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);

            BaseColor summaryBg = new BaseColor(82, 86, 88);

            // Outer table (2 columns)
            PdfPTable mainTable = new PdfPTable(2) { WidthPercentage = 100 };
            mainTable.SetWidths(new float[] { 0.6f, 0.4f });


            // ================= LEFT SIDE =================
            leftTable = new PdfPTable(1) { WidthPercentage = 100 };

            leftTable.AddCell(new PdfPCell(new Phrase($"Updated By : {report.UpdatedBy}", labelFontLeft)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            leftTable.AddCell(new PdfPCell(new Phrase($"Updated On : {report.UpdatedOn}", labelFontLeft)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            leftTable.AddCell(new PdfPCell(new Phrase($"Comment : {report.Comment}", labelFontLeft)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            leftTable.AddCell(new PdfPCell(new Phrase($"How did you reach us? : {report.ReachUs}", labelFontLeft)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });
            leftTable.AddCell(new PdfPCell(new Phrase($"Reference : {report.Reference}", labelFontLeft)) { Border = Rectangle.NO_BORDER, PaddingBottom = 4 });

            leftTable.AddCell(new PdfPCell(new Phrase("TERMS & CONDITIONS", labelFontBoldLeft))
            {
                Border = Rectangle.NO_BORDER,
                PaddingTop = 6,
                PaddingBottom = 6
            });

            mainTable.AddCell(new PdfPCell(leftTable)
            {
                Border = Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_TOP
            });


            // ================= RIGHT SIDE (AMOUNTS) =================
            rightTable = new PdfPTable(2) { WidthPercentage = 100 };
            rightTable.SetWidths(new float[] { 0.6f, 0.4f });

            void AddAmountRow(string label, string value)
            {
                rightTable.AddCell(new PdfPCell(new Phrase(label, summaryLabelFont))
                {
                    Border = Rectangle.NO_BORDER,
                    //BackgroundColor = summaryBg,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 6
                });

                rightTable.AddCell(new PdfPCell(new Phrase($"₹ {value:N0}", summaryValueFont))
                {
                    Border = Rectangle.NO_BORDER,
                    //BackgroundColor = summaryBg,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 6
                });
            }

            AddAmountRow("Deal Amount", report.PayableAmount);
            AddAmountRow("Booking Discount", report.BookingDiscount);
            AddAmountRow("Payable Amount", report.PayableAmount);
            AddAmountRow("Total Paid", report.TotalPaid);
            AddAmountRow("Outstanding Amount", report.OutstandingAmount);

            mainTable.AddCell(new PdfPCell(rightTable)
            {
                Border = Rectangle.NO_BORDER,
                VerticalAlignment = Element.ALIGN_TOP
            });

            doc.Add(mainTable);
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
            string reportsDirPath = Path.Combine(_env.WebRootPath, "reports");
            if (!Directory.Exists(reportsDirPath))
                Directory.CreateDirectory(reportsDirPath);

            string fileName = $"FR_BK_{report.BookingId}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = Path.Combine(reportsDirPath, fileName);

            Utils.deleteOldFiles(reportsDirPath, "FR_BK_*.pdf", 60);

            System.IO.File.WriteAllBytes(filePath, stream.ToArray());

            return $"{domainUrl}/reports/{fileName}";

            //return $"{Request.Scheme}://{Request.Host}/uploads/org/{domain}/PackageReports/{fileName}";
        }


        //unUsed - will do later with this template
        [HttpPost]
        [Route("GeneratePackageReport_template")]
        public IActionResult GeneratePackageReport_template([FromBody] PackageReportRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

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
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        //Reschedule
        [HttpPost]
        [Route("SaveBookingCalendar_Reschedule")]
        public IActionResult SaveBookingCalendar_Reschedule([FromBody] SaveBookingCalendarReschedule model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                // 🔹 Validate connection setup
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult)){
                    return errorResult!;
                }

                long bookingId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBookingCalendar_Reschedule", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 🔹 Required
                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);

                    // 🔹 Optional params
                    cmd.Parameters.AddWithValue("@BanquetId", model.BanquetId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SlotId", model.SlotId ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@FromDate", model.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@TillDate", model.TillDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@StartTime", model.StartTime ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndTime", model.EndTime ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@ShiftCode", model.ShiftCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Action_By", model.ActionBy);

                    // 🔹 Execute
                    con.Open();
                    bookingId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.status = "Success";
                apiResponse.data = new { BookingId = bookingId };
                apiResponse.message = "Booking rescheduled successfully!";

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
// --------------------- Request / Models ---------------------
public class PackageReportRequest
{
    public long BookingId { get; set; }
    public bool IsWithPrice { get; set; }
}

public class 
    BookingReportModel
{
    public long BookingId { get; set; }
    public string? BookingRef { get; set; }
    public string? ClientName { get; set; }
    public string? ContactNumber { get; set; }
    public string? EventDate { get; set; }
    public string? Property { get; set; }
    public int Pax { get; set; }
    public string? Hall { get; set; }
    public string? Slot { get; set; }
    public string? Timing { get; set; }
    public string? BookedOn { get; set; }
    public string? EventType { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyState { get; set; }
    public string? CompanyLogo { get; set; }
    public string? Email { get; set; }
    public string? HowReached { get; set; }
    public string? ReachUs { get; set; }
    public string? Reference { get; set; }
    public string? UpdatedBy { get; set; }
    public string? UpdatedOn { get; set; }
    public string? Comment { get; set; }
    public string? PrimaryPackage { get; set; }
    public string? RatePerPlate { get; set; }
    
    public string? Mukhwas { get; set; }
    public string? PackageRate { get; set; }
    public string? PackageAmount { get; set; }
    public string? TotalPaid { get; set; }
    public string? OutstandingAmount { get; set; }
    public string? BookingDiscount { get; set; }
    public string? PayableAmount { get; set; }
    public List<PackageItem>? PackageItems { get; set; }
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
