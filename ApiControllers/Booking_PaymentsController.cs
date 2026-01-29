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
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                long paymentId = 0;

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveBooking_Payments", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = model.BookingId;
                    cmd.Parameters.Add("@ModeId", SqlDbType.BigInt).Value = model.ModeId;
                    cmd.Parameters.Add("@AccountId", SqlDbType.BigInt).Value = model.AccountId;
                    cmd.Parameters.Add("@DateOfPayment", SqlDbType.VarChar).Value = model.DateOfPayment;
                    cmd.Parameters.Add("@PaymentTime", SqlDbType.VarChar).Value = (object)model.PaymentTime ?? DBNull.Value;
                    cmd.Parameters.Add("@TxnId", SqlDbType.VarChar).Value = (object)model.TxnId ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeBank", SqlDbType.VarChar).Value = (object)model.ChequeBank ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeNo", SqlDbType.VarChar).Value = (object)model.ChequeNo ?? DBNull.Value;
                    cmd.Parameters.Add("@ChequeDate", SqlDbType.VarChar).Value = (object)model.ChequeDate ?? DBNull.Value;
                    cmd.Parameters.Add("@Amount", SqlDbType.Decimal).Value = model.Amount;
                    cmd.Parameters.Add("@DueBalance", SqlDbType.Decimal).Value = model.DueBalance;
                    cmd.Parameters.Add("@NextDueDate", SqlDbType.VarChar).Value = (object)model.NextDueDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.VarChar).Value = model.ActionBy;

                    conn.Open();
                    object result = cmd.ExecuteScalar(); // 🔥 PaymentId return
                    conn.Close();

                    if (result != null)
                        paymentId = Convert.ToInt64(result);
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
                        out IActionResult errorResult))
                {
                    return errorResult;
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
                            DateOfPayment = row["DateOfPayment"].ToString(),
                            PaymentTime = row["PaymentTime"].ToString(),
                            PaymentDateTimeFormat = row["PaymentDateTimeFormat"].ToString(),
                            TxnId = Convert.ToString(row["TxnId"]),
                            ChequeBank = Convert.ToString(row["ChequeBank"]),
                            ChequeNo = Convert.ToString(row["ChequeNo"]),
                            ChequeDate = row["ChequeDate"].ToString(),
                            Amount = Convert.ToDecimal(row["Amount"]),
                            DueBalance = Convert.ToDecimal(row["DueBalance"]),
                            NextDueDate = row["NextDueDate"].ToString()
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

                apiResponse.message = "Error details: " + ex.Message;
                apiResponse.status = "Error";
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DownloadPaymentReceipt2")]
        public IActionResult DownloadPaymentReceipt2(PaymentReceiptRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataTable dtReceipt = new DataTable();
                DataRow r;   // ✅ moved outside

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // ================= COMPANY =================
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // ================= RECEIPT =================
                    using (SqlCommand cmd = new SqlCommand("Report_PaymentReceipt", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PaymentId", request.PaymentId);
                        cmd.Parameters.AddWithValue("@BookingId", request.BookingId);

                        new SqlDataAdapter(cmd).Fill(dtReceipt);
                    }

                    if (dtReceipt.Rows.Count == 0)
                        throw new Exception("Receipt data not found");

                    r = dtReceipt.Rows[0]; // ✅ safe
                }

                // ================= HEADER DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);

                // ================= TITLE =================
                PdfPTable titleTable = new PdfPTable(1);
                titleTable.WidthPercentage = 100;

                PdfPCell titleCell = new PdfPCell(new Phrase("TRANSACTION RECEIPT",FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.WHITE)));

                titleCell.BackgroundColor = new BaseColor(0, 102, 204); // Blue
                titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                titleCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                titleCell.PaddingTop = 10;
                titleCell.PaddingBottom = 10;
                titleCell.Border = Rectangle.NO_BORDER;
                titleTable.AddCell(titleCell);

                // spacing above & below
                document.Add(new Paragraph("\n"));
                document.Add(titleTable);
                document.Add(new Paragraph("\n"));

                // ================= RECEIPT TABLE =================
                PdfPTable receiptTable = new PdfPTable(2);
                receiptTable.WidthPercentage = 100;
                receiptTable.SetWidths(new float[] { 35f, 65f });

                Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                BaseColor labelBg = new BaseColor(235, 238, 255);

                void AddRow(string label, string value)
                {
                    receiptTable.AddCell(new PdfPCell(new Phrase(label, labelFont))
                    {
                        BackgroundColor = labelBg,
                        Padding = 6
                    });

                    receiptTable.AddCell(new PdfPCell(new Phrase(value, valueFont))
                    {
                        Padding = 6
                    });
                }

                // ================= DATA =================
                AddRow("Customer Name", r["CustomerName"].ToString());
                AddRow("Payment Date", r["PaymentDate"].ToString());
                AddRow("Account Name", r["AccountName"].ToString());
                AddRow("Account Type", r["AccountType"].ToString());
                AddRow("Total Amount", $"Rs. {Convert.ToDecimal(r["TotalAmount"]):N2}");
                AddRow("Total Paid (Previously)", $"Rs. {Convert.ToDecimal(r["TotalPaidPreviously"]):N2}");
                AddRow("Total Discount (Previously)", $"Rs. {Convert.ToDecimal(r["TotalDiscountPreviously"]):N2}");
                AddRow("Paid Amount", $"Rs. {Convert.ToDecimal(r["PaidAmount"]):N2}");
                AddRow("Discount", $"Rs. {Convert.ToDecimal(r["Discount"]):N2}");
                AddRow("Remaining Amount", $"Rs. {Convert.ToDecimal(r["RemainingAmount"]):N2}");

                document.Add(receiptTable); // ✅ THIS WAS MISSING
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"PaymentReceipt_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fs);
                }

                response.status = "Success";
                response.message = "Payment Receipt Generated";
                response.data = new { FileUrl = $"/reports/{fileName}" };

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
        public IActionResult DownloadPaymentReceipt(PaymentReceiptRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                DataTable dtCompany = new DataTable();
                DataTable dtReceipt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // -------- COMPANY --------
                    using (SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dtCompany);
                        }
                    }

                    // -------- RECEIPTS (UNION ALL) --------
                    using (SqlCommand cmd = new SqlCommand("Report_PaymentReceipt", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PaymentId", request.PaymentId);
                        cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                        new SqlDataAdapter(cmd).Fill(dtReceipt);
                    }
                }

                if (dtReceipt.Rows.Count == 0)
                    throw new Exception("Receipt data not found");

                // -------- HEADER DATA --------
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyPhone = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();
                string userName = dtCompany.Rows[0]["UserName"].ToString();

                writer.PageEvent = new PdfPageEvents(userName);
                document.Open();

                // ================= LOOP: EACH RECEIPT =================
                foreach (DataRow r in dtReceipt.Rows)
                {
                    // -------- HEADER --------
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 20f, 80f });

                    Image logo = Image.GetInstance(
                        Path.Combine(_env.WebRootPath, "assets2", "img", logoFile));
                    logo.ScaleToFit(70f, 70f);

                    headerTable.AddCell(new PdfPCell(logo)
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        Padding = 10
                    });

                    PdfPCell detailCell = new PdfPCell
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        Padding = 10,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };

                    detailCell.AddElement(new Paragraph(companyName,
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                    detailCell.AddElement(new Paragraph(companyAddress,
                        FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                    detailCell.AddElement(new Paragraph(companyPhone,
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                    headerTable.AddCell(detailCell);
                    document.Add(headerTable);

                    // -------- TITLE --------
                    document.Add(new Paragraph("\n"));

                    PdfPTable titleTable = new PdfPTable(1);
                    titleTable.WidthPercentage = 100;

                    PdfPCell titleCell = new PdfPCell(
                        new Phrase("TRANSACTION RECEIPT",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.WHITE))
                    );
                    titleCell.BackgroundColor = new BaseColor(0, 102, 204);
                    titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    titleCell.Padding = 10;
                    titleCell.Border = Rectangle.NO_BORDER;

                    titleTable.AddCell(titleCell);
                    document.Add(titleTable);
                    document.Add(new Paragraph("\n"));

                    // -------- RECEIPT TABLE --------
                    PdfPTable receiptTable = new PdfPTable(2);
                    receiptTable.WidthPercentage = 100;
                    receiptTable.SetWidths(new float[] { 35f, 65f });

                    Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                    Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    BaseColor labelBg = new BaseColor(235, 238, 255);

                    void AddRow(string label, string value)
                    {
                        receiptTable.AddCell(new PdfPCell(new Phrase(label, labelFont))
                        {
                            BackgroundColor = labelBg,
                            Padding = 6
                        });

                        receiptTable.AddCell(new PdfPCell(new Phrase(value, valueFont))
                        {
                            Padding = 6
                        });
                    }

                    AddRow("Customer Name", r["CustomerName"].ToString());
                    AddRow("Payment Date", r["PaymentDate"].ToString());
                    AddRow("Account Name", r["AccountName"].ToString());
                    AddRow("Account Type", r["AccountType"].ToString());
                    AddRow("Total Amount", $"Rs. {Convert.ToDecimal(r["TotalAmount"]):N2}");
                    AddRow("Total Paid (Previously)", $"Rs. {Convert.ToDecimal(r["TotalPaidPreviously"]):N2}");
                    AddRow("Total Discount (Previously)", $"Rs. {Convert.ToDecimal(r["TotalDiscountPreviously"]):N2}");
                    AddRow("Paid Amount", $"Rs. {Convert.ToDecimal(r["PaidAmount"]):N2}");
                    AddRow("Discount", $"Rs. {Convert.ToDecimal(r["Discount"]):N2}");
                    AddRow("Remaining Amount", $"Rs. {Convert.ToDecimal(r["RemainingAmount"]):N2}");

                    document.Add(receiptTable);

                    document.NewPage(); // 🔥 NEXT RECEIPT
                }

                document.Close();

                // -------- SAVE FILE --------
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName;
                if (request.PaymentId > 0)
                {
                    fileName = $"PaymentReceipt_PId_{request.PaymentId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                }
                else {
                    fileName = $"PaymentReceipt_BId_{request.BookingId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                }
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fs);
                }

                response.status = "Success";
                response.message = "All Payment Receipts Generated";
                response.data = new { FileUrl = $"/reports/{fileName}" };
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
