using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Presentation;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using static ERPAdmin.ApiControllers.ReportController;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Booking_PaymentsController : MasterController
    {

        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;
        private readonly string domainUrl;

        public Booking_PaymentsController(IConfiguration configuration, IWebHostEnvironment env,  IHttpContextAccessor httpContextAccessor, ImageUploadService imageUploadService) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("SaveBookingPayment")]
        public IActionResult SaveBookingPayment([FromBody] SaveBookingPaymentRequest model)
        {
            apiResponse apiResponse = new apiResponse();

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

                long paymentId = 0;

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBooking_Payments", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = model.BookingId;
                    cmd.Parameters.Add("@AccountId", SqlDbType.BigInt).Value = model.AccountId;
                    cmd.Parameters.Add("@ModeId", SqlDbType.BigInt).Value = model.ModeId;
                    cmd.Parameters.Add("@DateOfPayment", SqlDbType.VarChar).Value = model.DateOfPayment;
                    cmd.Parameters.Add("@PaymentTime", SqlDbType.VarChar).Value = (object)model.PaymentTime ?? DBNull.Value;
                    cmd.Parameters.Add("@TxnId", SqlDbType.VarChar, 100).Value = (object)model.TxnId ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeBank", SqlDbType.VarChar, 100).Value = (object)model.ChequeBank ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeNo", SqlDbType.VarChar, 100).Value = (object)model.ChequeNo ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeDate", SqlDbType.VarChar).Value = (object)model.ChequeDate ?? DBNull.Value;
                    var amountParam = new SqlParameter("@Amount", SqlDbType.Decimal)
                    {
                        Precision = 18,
                        Scale = 2,
                        Value = model.Amount
                    };
                    cmd.Parameters.Add(amountParam);
                    var dueParam = new SqlParameter("@DueBalance", SqlDbType.Decimal)
                    {
                        Precision = 18,
                        Scale = 2,
                        Value = model.DueBalance
                    };
                    cmd.Parameters.Add(dueParam);
                    cmd.Parameters.Add("@NextDueDate", SqlDbType.VarChar, 20).Value = (object)model.NextDueDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;
                    cmd.Parameters.Add("@PaymentNotes", SqlDbType.NVarChar, 500).Value = (object)model.PaymentNotes ?? DBNull.Value;

                    var outputParam = new SqlParameter("@PaymentId", SqlDbType.BigInt)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(outputParam);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    paymentId = outputParam.Value != DBNull.Value ? Convert.ToInt64(outputParam.Value) : 0;
                }

                apiResponse.status = "Success";
                apiResponse.message = "Payment saved successfully!";
                apiResponse.data = new { PaymentId = paymentId };

                return Ok(apiResponse);   // 🔥 200 OK
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
        [Route("GetBookingPayments")]
        public IActionResult GetBookingPayments([FromBody] GetBookingPayments_Request request)
        {
            apiResponse apiResponse = new apiResponse();
            List<BookingPayment> list = new List<BookingPayment>();
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

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetBooking_Payments", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@PaymentId", SqlDbType.BigInt).Value =
                        (object)request.PaymentId ?? DBNull.Value;

                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value =
                        (object)request.BookingId ?? DBNull.Value;

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        BookingPayment model = new BookingPayment
                        {
                            PaymentId = Convert.ToInt64(row["PaymentId"]),
                            BookingId = Convert.ToInt64(row["BookingId"]),
                            ModeId = Convert.ToInt64(row["ModeId"]),
                            AccountId = Convert.ToInt64(row["AccountId"]),
                            DateOfPayment = row["DateOfPayment"].ToString(),
                            PaymentTime = row["PaymentTime"].ToString(),
                            PaymentDateTimeFormat = row["PaymentDateTimeFormat"].ToString(),
                            TxnId = Convert.ToString(row["TxnId"]),
                            ChequeBank = Convert.ToString(row["ChequeBank"]),
                            ChequeNo = Convert.ToString(row["ChequeNo"]),
                            ChequeDate = row["ChequeDate"].ToString(),
                            Amount = Convert.ToDecimal(row["Amount"]),
                            DueBalance = Convert.ToDecimal(row["DueBalance"]),
                            NextDueDate = row["NextDueDate"].ToString(),
                            PaymentNotes = row["PaymentNotes"].ToString()
                        };
                        list.Add(model);
                    }
                }

                apiResponse.message = "Booking payments fetched successfully!";
                apiResponse.data = list;
                apiResponse.status = "Success";
                return Ok(apiResponse);   // ✅ 200 OK
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DownloadPaymentReceipt_OLD")]
        private IActionResult DownloadPaymentReceipt_OLD(PaymentReceiptRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                DataTable dtCompany = new DataTable();
                DataTable dtReceipt = new DataTable();
                DataTable dtPayments = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // COMPANY
                    using (SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                        new SqlDataAdapter(cmd).Fill(dtCompany);
                    }

                    // RECEIPTS
                    using (SqlCommand cmd = new SqlCommand("Report_PaymentReceipt", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PaymentId", request.PaymentId);
                        cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                        new SqlDataAdapter(cmd).Fill(dtReceipt);
                    }

                    // FOR PAYMENTS HISTORY
                    using (SqlCommand cmd = new SqlCommand("GetBooking_Payments", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PaymentId", 0);
                        cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                        new SqlDataAdapter(cmd).Fill(dtPayments);
                    }
                }

                if (dtReceipt.Rows.Count == 0)
                    throw new Exception("Receipt data not found.");

                // ================= COMPANY =================
                string companyName = "";
                string companyAddress = "";
                string companyPhone = "";
                string logoFile = "";
                string userName = "";
                Image logo = null;

                if (dtCompany.Rows.Count > 0)
                {
                    var row = dtCompany.Rows[0];

                    companyName = row["CompanyName"]?.ToString() ?? "";
                    companyAddress = row["CompanyAddress"]?.ToString() ?? "";
                    companyPhone = row["CompanyMobile"]?.ToString() ?? "";
                    logoFile = row["CompanyLogo"]?.ToString() ?? "";
                    userName = row["UserName"]?.ToString() ?? "";

                    if (!string.IsNullOrWhiteSpace(logoFile))
                    {
                        string path = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);
                        if (System.IO.File.Exists(path))
                            logo = Image.GetInstance(path);
                    }
                }

                // ================= FONTS =================
                iTextSharp.text.Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 10, iTextSharp.text.Font.NORMAL);  //FontFactory.GetFont(FontFactory.HELVETICA, 10);

                // ================= HEADER METHOD =================
                PdfPTable BuildHeader()
                {
                    PdfPTable tbl = new PdfPTable(2);
                    tbl.WidthPercentage = 100;
                    tbl.SetWidths(new float[] { 20f, 80f });

                    PdfPCell logoCell = logo != null
                        ? new PdfPCell(logo)
                        : new PdfPCell(new Phrase(""));

                    logoCell.Border = Rectangle.BOTTOM_BORDER;
                    logoCell.Padding = 10;

                    if (logo != null)
                        logo.ScaleToFit(70, 70);

                    tbl.AddCell(logoCell);

                    PdfPCell detail = new PdfPCell
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        Padding = 10,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };

                    detail.AddElement(new Paragraph(companyName, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                    detail.AddElement(new Paragraph(companyAddress, FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                    detail.AddElement(new Paragraph(companyPhone, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                    tbl.AddCell(detail);

                    return tbl;
                }

                writer.PageEvent = new PdfPageEvents(userName);
                document.Open();

                // ================= RECEIPTS =================
                for (int i = 0; i < dtReceipt.Rows.Count; i++)
                {
                    var r = dtReceipt.Rows[i];

                    document.Add(BuildHeader());

                    // TITLE
                    PdfPTable title = new PdfPTable(1);
                    title.WidthPercentage = 100;

                    PdfPCell tCell = new PdfPCell(new Phrase("PAYMENT RECEIPT",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.WHITE)))
                    {
                        BackgroundColor = new BaseColor(0, 102, 204),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 10,
                        Border = Rectangle.NO_BORDER
                    };

                    title.AddCell(tCell);
                    document.Add(title);
                    document.Add(new Paragraph("\n"));

                    // TABLE
                    PdfPTable tbl = new PdfPTable(2);
                    tbl.WidthPercentage = 100;
                    tbl.SetWidths(new float[] { 35, 65 });

                    iTextSharp.text.Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    BaseColor bg = new BaseColor(235, 238, 255);

                    void AddRow(string l, string v, iTextSharp.text.Font value_Font)
                    {
                        tbl.AddCell(new PdfPCell(new Phrase(l, labelFont)) { BackgroundColor = bg, Padding = 6 });
                        tbl.AddCell(new PdfPCell(new Phrase(v ?? "-", value_Font)) { Padding = 6 });
                    }

                    AddRow("Transaction Id", r["PaymentId"]?.ToString(), normalFont);
                    AddRow("Customer Name", r["CustomerName"]?.ToString(), normalFont);
                    AddRow("Payment Date", r["PaymentDate"]?.ToString(), normalFont);
                    AddRow("Account Name", r["AccountName"]?.ToString(), normalFont);
                    AddRow("Payment Mode", r["PaymentModeName"]?.ToString(), normalFont);

                    AddRow("Total Amount", $"₹ {Convert.ToDecimal(r["TotalAmount"] ?? 0):N2}", AmtFontNormal);
                    AddRow("Paid Previously", $"₹ {Convert.ToDecimal(r["TotalPaidPreviously"] ?? 0):N2}", AmtFontNormal);
                    AddRow("Discount Previously", $"₹ {Convert.ToDecimal(r["TotalDiscountPreviously"] ?? 0):N2}", AmtFontNormal);
                    AddRow("Paid Amount", $"₹ {Convert.ToDecimal(r["PaidAmount"] ?? 0):N2}", AmtFontNormal);
                    AddRow("Discount", $"₹ {Convert.ToDecimal(r["Discount"] ?? 0):N2}", AmtFontNormal);
                    AddRow("Remaining", $"₹ {Convert.ToDecimal(r["RemainingAmount"] ?? 0):N2}", AmtFontNormal);

                    document.Add(tbl);

                    // ================= PAYMENT TRAILS =================
                    if (dtPayments.Rows.Count > 0)
                    {
                        //document.NewPage();

                        //document.Add(BuildHeader());

                        document.Add(new Paragraph("\n"));
                        title = new PdfPTable(1);
                        title.WidthPercentage = 100;

                        title.AddCell(new PdfPCell(new Phrase("PAYMENT HISTORY",
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(0, 102, 204))))
                        {
                            BackgroundColor = BaseColor.WHITE,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 10,
                            Border = Rectangle.NO_BORDER
                        });

                        document.Add(title);
                        //document.Add(new Paragraph("\n"));

                        PdfPTable payTbl = new PdfPTable(5);
                        payTbl.WidthPercentage = 100;
                        payTbl.SetWidths(new float[] { 20, 20, 25, 15, 20 });
                        payTbl.HeaderRows = 1;

                        void H(string t)
                        {
                            payTbl.AddCell(new PdfPCell(new Phrase(t, boldFont))
                            {
                                BackgroundColor = new BaseColor(230, 230, 230),
                                HorizontalAlignment = Element.ALIGN_CENTER
                            });
                        }

                        H("Date"); H("Mode"); H("Reference"); H("Amount"); H("Balance");

                        foreach (DataRow pr in dtPayments.Rows)
                        {
                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentDateTimeFormat"].ToString(), normalFont))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentModeName"].ToString(), normalFont))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentId"].ToString(), normalFont)));

                            payTbl.AddCell(new PdfPCell(new Phrase(
                                $"₹ {Convert.ToDecimal(pr["Amount"]).ToString("N0")}", AmtFontNormal))
                            {
                                HorizontalAlignment = Element.ALIGN_RIGHT
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase(
                                $"₹ {Convert.ToDecimal(pr["DueBalance"]).ToString("N0")}", AmtFontNormal))
                            {
                                HorizontalAlignment = Element.ALIGN_RIGHT
                            });

                        }

                        document.Add(payTbl);
                    }

                    //NEW PAGE
                    if (i < dtReceipt.Rows.Count - 1)
                        document.NewPage();
                }

                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "PaymentReceipt_*.pdf");

                string fileName = request.PaymentId > 0
                    ? $"PaymentReceipt_PId_{request.PaymentId}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
                    : $"PaymentReceipt_BId_{request.BookingId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                string pathFile = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(pathFile, FileMode.Create))
                {
                    stream.WriteTo(fs);
                }

                response.status = "Success";
                response.message = "Receipt Generated";
                response.data = new { FileUrl = $"{domainUrl}/reports/{fileName}" };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("DownloadPaymentReceipt")]
        public IActionResult DownloadPaymentReceipt([FromBody] DownloadBookingReceiptRequest request)
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

                using MemoryStream stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 25, 25, 20, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                DataTable dtCompany = new DataTable();
                DataTable dt = new DataTable();
                DataTable dtPayments = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd;

                    // COMPANY
                    cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // RECEIPTS
                    cmd = new SqlCommand("Report_GetBookingPaymentReceipt", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@PaymentId", SqlDbType.BigInt).Value = request.PaymentId;
                    new SqlDataAdapter(cmd).Fill(dt);

                    // PAYMENTS
                    cmd = new SqlCommand("Report_GetBooking_Payments", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@PaymentId", SqlDbType.BigInt).Value = 0;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    new SqlDataAdapter(cmd).Fill(dtPayments);
                }

                if (dt.Rows.Count == 0)
                    throw new Exception("No receipt data found.");

                /* ================= FONTS ================= */
                iTextSharp.text.Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold10 = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font AmtFontBold13 = new iTextSharp.text.Font(bf, 13, iTextSharp.text.Font.BOLD);

                /* ================= HEADER METHOD ================= */
                PdfPTable BuildHeader()
                {
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 20f, 80f });

                    Image logo = null;
                    string logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(logoFile))
                    {
                        string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);
                        if (System.IO.File.Exists(logoPath))
                            logo = Image.GetInstance(logoPath);
                    }

                    PdfPCell logoCell = logo != null
                        ? new PdfPCell(logo)
                        : new PdfPCell(new Phrase(""));

                    if (logo != null)
                        logo.ScaleToFit(70, 70);

                    logoCell.Border = Rectangle.BOTTOM_BORDER;
                    logoCell.Padding = 10;

                    headerTable.AddCell(logoCell);

                    PdfPCell detailCell = new PdfPCell
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        Padding = 10,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };

                    detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyName"]?.ToString(),
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));

                    detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyAddress"]?.ToString(),
                        FontFactory.GetFont(FontFactory.HELVETICA, 9)));

                    detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyMobile"]?.ToString(),
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                    headerTable.AddCell(detailCell);

                    return headerTable;
                }

                document.Open();

                /* ================= LOOP RECEIPTS ================= */
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow r = dt.Rows[i];

                    document.Add(BuildHeader());

                    /* ================= DATA ================= */
                    string receiptNo = r["PaymentId"]?.ToString();
                    string receiptDate = r["PaymentDateTime"]?.ToString();

                    string customerName = r["Name"]?.ToString();
                    string customerMobile = r["Mobile"]?.ToString();

                    string eventName = r["EventName"]?.ToString();
                    string subEventName = r["SubEventName"]?.ToString();
                    string eventDate = r["EventDate"]?.ToString();
                    string hallName = r["HallName"]?.ToString();

                    string startTime = r["StartTime_Full"]?.ToString();
                    string shift = r["ShiftFullName"]?.ToString();

                    /* ================= TITLE ================= */
                    PdfPTable titleTable = new PdfPTable(1);
                    titleTable.WidthPercentage = 100;

                    titleTable.AddCell(new PdfPCell(
                        new Phrase("PAYMENT RECEIPT",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.WHITE)))
                    {
                        BackgroundColor = new BaseColor(0, 102, 204),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 6,
                        Border = Rectangle.NO_BORDER
                    });

                    document.Add(titleTable);

                    /* ================= TOP ================= */
                    PdfPTable top = new PdfPTable(2);
                    top.WidthPercentage = 100;
                    top.SetWidths(new float[] { 70, 30 });

                    top.AddCell(new PdfPCell(new Phrase($"RECEIPT NO: {receiptNo}", boldFont))
                    { Border = Rectangle.NO_BORDER });

                    top.AddCell(new PdfPCell(new Phrase($"DATE: {receiptDate}", boldFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });

                    document.Add(top);

                    /* ================= DETAILS ================= */
                    PdfPTable tbl = new PdfPTable(3);
                    tbl.WidthPercentage = 100;
                    tbl.SetWidths(new float[] { 25, 3, 72 });

                    void AddRow(string label, string value)
                    {
                        tbl.AddCell(new PdfPCell(new Phrase(label, boldFont)) { Border = Rectangle.NO_BORDER });

                        tbl.AddCell(new PdfPCell(new Phrase(":", boldFont))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });

                        tbl.AddCell(new PdfPCell(new Phrase(value ?? "-", normalFont))
                        {
                            Border = Rectangle.BOTTOM_BORDER,
                            PaddingBottom = 5
                        });
                    }

                    AddRow("Received with thanks from", customerName);
                    AddRow("", $"Mobile: {customerMobile}");

                    AddRow("Event details", $"Event : {eventName} ({subEventName})");
                    AddRow("", $"Event Date : {eventDate}");
                    AddRow("", $"Hall(s) : {hallName} at {startTime} ({shift})");

                    document.Add(tbl);

                    /* ================= PAYMENT DETAILS ================= */

                    decimal totalAmout = r["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalAmount"]);
                    decimal paidTillDate = r["PaidTillDate"] == DBNull.Value ? 0 : Convert.ToDecimal(r["PaidTillDate"]);
                    decimal thisPayment = r["ThisPayment"] == DBNull.Value ? 0 : Convert.ToDecimal(r["ThisPayment"]);
                    decimal totalPaid = r["TotalPaid"] == DBNull.Value ? 0 : Convert.ToDecimal(r["TotalPaid"]);
                    decimal outstandingAmount = r["OutstandingAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(r["OutstandingAmount"]);

                    //document.Add(new Paragraph("\n"));

                    /* ================= PAYMENT DETAILS ================= */

                    PdfPTable paySummary = new PdfPTable(3);
                    paySummary.WidthPercentage = 100;
                    paySummary.SetWidths(new float[] { 25, 3, 72 });

                    // 🔹 Title Row (full width)
                    PdfPCell titleCell = new PdfPCell(new Phrase("Payment Details", boldFont))
                    {
                        Colspan = 3,
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10,
                        PaddingBottom = 5
                    };
                    paySummary.AddCell(titleCell);

                    // 🔹 Helper (same pattern as above table)
                    void AddPayRow(string label, string value)
                    {
                        paySummary.AddCell(new PdfPCell(new Phrase(label, normalFont))
                        {
                            Border = Rectangle.NO_BORDER
                        });

                        paySummary.AddCell(new PdfPCell(new Phrase(":", normalFont))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });

                        paySummary.AddCell(new PdfPCell(new Phrase(value, AmtFontNormal))
                        {
                            Border = Rectangle.BOTTOM_BORDER,
                            PaddingBottom = 5,
                            HorizontalAlignment = Element.ALIGN_RIGHT   // 💡 amount right aligned
                        });
                    }

                    // 🔹 Data
                    AddPayRow("Total Booking Amount", $"₹ {totalAmout:N0}");
                    AddPayRow("Paid Till Date", $"₹ {paidTillDate:N0}");
                    AddPayRow("This Payment", $"₹ {thisPayment:N0}");
                    AddPayRow("Total Paid", $"₹ {totalPaid:N0}");
                    AddPayRow("Outstanding Amount", $"₹ {outstandingAmount:N0}");

                    document.Add(paySummary);

                    document.Add(new Paragraph("\n"));

                    /* ================= AMOUNT BOX ================= */
                    PdfPTable amtTbl = new PdfPTable(2);
                    amtTbl.WidthPercentage = 100;
                    amtTbl.SetWidths(new float[] { 30, 70 });

                    amtTbl.AddCell(new PdfPCell(new Phrase($"₹ {thisPayment:N0} /-", AmtFontBold13))
                    {
                        Border = Rectangle.BOX,
                        FixedHeight = 35,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE
                    });

                    amtTbl.AddCell(new PdfPCell(new Phrase("Authorised Signatory", normalFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        VerticalAlignment = Element.ALIGN_BOTTOM
                    });

                    document.Add(amtTbl);

                    /* ================= PAYMENT HISTORY ================= */
                    if (dtPayments.Rows.Count > 0)
                    {
                        document.Add(new Paragraph("\n"));

                        // HISTORY TITLE
                        PdfPTable title2 = new PdfPTable(1);
                        title2.WidthPercentage = 100;

                        PdfPCell titleCell2 = new PdfPCell(
                            new Phrase("ALL PAYMENT HISTORY",
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(0, 102, 204)))
                        );
                        titleCell2.BackgroundColor = BaseColor.WHITE;
                        titleCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                        titleCell2.Padding = 6;
                        titleCell2.Border = Rectangle.BOX;
                        //titleCell2.BorderColor = new BaseColor(0, 102, 204);

                        title2.AddCell(titleCell2);
                        document.Add(title2);
                        // HISTORY TITLE

                        PdfPTable payTbl = new PdfPTable(6);
                        payTbl.WidthPercentage = 100;
                        payTbl.SetWidths(new float[] { 15, 20, 15, 15, 15, 20 });
                        payTbl.HeaderRows = 1;

                        void H(string t)
                        {
                            payTbl.AddCell(new PdfPCell(new Phrase(t, boldFont))
                            {
                                BackgroundColor = new BaseColor(230, 230, 230),
                                HorizontalAlignment = Element.ALIGN_CENTER
                            });
                        }

                        H("Receipt No"); H("Date & Time"); H("Mode"); H("Amount"); H("Balance"); H("Notes");

                        decimal totalAmount = 0;
                        foreach (DataRow pr in dtPayments.Rows)
                        {
                            decimal amt = pr["Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(pr["Amount"]);
                            decimal bal = pr["DueBalance"] == DBNull.Value ? 0 : Convert.ToDecimal(pr["DueBalance"]);
                            totalAmount += amt;

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentId"]?.ToString() ?? "-", normalFont))
                            { HorizontalAlignment = Element.ALIGN_CENTER });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentDateTimeFormat"]?.ToString() ?? "-", normalFont))
                            { HorizontalAlignment = Element.ALIGN_CENTER });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentModeName"]?.ToString() ?? "-", normalFont))
                            { HorizontalAlignment = Element.ALIGN_CENTER });

                            payTbl.AddCell(new PdfPCell(new Phrase($"₹ {amt:N0}", AmtFontNormal))
                            { HorizontalAlignment = Element.ALIGN_RIGHT });

                            payTbl.AddCell(new PdfPCell(new Phrase($"₹ {bal:N0}", AmtFontNormal))
                            { HorizontalAlignment = Element.ALIGN_RIGHT });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentNotes"]?.ToString() ?? "-", normalFont))
                            { HorizontalAlignment = Element.ALIGN_LEFT });
                        }

                        // 🔹 Total Amount TITLE
                        payTbl.AddCell(new PdfPCell(new Phrase($"TOTAL", AmtFontNormal))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(230, 230, 230),
                            Padding = 5,
                            Colspan=3
                        });

                        // 🔹 Total Amount (Amount column)
                        payTbl.AddCell(new PdfPCell(new Phrase($"₹ {totalAmount:N0}", AmtFontNormal))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(230, 230, 230),
                            Padding = 5
                        });

                        // 🔹 Empty Balance column (optional)
                        payTbl.AddCell(new PdfPCell(new Phrase(""))
                        {
                            BackgroundColor = new BaseColor(230, 230, 230)
                        });

                        // 🔹 Empty Notes column (optional)
                        payTbl.AddCell(new PdfPCell(new Phrase(""))
                        {
                            BackgroundColor = new BaseColor(230, 230, 230)
                        });

                        document.Add(payTbl);
                    }

                    // NEW PAGE
                    if (i < dt.Rows.Count - 1)
                        document.NewPage();
                }

                document.Close();

                /* ================= SAVE ================= */
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Payment_Receipt_*.pdf");

                string fileName = $"Payment_Receipt_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fs);
                }

                response.status = "Success";
                response.message = "Receipt Generated Successfully";
                response.data = new { FileUrl = $"{domainUrl}/reports/{fileName}" };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

    }
}
