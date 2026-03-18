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
        [Route("DownloadPaymentReceipt")]
        public IActionResult DownloadPaymentReceipt(PaymentReceiptRequest request)
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

                    PdfPCell tCell = new PdfPCell(new Phrase("TRANSACTION RECEIPT",
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

                    void AddRow(string l, string v)
                    {
                        tbl.AddCell(new PdfPCell(new Phrase(l, labelFont)) { BackgroundColor = bg, Padding = 6 });
                        tbl.AddCell(new PdfPCell(new Phrase(v ?? "-", normalFont)) { Padding = 6 });
                    }

                    AddRow("Customer Name", r["CustomerName"]?.ToString());
                    AddRow("Payment Date", r["PaymentDate"]?.ToString());
                    AddRow("Account Name", r["AccountName"]?.ToString());
                    AddRow("Payment Mode", r["PaymentModeName"]?.ToString());

                    AddRow("Total Amount", $"Rs. {Convert.ToDecimal(r["TotalAmount"] ?? 0):N2}");
                    AddRow("Paid Previously", $"Rs. {Convert.ToDecimal(r["TotalPaidPreviously"] ?? 0):N2}");
                    AddRow("Discount Previously", $"Rs. {Convert.ToDecimal(r["TotalDiscountPreviously"] ?? 0):N2}");
                    AddRow("Paid Amount", $"Rs. {Convert.ToDecimal(r["PaidAmount"] ?? 0):N2}");
                    AddRow("Discount", $"Rs. {Convert.ToDecimal(r["Discount"] ?? 0):N2}");
                    AddRow("Remaining", $"Rs. {Convert.ToDecimal(r["RemainingAmount"] ?? 0):N2}");

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
                            payTbl.AddCell(new Phrase(pr["PaymentDateTimeFormat"]?.ToString() ?? "-", normalFont));
                            payTbl.AddCell(new Phrase(pr["PaymentModeName"]?.ToString() ?? "-", normalFont));
                            payTbl.AddCell(new Phrase(pr["TxnId"]?.ToString() ?? "-", normalFont));

                            payTbl.AddCell(new PdfPCell(new Phrase(Convert.ToDecimal(pr["Amount"] ?? 0).ToString("N0"), normalFont))
                            { HorizontalAlignment = Element.ALIGN_RIGHT });

                            payTbl.AddCell(new PdfPCell(new Phrase(Convert.ToDecimal(pr["DueBalance"] ?? 0).ToString("N0"), normalFont))
                            { HorizontalAlignment = Element.ALIGN_RIGHT });
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
    }
}
