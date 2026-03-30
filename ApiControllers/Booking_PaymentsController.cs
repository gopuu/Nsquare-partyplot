using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Presentation;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
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
                    cmd = new SqlCommand("Report_GetBooking_Payments_Receipt", con);
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
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.BOLD);
                iTextSharp.text.Font AmtFontBold13 = new iTextSharp.text.Font(bf, 13, iTextSharp.text.Font.BOLD);

                /* ================= HEADER METHOD ================= */
                PdfPTable BuildHeader()
                {
                    // Border settings -- START
                    PdfContentByte cb = writer.DirectContent;
                    float borderMargin = 10f; // distance from page edge
                    float lineWidth = 0.5f;

                    Rectangle rect = new Rectangle(
                        document.PageSize.Left + borderMargin,
                        document.PageSize.Bottom + borderMargin,
                        document.PageSize.Right - borderMargin,
                        document.PageSize.Top - borderMargin
                    );

                    rect.Border = Rectangle.BOX;
                    rect.BorderWidth = lineWidth;
                    rect.BorderColor = BaseColor.GRAY;
                    cb.Rectangle(rect);
                    // Border settings -- END

                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 20f, 80f });

                    Image logo = null;
                    string logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(logoFile))
                    {
                        string logoPath = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "CompanyInvoice_CompanyLogo_Image", logoFile);
                        if (System.IO.File.Exists(logoPath))
                            logo = Image.GetInstance(logoPath);
                        else {
                            logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);
                            if (System.IO.File.Exists(logoPath))
                                logo = Image.GetInstance(logoPath);
                        }
                    }

                    PdfPCell logoCell = logo != null
                        ? new PdfPCell(logo)
                        : new PdfPCell(new Phrase(""));

                    if (logo != null)
                        logo.ScaleToFit(92, 92);

                    logoCell.Border = Rectangle.BOTTOM_BORDER;
                    logoCell.Padding = 5;
                    logoCell.BorderColor = BaseColor.LIGHT_GRAY;
                    logoCell.BorderWidth = 0.5f;

                    headerTable.AddCell(logoCell);

                    PdfPCell detailCell = new PdfPCell
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        Padding = 5,
                        PaddingTop = 2,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        BorderWidth = 0.5f,
                        BorderColor = BaseColor.LIGHT_GRAY
                    };

                    detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["PropertyName"]?.ToString(),
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));

                    detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyName"]?.ToString(),
                        FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY)));

                    detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyAddress"]?.ToString(),
                        FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY)));

                    string contact = dtCompany.Rows[0]["CompanyMobile"]?.ToString();
                    string email = dtCompany.Rows[0]["CompanyEmail"]?.ToString();
                    string gstin = dtCompany.Rows[0]["CompanyGST"]?.ToString();

                    detailCell.AddElement(new Paragraph($"Contact: {contact} | Email: {email} | GSTIN: {gstin}",
                        FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY)));

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

                    document.Add(new Paragraph("\n"));
                    /* ================= TITLE ================= */
                    PdfPTable titleTable = new PdfPTable(1);
                    titleTable.WidthPercentage = 100;

                    titleTable.AddCell(new PdfPCell(
                        new Phrase("PAYMENT RECEIPT",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK)))
                    {
                        BackgroundColor = BaseColor.WHITE, //new BaseColor(0, 102, 204),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 6,
                        Border = Rectangle.BOX,
                        BorderColor = BaseColor.LIGHT_GRAY,
                        BorderWidth = 0.2f
                    });

                    document.Add(titleTable);

                    /* ================= TOP ================= */
                    PdfPTable top = new PdfPTable(2);
                    top.WidthPercentage = 100;
                    top.SetWidths(new float[] { 70, 30 });

                    top.AddCell(new PdfPCell(new Phrase($"RECEIPT NO: {receiptNo}", normalFont))
                    { Border = Rectangle.NO_BORDER });

                    top.AddCell(new PdfPCell(new Phrase($"DATE: {receiptDate}", normalFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT
                    });

                    document.Add(top);

                    /* ================= DETAILS ================= */
                    PdfPTable tbl = new PdfPTable(3);
                    tbl.WidthPercentage = 100;
                    tbl.SetWidths(new float[] { 19.5f, 3, 77 });

                    void AddRow(string label, string value)
                    {
                        tbl.AddCell(new PdfPCell(new Phrase(label, normalFont)) { Border = Rectangle.NO_BORDER });

                        tbl.AddCell(new PdfPCell(new Phrase(":", normalFont))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });

                        //tbl.AddCell(new PdfPCell(new Phrase(value ?? "-", boldFont))
                        //{
                        //    Border = Rectangle.BOTTOM_BORDER,
                        //    PaddingBottom = 5
                        //});
                        PdfPCell valueCell = new PdfPCell(new Phrase(value ?? "-", boldFont)) //normalFont
                        {
                            Border = Rectangle.NO_BORDER, // ❌ remove default solid border
                            PaddingBottom = 5
                        };

                        // ✅ Apply dotted underline
                        valueCell.CellEvent = new DottedBottomBorder();
                        tbl.AddCell(valueCell);
                    }

                    // 🔹 Title Row (full width)
                    PdfPCell titleCell = new PdfPCell(new Phrase("Customer Details", boldFont))
                    {
                        Colspan = 3,
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10,
                        PaddingBottom = 5
                    };
                    tbl.AddCell(titleCell);
                    AddRow("Name", customerName);
                    AddRow("Mobile:", customerMobile);

                    // 🔹 Title Row (full width)
                    titleCell = new PdfPCell(new Phrase("Event Details", boldFont))
                    {
                        Colspan = 3,
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10,
                        PaddingBottom = 5
                    };
                    tbl.AddCell(titleCell);
                    AddRow("Event Type", $"{eventName} ({subEventName})");
                    AddRow("Event Date ", $"{eventDate}");
                    AddRow("Hall Name", $"{hallName} at {startTime} ({shift})");

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
                    paySummary.WidthPercentage = 40;   // instead of 100
                    paySummary.HorizontalAlignment = Element.ALIGN_LEFT; // or CENTER
                    //paySummary.SetWidths(new float[] { 25, 3, 72 });
                    paySummary.SetWidths(new float[] { 50, 5, 45 });
                    // 🔹 Title Row (full width)
                    titleCell = new PdfPCell(new Phrase("Payment Details", boldFont))
                    {
                        Colspan = 3,
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10,
                        PaddingBottom = 5
                    };
                    paySummary.AddCell(titleCell);

                    // 🔹 Helper (same pattern as above table)
                    void AddPayRow2(string label, string value)
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
                    void AddPayRow(string label, string value)
                    {
                        paySummary.AddCell(new PdfPCell(new Phrase(label, normalFont))
                        {
                            Border = Rectangle.NO_BORDER,
                            Padding = 2
                        });

                        paySummary.AddCell(new PdfPCell(new Phrase(":", normalFont))
                        {
                            Border = Rectangle.NO_BORDER,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 2
                        });

                        //paySummary.AddCell(new PdfPCell(new Phrase(value, AmtFontNormal))
                        //{
                        //    Border = Rectangle.BOTTOM_BORDER,
                        //    PaddingBottom = 3,
                        //    PaddingTop = 2,
                        //    HorizontalAlignment = Element.ALIGN_RIGHT
                        //});

                        PdfPCell valueCell = new PdfPCell(new Phrase(value, AmtFontNormal)) //normalFont
                        {
                            Border = Rectangle.NO_BORDER, // ❌ remove default solid border
                            PaddingBottom = 3,
                            PaddingTop = 2,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };

                        // ✅ Apply dotted underline
                        valueCell.CellEvent = new DottedBottomBorder();
                        paySummary.AddCell(valueCell);

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

                    amtTbl.AddCell(new PdfPCell(new Phrase($"₹ {thisPayment:N0}", AmtFontBold13))
                    {
                        Border = Rectangle.BOX,
                        FixedHeight = 35,
                        BorderWidth = 0.5f,
                        BorderColor = BaseColor.DARK_GRAY,
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
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.BLACK))
                        );
                        titleCell2.BackgroundColor = BaseColor.WHITE;
                        titleCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                        titleCell2.Padding = 5;
                        titleCell2.Border = Rectangle.BOX;
                        titleCell2.BorderWidth = 0.2f;
                        titleCell2.BorderColor = BaseColor.LIGHT_GRAY;

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
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                Border = Rectangle.BOX,
                                BorderWidth = 0.2f,
                                BorderColor = BaseColor.LIGHT_GRAY
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
                            { 
                                HorizontalAlignment = Element.ALIGN_CENTER, 
                                Border = Rectangle.BOX,
                                BorderWidth = 0.2f,
                                BorderColor = BaseColor.LIGHT_GRAY
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentDateTimeFormat"]?.ToString() ?? "-", normalFont))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                Border = Rectangle.BOX,
                                BorderWidth = 0.2f,
                                BorderColor = BaseColor.LIGHT_GRAY
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentModeName"]?.ToString() ?? "-", normalFont))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                Border = Rectangle.BOX,
                                BorderWidth = 0.2f,
                                BorderColor = BaseColor.LIGHT_GRAY
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase($"₹ {amt:N0}", AmtFontNormal))
                            {
                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                Border = Rectangle.BOX,
                                BorderWidth = 0.2f,
                                BorderColor = BaseColor.LIGHT_GRAY
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase($"₹ {bal:N0}", AmtFontNormal))
                            {
                                HorizontalAlignment = Element.ALIGN_RIGHT,
                                Border = Rectangle.BOX,
                                BorderWidth = 0.2f,
                                BorderColor = BaseColor.LIGHT_GRAY
                            });

                            payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentNotes"]?.ToString() ?? "-", normalFont))
                            {
                                HorizontalAlignment = Element.ALIGN_LEFT,
                                Border = Rectangle.BOX,
                                BorderWidth = 0.2f,
                                BorderColor = BaseColor.LIGHT_GRAY
                            });
                        }

                        // 🔹 Total Amount TITLE
                        payTbl.AddCell(new PdfPCell(new Phrase($"TOTAL", AmtFontBold))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(230, 230, 230),
                            Padding = 5,
                            Colspan=3,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        // 🔹 Total Amount (Amount column)
                        payTbl.AddCell(new PdfPCell(new Phrase($"₹ {totalAmount:N0}", AmtFontBold))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(230, 230, 230),
                            Padding = 2,
                            PaddingTop = 5,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        // 🔹 Empty Balance column (optional)
                        payTbl.AddCell(new PdfPCell(new Phrase(""))
                        {
                            BackgroundColor = new BaseColor(230, 230, 230),
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        // 🔹 Empty Notes column (optional)
                        payTbl.AddCell(new PdfPCell(new Phrase(""))
                        {
                            BackgroundColor = new BaseColor(230, 230, 230),
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
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

class DottedBottomBorder : IPdfPCellEvent
{
    public void CellLayout(PdfPCell cell, Rectangle rect, PdfContentByte[] canvas)
    {
        PdfContentByte cb = canvas[PdfPTable.TEXTCANVAS]; // ✅ IMPORTANT

        cb.SaveState();

        cb.SetLineWidth(0.5f);
        cb.SetGrayStroke(0f);

        // 🔥 Force dotted
        cb.SetLineDash(0.5f, 2f);

        cb.MoveTo(rect.Left, rect.Bottom + 1); // slight offset
        cb.LineTo(rect.Right, rect.Bottom + 1);
        cb.Stroke();

        cb.RestoreState();
    }
}


class DottedBottomBorder2 : IPdfPCellEvent
{
    public void CellLayout(PdfPCell cell, Rectangle rect, PdfContentByte[] canvas)
    {
        PdfContentByte cb = canvas[PdfPTable.LINECANVAS];

        cb.SaveState();

        // 🔹 Set dotted line style
        //cb.SetLineDash(2f, 2f); // (dash length, gap length)
        cb.SetLineDash(1.5f, 1.5f);

        cb.MoveTo(rect.Left, rect.Bottom);
        cb.LineTo(rect.Right, rect.Bottom);
        cb.Stroke();

        cb.RestoreState();
    }
}