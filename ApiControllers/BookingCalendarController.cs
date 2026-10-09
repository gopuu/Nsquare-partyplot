using DocumentFormat.OpenXml.Office.CoverPageProps;
using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using ERPAdmin.Services;
using Humanizer;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
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
        private readonly WhatsAppService _whatsAppService;
        private readonly ImageUploadService _imageUploadService;

        public BookingCalendarController(IConfiguration configuration, IWebHostEnvironment env,
            ImageUploadService imageUploadService, IHttpContextAccessor httpContextAccessor, WhatsAppService whatsAppService)
                : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly

        {
            _env = env;
            _whatsAppService = whatsAppService;
            _imageUploadService = imageUploadService;
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
                        long bookingId = dr["BookingId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["BookingId"]);
                        booking.BookingCalendar_IsBlocked = dr["BookingCalendar_IsBlocked"] == DBNull.Value ? false : Convert.ToBoolean(dr["BookingCalendar_IsBlocked"]);
                        booking.BookingId = bookingId;
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
                        booking.ClientMaster_Id = dr["ClientMaster_Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["ClientMaster_Id"]);
                        booking.Client_Name = dr["Client_Name"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Name"]);
                        booking.Client_Address = dr["Client_Address"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Address"]);
                        booking.Client_Email = dr["Client_Email"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Email"]);
                        booking.Client_Contact = dr["Client_Contact"] == DBNull.Value ? "" : Convert.ToString(dr["Client_Contact"]);
                        booking.Client_GSTIN = dr["Client_GSTIN"] == DBNull.Value ? "" : Convert.ToString(dr["Client_GSTIN"]);
                        booking.Client_StateId = dr["Client_StateId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Client_StateId"]);
                        booking.Client_DOB = dr["Client_DOB"] == DBNull.Value ? "" : Convert.ToString(dr["Client_DOB"]);
                        booking.Client_DOB2 = dr["Client_DOB2"] == DBNull.Value ? "" : Convert.ToString(dr["Client_DOB2"]);
                        booking.Client_AnniversaryDate = dr["Client_AnniversaryDate"] == DBNull.Value ? "" : Convert.ToString(dr["Client_AnniversaryDate"]);
                        booking.Client_AnniversaryDate2 = dr["Client_AnniversaryDate2"] == DBNull.Value ? "" : Convert.ToString(dr["Client_AnniversaryDate2"]);

                        booking.Booking_SubEventId = dr["Booking_SubEventId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_SubEventId"]);
                        booking.Booking_SubEventName = dr["Booking_SubEventName"] == DBNull.Value ? "" : Convert.ToString(dr["Booking_SubEventName"]);
                        booking.Booking_PAX = dr["Booking_PAX"] == DBNull.Value ? 0 : Convert.ToInt32(dr["Booking_PAX"]);
                        booking.Booking_HallRent = dr["Booking_HallRent"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Booking_HallRent"]);
                        booking.Booking_GST = dr["Booking_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["Booking_GST"]);
                        booking.Booking_LeadReferenceId = dr["Booking_LeadReferenceId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_LeadReferenceId"]);
                        booking.Booking_MenuType_Id = dr["Booking_MenuType_Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_MenuType_Id"]);
                        booking.Booking_TypeId = dr["Booking_TypeId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_TypeId"]);

                        booking.Booking_EventId = dr["Booking_EventId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booking_EventId"]);
                        booking.Booking_EventName = dr["Booking_EventName"] == DBNull.Value ? "" : Convert.ToString(dr["Booking_EventName"]);

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

                        booking.OtherCharges_ElectricityBill_Amount = dr["OtherCharges_ElectricityBill_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_ElectricityBill_Amount"]);
                        booking.OtherCharges_ElectricityBill_GST = dr["OtherCharges_ElectricityBill_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_ElectricityBill_GST"]);

                        booking.OtherCharges_GeneratorCharge_Amount = dr["OtherCharges_GeneratorCharge_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_GeneratorCharge_Amount"]);
                        booking.OtherCharges_GeneratorCharge_GST = dr["OtherCharges_GeneratorCharge_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_GeneratorCharge_GST"]);

                        booking.OtherCharges_Amount = dr["OtherCharges_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_Amount"]);
                        booking.OtherCharges_GST = dr["OtherCharges_GST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["OtherCharges_GST"]);

                        booking.OtherCharges_BookingNoteForCustomer = dr["OtherCharges_BookingNoteForCustomer"] == DBNull.Value ? "" : Convert.ToString(dr["OtherCharges_BookingNoteForCustomer"]);
                        booking.OtherCharges_InternalOfficeNote = dr["OtherCharges_InternalOfficeNote"] == DBNull.Value ? "" : Convert.ToString(dr["OtherCharges_InternalOfficeNote"]);

                        var foodPackageJson = Convert.ToString(dr["FoodPackageJSON"]);
                        booking.JSON_FoodPackage = string.IsNullOrWhiteSpace(foodPackageJson)
                            ? new List<GetFoodPackage>()
                            : JsonConvert.DeserializeObject<List<GetFoodPackage>>(foodPackageJson) ?? new List<GetFoodPackage>();

                        foreach (var foodPackage in booking.JSON_FoodPackage)
                        {
                            foodPackage.MenuItemsList ??= new List<GetFoodPackage_Menu>();
                            foodPackage.AddOnsList ??= new List<GetFoodPackage_AddOn>();

                            foreach (var menu in foodPackage.MenuItemsList)
                            {
                                menu.Items ??= new List<GetFoodPackage_MenuItem>();
                                foreach (var item in menu.Items)
                                {
                                    item.Children ??= new List<GetFoodPackage_MenuChildItem>();
                                }
                            }

                            foreach (var addon in foodPackage.AddOnsList)
                            {
                                addon.Items ??= new List<GetFoodPackage_AddOnItem>();
                                foreach (var item in addon.Items)
                                {
                                    item.Children ??= new List<GetFoodPackage_AddOnChildItem>();
                                }
                            }
                        }

                        booking.JSON_Notes = JsonConvert.DeserializeObject<List<GetNotes>>(Convert.ToString(dr["NotesJSON"]));

                        booking.PaymentSummary_TaxableAmount = dr["PaymentSummary_TaxableAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_TaxableAmount"]);
                        booking.PaymentSummary_CGST = dr["PaymentSummary_CGST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_CGST"]);
                        booking.PaymentSummary_SGST = dr["PaymentSummary_SGST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_SGST"]);
                        booking.PaymentSummary_IGST = dr["PaymentSummary_IGST"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_IGST"]);
                        booking.PaymentSummary_Total = dr["PaymentSummary_Total"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_Total"]);
                        booking.PaymentSummary_BookingDiscount_Type_Id = dr["PaymentSummary_BookingDiscount_Type_Id"] == DBNull.Value ? 0 : Convert.ToInt64(dr["PaymentSummary_BookingDiscount_Type_Id"]);
                        booking.PaymentSummary_BookingDiscount_Type_Amount = dr["PaymentSummary_BookingDiscount_Type_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_BookingDiscount_Type_Amount"]);
                        booking.PaymentSummary_BookingDiscount_Total = dr["PaymentSummary_BookingDiscount_Total"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_BookingDiscount_Total"]);
                        booking.PaymentSummary_PayableAmount = dr["PaymentSummary_PayableAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(dr["PaymentSummary_PayableAmount"]);
                        booking.Payment_Status = dr["Payment_Status"] == DBNull.Value ? "" : Convert.ToString(dr["Payment_Status"]);

                        booking.Booked_By_EmpId = dr["Booked_By_EmpId"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Booked_By_EmpId"]);
                        booking.Booked_By_EmpName = dr["Booked_By_EmpName"] == DBNull.Value ? "" : Convert.ToString(dr["Booked_By_EmpName"]);
                        booking.Booked_By_EmpName = dr["Booked_By_EmpName"] == DBNull.Value ? "" : Convert.ToString(dr["Booked_By_EmpName"]);
                        booking.Booked_On = dr["Booked_On"] == DBNull.Value ? "" : Convert.ToString(dr["Booked_On"]);
                        booking.VendorWorkOrder_Count = dr["VendorWorkOrder_Count"] == DBNull.Value ? 0 : Convert.ToInt64(dr["VendorWorkOrder_Count"]);

                        booking.ReachedUs = dr["ReachedUs"] == DBNull.Value ? "" : Convert.ToString(dr["ReachedUs"]);
                        booking.Reference = dr["Reference"] == DBNull.Value ? "" : Convert.ToString(dr["Reference"]);

                        booking.InvoiceNo = dr["InvoiceNo"] == DBNull.Value ? 0 : Convert.ToInt64(dr["InvoiceNo"]);
                        booking.ProformaInvoiceNo = dr["ProformaInvoiceNo"] == DBNull.Value ? 0 : Convert.ToInt64(dr["ProformaInvoiceNo"]);

                        var paymentJson = dr["PaymentTrailList"] == DBNull.Value ? null : dr["PaymentTrailList"].ToString();
                        booking.PaymentTrailList = !string.IsNullOrWhiteSpace(paymentJson)
                            ? JsonConvert.DeserializeObject<List<PaymentTrailModel>>(paymentJson)
                            : new List<PaymentTrailModel>();

                        var guestJson = dr["GuestJSON"] == DBNull.Value ? null : dr["GuestJSON"].ToString();
                        booking.GuestList = !string.IsNullOrWhiteSpace(guestJson)
                            ? JsonConvert.DeserializeObject<List<GuestModel>>(guestJson)
                            : new List<GuestModel>();

                        // Booking Uploads
                        var uploadsJson = dr["UploadsJSON"] == DBNull.Value ? null : dr["UploadsJSON"].ToString();
                        booking.UploadsList = !string.IsNullOrWhiteSpace(uploadsJson)
                            ? JsonConvert.DeserializeObject<List<BookingUploadModel>>(uploadsJson)
                            : new List<BookingUploadModel>();

                        string folderPath = $"org/{domain}/BookingCalendar/BookingId_{bookingId}/";

                        if (booking.UploadsList != null && booking.UploadsList.Any())
                        {
                            foreach (var upload in booking.UploadsList)
                            {
                                if (!string.IsNullOrWhiteSpace(upload.FilePath))
                                {
                                    upload.FilePath = $"{domainUrl}/uploads/{folderPath}{upload.FilePath}";
                                }
                            }
                        }

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
        public async Task<IActionResult> SaveBookingCalendar([FromBody] SaveBookingCalendar model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                // 🔹 Validate connection setup
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long bookingId = 0, paymentId = 0;

                string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                string userAgent = HttpContext.Request.Headers.UserAgent.ToString();

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
                    cmd.Parameters.AddWithValue("@ClientMaster_Id", model.ClientMaster_Id ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Name", model.Client_Name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Address", model.Client_Address ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Email", model.Client_Email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_Contact", model.Client_Contact ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_GSTIN", model.Client_GSTIN ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_StateId", model.Client_StateId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_DOB", model.Client_DOB ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Client_AnniversaryDate", model.Client_AnniversaryDate ?? (object)DBNull.Value);

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

                    cmd.Parameters.AddWithValue("@OtherCharges_ElectricityBill_Amount", model.OtherCharges_ElectricityBill_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_ElectricityBill_GST", model.OtherCharges_ElectricityBill_GST ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@OtherCharges_GeneratorCharge_Amount", model.OtherCharges_GeneratorCharge_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_GeneratorCharge_GST", model.OtherCharges_GeneratorCharge_GST ?? (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@OtherCharges_Amount", model.OtherCharges_Amount ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@OtherCharges_GST", model.OtherCharges_GST ?? (object)DBNull.Value);

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

                    cmd.Parameters.AddWithValue("@JSON_Notes",
                        model.JSON_Notes != null ? JsonConvert.SerializeObject(model.JSON_Notes) : (object)DBNull.Value);
 
                    cmd.Parameters.AddWithValue("@JSON_FoodPackage",
                        model.JSON_FoodPackage != null ? JsonConvert.SerializeObject(model.JSON_FoodPackage) : (object)DBNull.Value);
                    
                    cmd.Parameters.AddWithValue("@JSON_Guests",
                        model.JSON_Guests != null ? JsonConvert.SerializeObject(model.JSON_Guests) : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@Action_By", model.ActionBy);

                    cmd.Parameters.AddWithValue("@IPAddress", ipAddress);
                    cmd.Parameters.AddWithValue("@UserAgent", userAgent);

                    // 🔹 Execute SP
                    con.Open();

                    string whatsapp_name = "", whatsapp_bName = "", whatsapp_EDate = "", whatsapp_orgMobile = "", whatsapp_orgName = "",
                        whatsapp_client_contactno = "", templateName = "", msgTemplate = "";
                    int companyId = 0;
                    bool isSendMsg = false;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bookingId = reader["BookingId"] != DBNull.Value ? Convert.ToInt64(reader["BookingId"]) : 0;
                            paymentId = reader["PaymentId"] != DBNull.Value ? Convert.ToInt64(reader["PaymentId"]) : 0;

                            whatsapp_name = reader["Name"]?.ToString() ?? "";
                            whatsapp_bName = reader["BName"]?.ToString() ?? "";
                            whatsapp_EDate = reader["EDate"]?.ToString() ?? "";
                            whatsapp_orgMobile = reader["OrgMobile"]?.ToString() ?? "";
                            whatsapp_orgName = reader["OrgName"]?.ToString() ?? "";
                            whatsapp_client_contactno = model.Client_Contact ?? "";
                            companyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : 0;
                            templateName = reader["TempName"]?.ToString() ?? "";
                            msgTemplate = reader["MsgTemplate"]?.ToString() ?? "";
                            isSendMsg = reader["IsSendMsg"] != DBNull.Value ? Convert.ToBoolean(reader["IsSendMsg"]) : false;
                        }
                    }

                    //NOTIFY 2 SEND WHATSAPP - NEW BOOKING CREATED
                    #region SEND WHATSAPP
                    if (model.BookingId == 0 && isSendMsg)
                    {
                        List<string> variables = new List<string>
                        {
                            whatsapp_name,
                            whatsapp_bName,
                            whatsapp_EDate,
                            whatsapp_orgMobile,
                            whatsapp_orgName
                        };

                        await _whatsAppService.SendTemplateWhatsAppCommon(
                            companyId: companyId,
                            mobileNo: whatsapp_client_contactno,
                            templateName: templateName,
                            variables: variables,
                            actionBy: model.ActionBy,
                            isPromotional: false,
                            domainUrl: domainUrl,
                            msgTemplate: msgTemplate
                        );
                    }
                    #endregion

                    //Reference Image Upload
                    // Reference File Upload
                    #region REFERENCE FILE UPLOAD

                    if (model.JSON_Uploads != null && model.JSON_Uploads.Any())
                    {
                        List<object> uploadedFiles = new List<object>();
                        string folderPath = $"org/{domain}/BookingCalendar/BookingId_{bookingId}/";

                        foreach (var upload in model.JSON_Uploads)
                        {
                            if (string.IsNullOrWhiteSpace(upload.UploadFile_Base64))
                                continue;

                            string fileType = _imageUploadService.GetFileTypeFromBase64(upload.UploadFile_Base64);
                            string filePrefix = $"BookingId_{bookingId}_{Guid.NewGuid():N}";
                            string fileName = _imageUploadService.UploadImage(upload.UploadFile_Base64, folderPath, filePrefix, fileType);
                            uploadedFiles.Add(new { FileName = fileName});
                        }

                        if (uploadedFiles.Any())
                        {
                            string documentNameJSON = JsonConvert.SerializeObject(uploadedFiles);
                            using (SqlCommand fileCmd = new SqlCommand("Save_DocumentFile", con))
                            {
                                fileCmd.CommandType = CommandType.StoredProcedure;
                                fileCmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = bookingId;
                                fileCmd.Parameters.Add("@DocumentNameJSON", SqlDbType.NVarChar, -1).Value = documentNameJSON;
                                fileCmd.Parameters.Add("@TableName", SqlDbType.NVarChar, 100).Value = "BookingCalendar";
                                fileCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    #endregion

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
        [Route("DeleteBookingCalendar_Upload")]
        public IActionResult DeleteBookingCalendar_Upload([FromBody] DeleteBookingCalendarUploadRequest model)
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
                    using (SqlCommand cmd = new SqlCommand("DeleteBookingCalendar_Upload", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = model.BookingUploadId;
                        cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                // 🔹 Success response
                apiResponse.message = "Booking Upload deleted successfully!";
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                // 🔹 Log and return error
                ErrorLogger.LogException(ex, ControllerContext, Request, null);
                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                return StatusCode(200, apiResponse);
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
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetPackage_MenuAndFoodItemsList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@package_id", SqlDbType.BigInt).Value = request.Package_Id ?? (object)DBNull.Value;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);

                        foreach (DataRow dr in dt.Rows)
                        {
                            var menu = new FoodMenuModel
                            {
                                FoodMenu_Id = dr["FoodMenu_Id"] == DBNull.Value ? null : Convert.ToInt64(dr["FoodMenu_Id"]),
                                FoodMenu_Name = dr["FoodMenu_Name"] == DBNull.Value ? null : Convert.ToString(dr["FoodMenu_Name"]),
                                Inclusion_Id = dr["Inclusion_Id"] == DBNull.Value ? null : Convert.ToInt64(dr["Inclusion_Id"]),
                                Count_Size = dr["Count_Size"] == DBNull.Value ? 0 : Convert.ToInt64(dr["Count_Size"]),
                                FoodItemsList = new List<FoodItemModel>()
                            };

                            string jsonFoodItems = dr["FoodItemsList"] == DBNull.Value ? "[]" : Convert.ToString(dr["FoodItemsList"]);
                            if (!string.IsNullOrWhiteSpace(jsonFoodItems))
                            {
                                try
                                {
                                    menu.FoodItemsList = JsonConvert.DeserializeObject<List<FoodItemModel>>(jsonFoodItems) ?? new List<FoodItemModel>();
                                }
                                catch
                                {
                                    menu.FoodItemsList = new List<FoodItemModel>();
                                }
                            }

                            menuList.Add(menu);
                        }
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

        // Bookings Notes - MASTER DATA
        [HttpGet]
        [Route("GetBookingCalendar_NotesMaster")]
        public IActionResult GetBookingCalendar_NotesMaster()
        {
            apiResponse apiResponse = new apiResponse();
            List<BookingNoteCategoryModel_Master> list = new();

            string connString = string.Empty;

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("GetBookingCalendar_NotesMaster", con);

                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();

                DataSet ds = new DataSet();

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(ds);
                }

                if (ds.Tables.Count < 3)
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid data returned from database.";
                    return BadRequest(apiResponse);
                }

                DataTable categories = ds.Tables[0];
                DataTable subCategories = ds.Tables[1];
                DataTable options = ds.Tables[2];

                // Build lookup for options
                var optionLookup = options.AsEnumerable()
                    .GroupBy(x => Convert.ToInt64(x["SubCategoryId"]))
                    .ToDictionary(g => g.Key, g => g.ToList());

                // Build lookup for sub categories
                var subCategoryLookup = subCategories.AsEnumerable()
                    .GroupBy(x => Convert.ToInt64(x["CategoryId"]))
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (DataRow categoryRow in categories.Rows)
                {
                    long categoryId = Convert.ToInt64(categoryRow["CategoryId"]);

                    var category = new BookingNoteCategoryModel_Master
                    {
                        CategoryId = categoryId,
                        Name = Convert.ToString(categoryRow["Name"]),
                        SortOrder = Convert.ToInt32(categoryRow["SortOrder"]),
                        SubCategories = new List<BookingNoteSubCategoryModel_Master>()
                    };

                    if (subCategoryLookup.TryGetValue(categoryId, out var subCategoryRows))
                    {
                        foreach (var subCategoryRow in subCategoryRows)
                        {
                            long subCategoryId = Convert.ToInt64(subCategoryRow["SubCategoryId"]);

                            var subCategory = new BookingNoteSubCategoryModel_Master
                            {
                                SubCategoryId = subCategoryId,
                                CategoryId = Convert.ToInt64(subCategoryRow["CategoryId"]),
                                Name = Convert.ToString(subCategoryRow["Name"]),
                                InputType = Convert.ToString(subCategoryRow["InputType"]),
                                SortOrder = Convert.ToInt32(subCategoryRow["SortOrder"]),
                                Options = new List<BookingNoteOptionModel_Master>()
                            };

                            if (optionLookup.TryGetValue(subCategoryId, out var optionRows))
                            {
                                foreach (var optionRow in optionRows)
                                {
                                    subCategory.Options.Add(new BookingNoteOptionModel_Master
                                    {
                                        SubCategoryOptionId = Convert.ToInt64(optionRow["SubCategoryOptionId"]),
                                        SubCategoryId = Convert.ToInt64(optionRow["SubCategoryId"]),
                                        OptionName = Convert.ToString(optionRow["OptionName"]),
                                        SortOrder = Convert.ToInt32(optionRow["SortOrder"])
                                    });
                                }
                            }

                            category.SubCategories.Add(subCategory);
                        }
                    }

                    list.Add(category);
                }

                apiResponse.status = "Success";
                apiResponse.message = "Notes master fetched successfully.";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        #region MUHURAT
        [HttpPost]
        [Route("SaveMuhurat")]
        public IActionResult SaveMuhurat([FromBody] SaveMuhuratModel model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long muhuratId = 0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveMuhurat", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@MuhuratDate", model.MuhuratDate);
                    cmd.Parameters.AddWithValue("@StartTime", model.StartTime);
                    cmd.Parameters.AddWithValue("@EndTime", model.EndTime);
                    cmd.Parameters.AddWithValue("@Title", model.Title ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();

                    object result = cmd.ExecuteScalar();
                    muhuratId = Convert.ToInt64(result);
                }

                apiResponse.status = "Success";
                apiResponse.data = new { Id = muhuratId };
                apiResponse.message = "Muhurat saved successfully.";

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
        [Route("DeleteMuhurat")]
        public IActionResult DeleteMuhurat([FromBody] DeleteMuhuratModel model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteMuhurat", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.data = null;
                apiResponse.message = "Muhurat deleted successfully.";

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
        [Route("GetMuhurat")]
        public IActionResult GetMuhurat([FromBody] GetMuhuratModelRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetMuhurat", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@MuhuratDate", string.IsNullOrWhiteSpace(model.MuhuratDate) ? (object)DBNull.Value : model.MuhuratDate);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                List<GetMuhuratModelResponse> list = new List<GetMuhuratModelResponse>();

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new GetMuhuratModelResponse
                    {
                        Id = Convert.ToInt32(row["Id"]),

                        MuhuratDate = Convert.ToString(row["MuhuratDate"]) ?? "",
                        MuhuratDate1 = Convert.ToString(row["MuhuratDate1"]) ?? "",

                        StartTime = Convert.ToString(row["StartTime"]) ?? "",
                        StartTime1 = Convert.ToString(row["StartTime1"]) ?? "",

                        EndTime = Convert.ToString(row["EndTime"]) ?? "",
                        EndTime1 = Convert.ToString(row["EndTime1"]) ?? "",

                        Title = Convert.ToString(row["Title"]) ?? ""
                    });
                }

                apiResponse.status = "Success";
                apiResponse.data = list;
                apiResponse.message = "Muhurat fetched successfully.";

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
        #endregion
    }
}
