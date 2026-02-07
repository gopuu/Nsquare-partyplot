using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2016.Excel;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;
using Newtonsoft.Json;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;

        private string connString = string.Empty;

        public ReportController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        #region Useful Methods
        private PdfPCell Cell(string text, Font font, int align = Element.ALIGN_LEFT, int colspan = 1, float height = 25)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? " ", font));
            cell.Colspan = colspan;
            cell.FixedHeight = height;
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 6;
            cell.Border = Rectangle.BOX;
            return cell;
        }
        PdfPCell CreateTitleWithSpaceCell(string title, Font titleFont, float height)
        {
            PdfPTable inner = new PdfPTable(1);
            inner.WidthPercentage = 100;

            // 🔸 Title (TOP)
            PdfPCell titleCell = new PdfPCell(new Phrase(title, titleFont))
            {
                Border = Rectangle.BOTTOM_BORDER,
                Padding = 4,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            inner.AddCell(titleCell);

            // 🔸 Empty Space for VALUE
            PdfPCell valueCell = new PdfPCell(new Phrase(" "))
            {
                MinimumHeight = height,
                Border = Rectangle.NO_BORDER
            };
            inner.AddCell(valueCell);

            return new PdfPCell(inner)
            {
                Padding = 0
            };
        }
        PdfPCell CreateTitleWithValueCell(string title, string value, Font titleFont, Font valueFont,float minHeight)
        {
            PdfPTable inner = new PdfPTable(1);
            inner.WidthPercentage = 100;

            // 🔹 Title
            PdfPCell titleCell = new PdfPCell(new Phrase(title, titleFont))
            {
                Border = Rectangle.BOTTOM_BORDER,
                Padding = 4,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            inner.AddCell(titleCell);

            // 🔹 Value (directly below title)
            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont))
            {
                MinimumHeight = minHeight,
                Padding = 6,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Border = Rectangle.NO_BORDER
            };
            inner.AddCell(valueCell);

            return new PdfPCell(inner)
            {
                Padding = 0
            };
        }

        private PdfPCell CreateCell(string text, Font font, int align = Element.ALIGN_LEFT, int colspan = 1, BaseColor bg = null)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? " ", font));
            cell.Colspan = colspan;
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 4;
            cell.Border = Rectangle.BOX;

            if (bg != null)
                cell.BackgroundColor = bg;

            return cell;
        }
        public static PdfPCell CreateCell2(string text, Font font, int hAlign = Element.ALIGN_LEFT, int colspan = 1, float fixedHeight = 0, int border = Rectangle.BOX)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = hAlign;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Colspan = colspan;
            cell.Padding = 5;
            cell.Border = border;

            if (fixedHeight > 0)
                cell.FixedHeight = fixedHeight;

            return cell;
        }
        private PdfPCell CreateHeaderCell(string text, Font font, BaseColor bgColor)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = bgColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 6,
                BorderColor = new BaseColor(220, 220, 220), // light gray
                BorderWidth = 0.5f
            };
        }
        private PdfPCell CreateBodyCell(string text, Font font, int align = Element.ALIGN_LEFT, float padding = 4f)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = align,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = padding,
                BorderColor = new BaseColor(220, 220, 220), // light gray
                BorderWidth = 0.5f
            };
        }
        private PdfPCell CreateInfoCell(string label, object? value, bool bold = false)
        {
            Font font = FontFactory.GetFont(FontFactory.HELVETICA, bold ? 10 : 9, bold ? Font.BOLD : Font.NORMAL);
            return new PdfPCell(new Phrase($"{label} : {value}", font))
            {
                Border = Rectangle.NO_BORDER,
                Padding = 4
            };
        }
        private PdfPCell CreateTotalLabelCell(string text, bool isBold = false)
        {
            return new PdfPCell(new Phrase(
                text,
                FontFactory.GetFont(FontFactory.HELVETICA, 9, isBold ? Font.BOLD : Font.NORMAL)
            ))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 4
            };
        }
        private PdfPCell CreateTotalValueCell(object value, bool isBold = false)
        {
            return new PdfPCell(new Phrase(
                Convert.ToDecimal(value).ToString("N2"),
                FontFactory.GetFont(FontFactory.HELVETICA, 9, isBold ? Font.BOLD : Font.NORMAL)
            ))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 4
            };
        }

        #endregion

        [HttpPost]
        [Route("DownloadCustomerInvoiceReport")]
        public IActionResult DownloadCustomerInvoiceReport(CustomerInvoiceReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
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

                DataTable dtCompany = new DataTable();
                DataTable dtInvoice = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company Header
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Invoice Report
                    using SqlCommand cmd = new SqlCommand("Report_CustomerInvoiceById", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                    cmd.Parameters.AddWithValue("@InvoiceNo", request.InvoiceNo);
                    cmd.Parameters.AddWithValue("@ProformaInvoiceNo", request.ProformaInvoiceNo);

                    new SqlDataAdapter(cmd).Fill(dtInvoice);
                }

                if (dtCompany.Rows.Count == 0 || dtInvoice.Rows.Count == 0)
                    throw new Exception("Invoice data not found");

                DataRow inv = dtInvoice.Rows[0];

                string companyName = dtCompany.Rows[0]["CompanyName"].ToString() ?? "";
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString() ?? "";
                string companyPhone = dtCompany.Rows[0]["CompanyMobile"].ToString() ?? "";
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString() ?? "";
                string userName = dtCompany.Rows[0]["UserName"].ToString() ?? "";

                writer.PageEvent = new PdfPageEvents(userName);
                document.Open();

                // ================= HEADER =================
                if (request.WithHeader == true)
                {
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

                    PdfPCell infoCell = new PdfPCell
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        Padding = 10,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };

                    infoCell.AddElement(new Paragraph(companyName, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                    infoCell.AddElement(new Paragraph(companyAddress, FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                    infoCell.AddElement(new Paragraph(companyPhone, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                    headerTable.AddCell(infoCell);
                    document.Add(headerTable);
                }

                // ================= TITLE =================
                string title = "TAX INVOICE";
                if (request.ProformaInvoiceNo > 0)
                {
                    title = "PROFORMA INVOICE";
                }

                document.Add(new Paragraph(title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
                { Alignment = Element.ALIGN_CENTER, SpacingAfter = 12f });

                // ================= CUSTOMER INFO =================
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 50f, 50f });

                string? invoiceNoToShow = request.ProformaInvoiceNo > 0 //Convert.ToInt64(inv["ProformaInvoiceNo"]) > 0
                        ? inv["ProformaInvoiceNo"].ToString()
                        : inv["InvoiceNo"]?.ToString();

                infoTable.AddCell(CreateInfoCell("Invoice No", invoiceNoToShow));
                infoTable.AddCell(CreateInfoCell("Invoice Date", inv["InvoiceDate_Format"]));
                infoTable.AddCell(CreateInfoCell("Customer Name", inv["CustomerName"]));
                infoTable.AddCell(CreateInfoCell("GSTIN", inv["GSTIN"]));
                infoTable.AddCell(CreateInfoCell("Email", inv["EmailId"]));
                infoTable.AddCell(CreateInfoCell("Address", inv["Address"]));

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                // ================= ITEMS TABLE =================
                string jsonItems = inv["JSON_Items"]?.ToString() ?? "[]";
                var items = JsonConvert.DeserializeObject<DataTable>(jsonItems);

                PdfPTable itemTable = new PdfPTable(8);
                itemTable.WidthPercentage = 100;
                itemTable.SetWidths(new float[] { 5, 25, 10, 10, 10, 10, 10, 10 });

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "#", "Description", "HSN", "Rate", "Qty", "Taxable", "GST", "Total" };

                foreach (var h in headers)
                    itemTable.AddCell(CreateHeaderCell(h, thFont, thColor));

                itemTable.HeaderRows = 1;

                if (items == null || items.Rows.Count == 0)
                {
                    itemTable.AddCell(new PdfPCell(new Phrase("No Items"))
                    {
                        Colspan = 8,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 10
                    });
                }
                else
                {
                    foreach (DataRow r in items.Rows)
                    {
                        itemTable.AddCell(CreateBodyCell(r["SrNo"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(CreateBodyCell(r["Description"].ToString(), tdFont));
                        itemTable.AddCell(CreateBodyCell(r["HSN"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(CreateBodyCell(r["Rate"].ToString(), tdFont, Element.ALIGN_RIGHT));
                        itemTable.AddCell(CreateBodyCell(r["Qty"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(CreateBodyCell(r["TaxableAmount"].ToString(), tdFont, Element.ALIGN_RIGHT));
                        itemTable.AddCell(CreateBodyCell(r["GSTRate"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(CreateBodyCell(r["Total"].ToString(), tdFont, Element.ALIGN_RIGHT));
                    }
                }

                document.Add(itemTable);

                // ================= TOTALS =================
                document.Add(new Paragraph("\n"));
                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 40;
                totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalTable.SetWidths(new float[] { 60f, 40f });

                totalTable.AddCell(CreateTotalLabelCell("Sub Total"));
                totalTable.AddCell(CreateTotalValueCell(inv["SubTotal"]));

                totalTable.AddCell(CreateTotalLabelCell("Booking Discount"));
                totalTable.AddCell(CreateTotalValueCell(inv["BookingDiscount"]));

                totalTable.AddCell(CreateTotalLabelCell("Total Taxable"));
                totalTable.AddCell(CreateTotalValueCell(inv["TotalTaxableAmount"]));

                totalTable.AddCell(CreateTotalLabelCell("Total GST"));
                totalTable.AddCell(CreateTotalValueCell(inv["TotalGST"]));

                totalTable.AddCell(CreateTotalLabelCell("Round Off"));
                totalTable.AddCell(CreateTotalValueCell(inv["RoundOff"]));

                totalTable.AddCell(CreateTotalLabelCell("Advance Paid"));
                totalTable.AddCell(CreateTotalValueCell(inv["TotalAdvancePaid"]));

                totalTable.AddCell(CreateTotalLabelCell("Grand Total", true));
                totalTable.AddCell(CreateTotalValueCell(inv["GrandTotal"], true));

                document.Add(totalTable);

                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);
                
                string fileName = request.ProformaInvoiceNo > 0 ?
                    $"ProformaInvoice_{invoiceNoToShow}_{DateTime.Now:yyyyMMddHHmmss}.pdf" :
                    $"Invoice_{invoiceNoToShow}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Invoice_*.pdf", 60);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fs);

                response.status = "Success";
                response.message = "Invoice Report Generated";
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
        [Route("DownloadBookingEventReport")]
        public IActionResult DownloadBookingEventReport(DownloadBookingEventReportRequest request)
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

                using MemoryStream stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 25, 25, 30, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // ================= COMPANY HEADER =================
                    using (SqlCommand cmdCompany = new SqlCommand("Report_PageHeader_GetCompany", con))
                    {
                        cmdCompany.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter daCompany = new SqlDataAdapter(cmdCompany);
                        daCompany.Fill(dtCompany);
                    }

                    // ================= BOOKING EVENT DETAILS =================
                    using (SqlCommand cmdReport = new SqlCommand("Report_GetBookingEventDetails", con))
                    {
                        cmdReport.CommandType = CommandType.StoredProcedure;

                        // 🔹 BookingId Parameter
                        cmdReport.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;

                        SqlDataAdapter daReport = new SqlDataAdapter(cmdReport);
                        daReport.Fill(dsReport);
                    }
                }


                /* ================= FONT ================= */
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                BaseColor gray = new BaseColor(230, 230, 230);

                /* ================= DATASET MAPPING ================= */
                DataTable dtFormTitle = dsReport.Tables[0];
                DataTable dtBasic = dsReport.Tables[1];
                DataTable dtHeaders = dsReport.Tables[2];
                DataTable dtPlateRows = dsReport.Tables[3];
                DataTable dtSummary = dsReport.Tables[4];
                DataTable dtPayment = dsReport.Tables[5];
                DataTable dtFooter = dsReport.Tables[6];
                DataTable dtStaffTitle = dsReport.Tables[7];
                DataTable dtFoodTitles = dsReport.Tables[8];
                DataTable dtStaffDuty = dsReport.Tables[9];
                DataTable dtChkTitle = dsReport.Tables[10];
                DataTable dtChkItems = dsReport.Tables[11];
                DataTable dtNotes = dsReport.Tables[12];


                /* ================= HEADER ================= */
                PdfPTable header = new PdfPTable(2);
                header.WidthPercentage = 100;
                header.SetWidths(new float[] { 20, 80 });

                string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img",
                    dtCompany.Rows[0]["CompanyLogo"].ToString());

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(60, 60);

                header.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER });

                PdfPCell compCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                compCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyName"].ToString(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));
                compCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyAddress"].ToString(),
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));

                header.AddCell(compCell);
                document.Add(header);

                document.Add(new Paragraph("\n"));

                /* ================= EVENT FORM TITLE ================= */
                PdfPTable titleTable = new PdfPTable(2);
                titleTable.WidthPercentage = 100;
                titleTable.SetWidths(new float[] { 70, 30 });

                PdfPCell titleCell = CreateCell(
                    dtFormTitle.Rows[0]["FormTitle"].ToString(),
                    titleFont,
                    Element.ALIGN_CENTER);
                titleCell.BackgroundColor = new BaseColor(230, 230, 230); // light gray
                titleTable.AddCell(titleCell);

                PdfPCell dateCell = CreateCell(dtFormTitle.Rows[0]["DateTitle"].ToString(),titleFont);
                dateCell.BackgroundColor = new BaseColor(230, 230, 230);
                titleTable.AddCell(dateCell);

                document.Add(titleTable);


                /* ================= GUEST DETAILS ================= */
                PdfPTable guestTable = new PdfPTable(2);
                guestTable.WidthPercentage = 100;
                guestTable.SetWidths(new float[] { 30, 70 });

                guestTable.AddCell(CreateCell(dtBasic.Rows[0]["GuestNameTitle"].ToString(), boldFont));
                guestTable.AddCell(CreateCell(dtBasic.Rows[0]["GuestName"].ToString(), normalFont));

                guestTable.AddCell(CreateCell(dtBasic.Rows[0]["EventDetailsTitle"].ToString(), boldFont));
                guestTable.AddCell(CreateCell(dtBasic.Rows[0]["EventDetails"].ToString(), normalFont));

                document.Add(guestTable);

                /* ================= PLATE TABLE ================= */
                PdfPTable plateTable = new PdfPTable(4);
                plateTable.WidthPercentage = 100;
                plateTable.SetWidths(new float[] { 30, 20, 25, 25 });

                plateTable.AddCell(CreateCell("", boldFont));
                plateTable.AddCell(CreateCell(dtHeaders.Rows[0]["Col_NoOfPlates"].ToString(), boldFont, Element.ALIGN_CENTER));
                plateTable.AddCell(CreateCell(dtHeaders.Rows[0]["Col_AuthorisedSignature"].ToString(), boldFont, Element.ALIGN_CENTER));
                plateTable.AddCell(CreateCell(dtHeaders.Rows[0]["Col_GuestSignature"].ToString(), boldFont, Element.ALIGN_CENTER));

                foreach (DataRow r in dtPlateRows.Rows)
                {
                    plateTable.AddCell(CreateCell(r["RowTitle"].ToString(), normalFont));
                    plateTable.AddCell(CreateCell(r["NoOfPlates"].ToString(), normalFont));
                    plateTable.AddCell(CreateCell(r["AuthSign"].ToString(), normalFont));
                    plateTable.AddCell(CreateCell(r["GuestSignature"].ToString(), normalFont));
                }

                document.Add(plateTable);

                /* ================= SUMMARY ================= */
                PdfPTable summaryTable = new PdfPTable(4);
                summaryTable.WidthPercentage = 100;

                summaryTable.AddCell(CreateCell(
                    dtSummary.Rows[0]["SummaryTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    4,
                    gray));

                document.Add(summaryTable);

                /* ================= PAYMENT ================= */
                PdfPTable paymentTable = new PdfPTable(3);
                paymentTable.WidthPercentage = 100;
                paymentTable.SetWidths(new float[] { 35, 30, 35 });

                /* ---------- Row 1 ---------- */
                paymentTable.AddCell(CreateCell(dtPayment.Rows[0]["RatePerPlateTitle"].ToString(), boldFont));
                paymentTable.AddCell(CreateCell(dtPayment.Rows[0]["RatePerPlate"].ToString(), normalFont));

                paymentTable.AddCell(CreateTitleWithValueCell(
                    dtPayment.Rows[0]["ExtraAmountTitle"].ToString(),
                    dtPayment.Rows[0]["ExtraAmount"].ToString(),   // 👈 VALUE HERE
                    boldFont,
                    normalFont,
                    55f
                ));
                /* ---------- Row 2 ---------- */
                paymentTable.AddCell(CreateCell(dtPayment.Rows[0]["TotalPayableTitle"].ToString(), boldFont));
                PdfPCell totalPayableCell = CreateCell(dtPayment.Rows[0]["TotalPayable"].ToString(), normalFont);
                totalPayableCell.Colspan = 2;
                paymentTable.AddCell(totalPayableCell);

                /* ---------- Row 3 ---------- */
                paymentTable.AddCell(CreateCell(dtPayment.Rows[0]["AdvanceAmountTitle"].ToString(), boldFont));
                paymentTable.AddCell(CreateCell(dtPayment.Rows[0]["AdvanceAmount"].ToString(), normalFont));

                paymentTable.AddCell(CreateTitleWithValueCell(
                    dtPayment.Rows[0]["AdvancePaymentModeTitle"].ToString(),
                    dtPayment.Rows[0]["AdvancePaymentMode"].ToString(),   // 👈 VALUE HERE
                    boldFont,
                    normalFont,
                    55f
                ));

                /* ---------- Row 4 ---------- */
                paymentTable.AddCell(CreateCell(dtPayment.Rows[0]["FinalRemainingPaymentTitle"].ToString(), boldFont));
                paymentTable.AddCell(CreateCell(dtPayment.Rows[0]["FinalRemainingPayment"].ToString(), normalFont));

                paymentTable.AddCell(CreateTitleWithValueCell(
                    dtPayment.Rows[0]["FinalPaymentModeTitle"].ToString(),
                    dtPayment.Rows[0]["FinalPaymentMode"].ToString(),   // 👈 VALUE HERE
                    boldFont,
                    normalFont,
                    55f
                ));

                document.Add(paymentTable);

                /* ================= FOOTER ================= */
                PdfPTable footer = new PdfPTable(1);
                footer.WidthPercentage = 100;

                footer.AddCell(CreateCell(
                    dtFooter.Rows[0]["FeedbackTitle"].ToString(),
                    titleFont,
                    Element.ALIGN_CENTER,
                    1,
                    gray));

                document.Add(footer);

                PdfPTable mainTable = new PdfPTable(2);
                mainTable.WidthPercentage = 100;
                mainTable.SetWidths(new float[] { 65, 35 });

                PdfPTable leftTable = new PdfPTable(1);
                leftTable.WidthPercentage = 100;

                // Feedback image (emoji + stars combined PNG)
                string imgPath = Path.Combine(
                                    _env.WebRootPath,
                                    "assets2",
                                    "img",
                                    "feedback-stars.png"
                                );

                Image feedbackImg = Image.GetInstance(imgPath);
                feedbackImg.ScaleToFit(300f, 200f);
                feedbackImg.Alignment = Image.ALIGN_LEFT;

                PdfPCell imgCell = new PdfPCell(feedbackImg)
                {
                    Border = Rectangle.BOX,
                    Padding = 8,
                    FixedHeight = 100
                };

                leftTable.AddCell(imgCell);


                PdfPTable rightTable = new PdfPTable(1);
                rightTable.WidthPercentage = 100;

                /* COMMENTS */
                PdfPCell commentTitle = new PdfPCell(new Phrase("Comments", boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 4
                };

                PdfPCell commentBox = new PdfPCell(new Phrase(" "))
                {
                    FixedHeight = 100
                };

                rightTable.AddCell(commentTitle);
                rightTable.AddCell(commentBox);

                /* SIGNATURE */
                PdfPCell signTitle = new PdfPCell(new Phrase("Guest Signature", boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 4
                };

                PdfPCell signBox = new PdfPCell(new Phrase(" "))
                {
                    FixedHeight = 100
                };

                rightTable.AddCell(signTitle);
                rightTable.AddCell(signBox);

                mainTable.AddCell(new PdfPCell(leftTable));
                mainTable.AddCell(new PdfPCell(rightTable));

                document.Add(mainTable);

                document.NewPage();

                PdfPTable titleTbl = new PdfPTable(1);
                titleTbl.WidthPercentage = 100;
                titleTbl.AddCell(CreateCell2(dtStaffTitle.Rows[0]["StaffUseOnlyTitle"].ToString(), titleFont, Element.ALIGN_CENTER, 1, 30));
                document.Add(titleTbl);
                document.Add(new Paragraph("\n")); // Space

                // ------------------- FOOD WASTAGE TABLE -------------------
                PdfPTable fwTable = new PdfPTable(2); // Left + Right
                fwTable.WidthPercentage = 100;
                fwTable.SetWidths(new float[] { 30f, 70f });

                // Left box: FoodWastageTitle
                PdfPTable leftBox = new PdfPTable(1);
                leftBox.WidthPercentage = 100;

                PdfPCell leftTitleCell = new PdfPCell(
                    new Phrase(dtFoodTitles.Rows[0]["FoodWastageTitle"].ToString(), boldFont)
                );
                leftTitleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                leftTitleCell.VerticalAlignment = Element.ALIGN_TOP;
                leftTitleCell.FixedHeight = 60f;
                leftTitleCell.Border = Rectangle.NO_BORDER;   // ✅ IMPORTANT

                leftBox.AddCell(leftTitleCell);


                // Right box: 3 rows (LocationTime, WeightDoneBy, WeightCrossCheck)
                PdfPTable rightBox = new PdfPTable(2);
                rightBox.WidthPercentage = 100;
                rightBox.SetWidths(new float[] { 50f, 50f });

                // Row 1
                rightBox.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["LocationTimeTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));

                // Row 2
                rightBox.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["WeightDoneByTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));

                // Row 3
                rightBox.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["WeightCrossCheckByTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));


                // Add left and right boxes to main table
                fwTable.AddCell(new PdfPCell(leftBox) { Border = Rectangle.BOX, Padding = 5 });
                fwTable.AddCell(new PdfPCell(rightBox) { Border = Rectangle.BOX, Padding = 5 });

                document.Add(fwTable);
                document.Add(new Paragraph("\n"));

                // ================= MAIN CONTAINER TABLE =================
                PdfPTable mainTable2 = new PdfPTable(2);
                mainTable2.WidthPercentage = 100;
                mainTable2.SetWidths(new float[] { 60f, 40f }); // Left (Item) | Right (Staff)

                // =======================================================
                // LEFT SIDE : ITEM & WEIGHT TABLE
                // =======================================================
                PdfPTable itemTable = new PdfPTable(2);
                itemTable.WidthPercentage = 100;
                itemTable.SetWidths(new float[] { 60f, 40f });

                // Header
                itemTable.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["ItemNameTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 1, 25));

                itemTable.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["WeightKgTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 1, 25));

                // 7 blank rows
                for (int i = 0; i < 7; i++)
                {
                    itemTable.AddCell(CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 25));
                    itemTable.AddCell(CreateCell2("", normalFont, Element.ALIGN_CENTER, 1, 25));
                }

                // Add LEFT table to main table
                PdfPCell leftCell = new PdfPCell(itemTable);
                leftCell.Border = Rectangle.NO_BORDER;
                leftCell.Padding = 3;
                mainTable2.AddCell(leftCell);

               
                // RIGHT SIDE : STAFF TABLE
                // =======================================================
                PdfPTable staffTbl = new PdfPTable(2);
                staffTbl.WidthPercentage = 100;
                staffTbl.SetWidths(new float[] { 45f, 45f });

                // Staff Header
                staffTbl.AddCell(CreateCell2(
                    dtStaffDuty.Rows[0]["StaffDutyTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 2, 25));

                // --- PRESENT STAFF 
                staffTbl.AddCell(CreateCell2(
                    dtStaffDuty.Rows[0]["StaffPresentTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 50)); // Label height: 50

                staffTbl.AddCell(CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 80)); 

                // --- ABSENT STAFF 
                staffTbl.AddCell(CreateCell2(
                    dtStaffDuty.Rows[0]["StaffAbsentTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 30)); // Label height: 30

                staffTbl.AddCell(CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 30)); 

                // Add RIGHT table to main table
                PdfPCell rightCell = new PdfPCell(staffTbl);
                rightCell.Border = Rectangle.NO_BORDER;
                rightCell.Padding = 3;
                mainTable2.AddCell(rightCell);

                // =======================================================
                // ADD TO DOCUMENT
                // =======================================================
                document.Add(mainTable2);
                document.Add(new Paragraph("\n"));

                PdfPTable chkTbl = new PdfPTable(2);
                chkTbl.WidthPercentage = 100;

                chkTbl.AddCell(Cell(
                    dtChkTitle.Rows[0]["ChecklistTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    2,
                    30
                ));

                foreach (DataRow r in dtChkItems.Rows)
                {
                    chkTbl.AddCell(Cell(r["ChecklistItem"].ToString(), normalFont));
                    chkTbl.AddCell(Cell("", normalFont));
                }

                document.Add(chkTbl);

                PdfPTable notesTbl = new PdfPTable(1);
                notesTbl.WidthPercentage = 100;

                notesTbl.AddCell(Cell(
                    dtNotes.Rows[0]["EventNotesTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    1,
                    30
                ));

                notesTbl.AddCell(Cell("", normalFont, Element.ALIGN_LEFT, 1, 120));
                document.Add(notesTbl);

                document.Close();

                /* ================= SAVE ================= */
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"BookingEvent_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "PDF Generated Successfully";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
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
        [Route("DownloadBookingReport")]
        public IActionResult DownloadBookingReport(BookingReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                {
                    return errorResult;
                }

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetBookingDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@MonthDuration", request.FilterDuration);
               
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString() ?? string.Empty;
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString() ?? string.Empty;
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString() ?? string.Empty;
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString() ?? string.Empty;
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString() ?? string.Empty;
                string userName = dtCompany.Rows[0]["UserName"].ToString() ?? string.Empty;

                writer.PageEvent = new PdfPageEvents(userName);
                document.Open();

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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

                detailCell.AddElement(new Paragraph(companyName, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress, FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK);

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 12f
                };

                document.Add(reportTitle);

                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                // ---- Header ----
                //table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));
                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                {
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString() ?? string.Empty, thFont, thColor));
                }

                table.HeaderRows = 1;


                // ---- NO DATA FOUND CONDITION ----
                if (dataTableDB.Rows.Count == 0)
                {
                    Font noDataFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.RED);

                    PdfPCell noDataCell = new PdfPCell(
                        new Phrase("No Data Found", noDataFont)
                    )
                    {
                        Colspan = columnCount,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 12f,
                        BackgroundColor = BaseColor.WHITE
                    };

                    table.AddCell(noDataCell);
                }
                else
                {
                    // ---- DATA ROWS ----
                    foreach (DataRow row in dataTableDB.Rows)
                    {
                        for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        {
                            int alignment = Element.ALIGN_LEFT; // default

                            // Column index based alignment
                            if (i == 0 || i == 5)          // 1st & 6th column
                                alignment = Element.ALIGN_CENTER;
                            else if (i == 7)               // 8th column
                                alignment = Element.ALIGN_RIGHT;

                            if (i == 0)          // 1st column
                                table.AddCell(CreateBodyCell(row[i]?.ToString() ?? "-", tdFont, alignment, 2f));
                            else
                                table.AddCell(CreateBodyCell(row[i]?.ToString() ?? "-", tdFont, alignment));
                        }
                    }
                }
                document.Add(table);

                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Booking_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Booking_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

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
        [Route("DownloadBookingReport_Excel")]
        public IActionResult DownloadBookingReport_Excel([FromBody] BookingReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult errorResult))
                    return errorResult;

                // === Fetch Data from DB ===
                DataSet dsReport = new DataSet();
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("Report_GetBookingDetails", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                        cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                        cmd.Parameters.AddWithValue("@HallId", request.HallId);
                        cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                        cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                        cmd.Parameters.AddWithValue("@MonthDuration", request.FilterDuration);

                        new SqlDataAdapter(cmd).Fill(dsReport);
                    }
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // === Create Excel ===
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("BookingReport");

                // === Headers ===
                int colCount = headerTableDB.Columns.Count;
                worksheet.Cell(1, 1).Value = "Sr. No";
                worksheet.Cell(1, 1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int i = 0; i < colCount; i++)
                {
                    worksheet.Cell(1, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    worksheet.Cell(1, i + 2).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 2).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, i + 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // === Data Rows ===
                int row = 2;
                int srNo = 1;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    worksheet.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < colCount; i++)
                    {
                        worksheet.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                // === Save File ===
                string reportsDir = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "reports");
                if (!Directory.Exists(reportsDir))
                    Directory.CreateDirectory(reportsDir);

                Utils.deleteOldFiles(reportsDir, "BookingReport_*.xlsx");

                string fileName = $"BookingReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(reportsDir, fileName);

                workbook.SaveAs(filePath);

                string fileUrl = $"{domainUrl}/uploads/org/{domain}/reports/{fileName}";

                response.status = "Success";
                response.message = "Booking Report Excel generated successfully.";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = "Error generating Excel: " + ex.Message;
                response.data = null;
                return Ok(response);
            }
        }

        [HttpPost]
        [Route("Report_DailySalesDetails")]
        public IActionResult Report_DailySalesDetails(DailySalesDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetDailySales", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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

                //headerTable.AddCell(detailCell);
                //document.Add(headerTable);

                headerTable.AddCell(detailCell);
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"DailySales_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "DailySales_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

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
        [Route("Report_DailySalesDetails_Excel")]
        public IActionResult Report_DailySalesDetails_Excel(DailySalesDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    using SqlCommand cmd = new SqlCommand("Report_GetDailySales", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Daily Sales");

                int colCount = headerTableDB.Columns.Count;

                // ===== Report Title =====
                ws.Cell(1, 1).Value = reportName;
                ws.Range(1, 1, 1, colCount + 1).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ===== Header =====
                ws.Cell(3, 1).Value = "Sr. No";
                ws.Cell(3, 1).Style.Font.Bold = true;
                ws.Cell(3, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                for (int i = 0; i < colCount; i++)
                {
                    ws.Cell(3, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    ws.Cell(3, i + 2).Style.Font.Bold = true;
                    ws.Cell(3, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                }

                // ===== Data =====
                int row = 4;
                int srNo = 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < colCount; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ===== Save =====
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "DailySales_Report_*.xlsx");

                string fileName = $"DailySales_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Daily Sales Excel Generated";
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
        [Route("Report_MonthlySalesDetails")]
        public IActionResult Report_MonthlySalesDetails(MonthlySalesDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetMonthlySales", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }

                //document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"MonthlySales_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "MonthlySales_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_MonthlySalesDetails_Excel")]
        public IActionResult Report_MonthlySalesDetails_Excel(MonthlySalesDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using SqlCommand cmd = new SqlCommand("Report_GetMonthlySales", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Monthly Sales");

                int colCount = headerTableDB.Columns.Count;

                // ===== Report Title =====
                ws.Cell(1, 1).Value = reportName;
                ws.Range(1, 1, 1, colCount + 1).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ===== Header =====
                ws.Cell(3, 1).Value = "Sr. No";
                ws.Cell(3, 1).Style.Font.Bold = true;
                ws.Cell(3, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                for (int i = 0; i < colCount; i++)
                {
                    ws.Cell(3, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    ws.Cell(3, i + 2).Style.Font.Bold = true;
                    ws.Cell(3, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                }

                // ===== Data =====
                int row = 4;
                int srNo = 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < colCount; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ===== Save =====
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "MonthlySales_Report_*.xlsx");

                string fileName = $"MonthlySales_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Monthly Sales Excel Generated";
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
        [Route("Report_RevenueLoss")]
        public IActionResult Report_RevenueLoss(RevenueLossRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetRevenueLoss", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"RevenueLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "RevenueLoss_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_RevenueLoss_Excel")]
        public IActionResult Report_RevenueLoss_Excel(RevenueLossRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using SqlCommand cmd = new SqlCommand("Report_GetRevenueLoss", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Revenue Loss");

                int colCount = headerTableDB.Columns.Count;

                // ================= REPORT TITLE =================
                ws.Cell(1, 1).Value = reportName.ToUpper();
                ws.Range(1, 1, 1, colCount + 1).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ================= HEADER =================
                ws.Cell(3, 1).Value = "Sr. No";
                ws.Cell(3, 1).Style.Font.Bold = true;
                ws.Cell(3, 1).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightBlue;

                for (int i = 0; i < colCount; i++)
                {
                    ws.Cell(3, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    ws.Cell(3, i + 2).Style.Font.Bold = true;
                    ws.Cell(3, i + 2).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.LightBlue;
                }

                // ================= DATA =================
                int row = 4;
                int srNo = 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < colCount; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "RevenueLoss_Report_*.xlsx");

                string fileName =
                    $"RevenueLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Revenue Loss Excel Generated";
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
        [Route("Report_GSTDetails")]
        public IActionResult Report_GSTDetails(GSTDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetGSTDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                //document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"GST_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "GST_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_GSTDetails_Excel")]
        public IActionResult Report_GSTDetails_Excel(GSTDetailsRequest request)
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

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    using SqlCommand cmd = new SqlCommand("Report_GetGSTDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");
           

                using var wb = new ClosedXML.Excel.XLWorkbook();
                var ws = wb.Worksheets.Add("GST Report");
                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                int totalColumns = dataTableDB.Columns.Count + 1;

                // ===== Report Title =====
                ws.Cell(1, 1).Value = reportName.ToUpper();
                ws.Range(1, 1, 1, totalColumns).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;


                int headerRow = 2;
                int colIndex = 1;

                // 🔹 SR NO
                ws.Cell(headerRow, colIndex).Value = "Sr. No";
                ws.Cell(headerRow, colIndex).Style.Font.Bold = true;
                colIndex++;

                // 🔹 HEADERS
                foreach (DataColumn col in headerTableDB.Columns)
                {
                    ws.Cell(headerRow, colIndex).Value =
                        headerTableDB.Rows[0][col.ColumnName].ToString();
                    ws.Cell(headerRow, colIndex).Style.Font.Bold = true;
                    colIndex++;
                }

                // 🔹 DATA
                int rowIndex = headerRow + 1;
                int srNo = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    ws.Cell(rowIndex, 1).Value = srNo++;

                    colIndex = 2;
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        ws.Cell(rowIndex, colIndex).Value = row[col]?.ToString() ?? "-";
                        colIndex++;
                    }

                    rowIndex++;
                }

                ws.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"GST_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    wb.SaveAs(stream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Excel Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_Purchase")]
        public IActionResult Report_Purchase(PurchaseDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetPurchaseDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Purchase_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_Purchase_Excel")]
        public IActionResult Report_Purchase_Excel(PurchaseDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using SqlCommand cmd = new SqlCommand("Report_GetPurchaseDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Purchase Report");

                int colCount = headerTableDB.Columns.Count;

                // ================= TITLE =================
                ws.Cell(1, 1).Value = reportName.ToUpper();
                ws.Range(1, 1, 1, colCount + 1).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ================= HEADER =================
                ws.Cell(3, 1).Value = "Sr. No";
                ws.Cell(3, 1).Style.Font.Bold = true;
                ws.Cell(3, 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;

                for (int i = 0; i < colCount; i++)
                {
                    ws.Cell(3, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    ws.Cell(3, i + 2).Style.Font.Bold = true;
                    ws.Cell(3, i + 2).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
                }

                // ================= DATA =================
                int row = 4;
                int srNo = 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < colCount; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Purchase_Report_*.xlsx");

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Purchase Excel Generated";
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
        [Route("Report_FoodWastage")]
        public IActionResult Report_FoodWastage(FoodWastageDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetFoodWastageDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"FoodWastage_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "FoodWastage_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_FoodWastage_Excel")]
        public IActionResult Report_FoodWastage_Excel(FoodWastageDetailsRequest request)
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

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();


                    // 🔹 Report Data
                    using SqlCommand cmd = new SqlCommand("Report_GetFoodWastageDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= EXCEL CREATION =================
                using var wb = new ClosedXML.Excel.XLWorkbook();
                var ws = wb.Worksheets.Add("Food Wastage Report");

                int colCount = headerTableDB.Columns.Count + 1; // + Sr No
                int rowIndex = 1;

                

                // ================= REPORT NAME =================
                ws.Cell(rowIndex, 1).Value = reportName.ToUpper();
                ws.Range(rowIndex, 1, rowIndex, colCount).Merge();
                ws.Cell(rowIndex, 1).Style.Font.Bold = true;
                ws.Cell(rowIndex, 1).Style.Font.FontSize = 14;
                ws.Cell(rowIndex, 1).Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                rowIndex += 2;

                // ================= TABLE HEADER =================
                int colIndex = 1;

                ws.Cell(rowIndex, colIndex).Value = "Sr. No";
                ws.Cell(rowIndex, colIndex).Style.Font.Bold = true;
                ws.Cell(rowIndex, colIndex).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.FromArgb(0, 121, 182);
                ws.Cell(rowIndex, colIndex).Style.Font.FontColor =
                    ClosedXML.Excel.XLColor.White;
                colIndex++;

                foreach (DataColumn col in headerTableDB.Columns)
                {
                    ws.Cell(rowIndex, colIndex).Value =
                        headerTableDB.Rows[0][col.ColumnName].ToString();
                    ws.Cell(rowIndex, colIndex).Style.Font.Bold = true;
                    ws.Cell(rowIndex, colIndex).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.FromArgb(0, 121, 182);
                    ws.Cell(rowIndex, colIndex).Style.Font.FontColor =
                        ClosedXML.Excel.XLColor.White;
                    colIndex++;
                }

                rowIndex++;

                // ================= TABLE DATA =================
                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    ws.Cell(rowIndex, 1).Value = srNo++;
                    colIndex = 2;

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        ws.Cell(rowIndex, colIndex).Value =
                            row[col]?.ToString() ?? "-";
                        colIndex++;
                    }
                    rowIndex++;
                }

                ws.Columns().AdjustToContents();

                // ================= SAVE FILE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"FoodWastage_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "FoodWastage_Report_*.xlsx");

                using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    wb.SaveAs(stream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Excel Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_Expense")]
        public IActionResult Report_Expense(ExpenseDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetExpenseDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Expense_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Expense_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }
                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_Expense_Excel")]
        public IActionResult Report_Expense_Excel(ExpenseDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using SqlCommand cmd = new SqlCommand("Report_GetExpenseDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Expense Report");

                int colCount = headerTableDB.Columns.Count + 1; // Sr No

                // ===== REPORT TITLE =====
                ws.Cell(1, 1).Value = reportName.ToUpper();
                ws.Range(1, 1, 1, colCount).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ===== HEADER =====
                int headerRow = 3;

                ws.Cell(headerRow, 1).Value = "Sr. No";
                ws.Cell(headerRow, 1).Style.Font.Bold = true;
                ws.Cell(headerRow, 1).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightBlue;

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                {
                    ws.Cell(headerRow, i + 2).Value =
                        headerTableDB.Rows[0][i].ToString();
                    ws.Cell(headerRow, i + 2).Style.Font.Bold = true;
                    ws.Cell(headerRow, i + 2).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.LightBlue;
                }

                // ===== DATA =====
                int row = headerRow + 1;
                int srNo = 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ===== SAVE =====
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Expense_Report_*.xlsx");

                string fileName = $"Expense_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Expense Excel Generated";
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
        [Route("Report_ProfitLoss")]
        public IActionResult Report_ProfitLoss(ProfitLossDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "ProfitLoss_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_ProfitLoss_Excel")]
        public IActionResult Report_ProfitLoss_Excel(ProfitLossDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("ProfitLoss");

                int row = 1;

                // ================= REPORT TITLE =================
                var titleRange = ws.Range(row, 1, row, dataTableDB.Columns.Count + 1);
                titleRange.Merge();
                titleRange.Value = reportName;

                titleRange.Style.Font.Bold = true;
                titleRange.Style.Font.FontSize = 14;
                titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                row += 2;

                // ================= HEADER =================
                ws.Cell(row, 1).Value = "Sr No";
                ws.Cell(row, 1).Style.Font.Bold = true;

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                {
                    ws.Cell(row, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    ws.Cell(row, i + 2).Style.Font.Bold = true;
                }

                row++;

                // ================= DATA =================
                int srNo = 1;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
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
        [Route("Report_Cashflow")]
        public IActionResult Report_Cashflow([FromBody] CashflowDetailsRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetCashflowDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Cashflow_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Cashflow_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_Cashflow_Excel")]
        public IActionResult Report_Cashflow_Excel(CashflowDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using SqlCommand cmd = new SqlCommand("Report_GetCashflowDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Cashflow");

                int colCount = headerTableDB.Columns.Count;

                // ================= REPORT TITLE =================
                ws.Cell(1, 1).Value = reportName;
                ws.Range(1, 1, 1, colCount + 1).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ================= HEADER =================
                ws.Cell(3, 1).Value = "Sr. No";
                ws.Cell(3, 1).Style.Font.Bold = true;
                ws.Cell(3, 1).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightBlue;

                for (int i = 0; i < colCount; i++)
                {
                    ws.Cell(3, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    ws.Cell(3, i + 2).Style.Font.Bold = true;
                    ws.Cell(3, i + 2).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.LightBlue;
                }

                // ================= DATA =================
                int row = 4;
                int srNo = 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < colCount; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Cashflow_Report_*.xlsx");

                string fileName = $"Cashflow_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Cashflow Excel Generated";
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
        [Route("Report_Modification")]
        public IActionResult Report_Modification(ModificationRequest request)
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;
                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetModificationDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

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
                if (request.WithHeader == true)
                {
                    document.Add(headerTable);
                }
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Modification_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Modification_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };
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
        [Route("Report_Modification_Excel")]
        public IActionResult Report_Modification_Excel(ModificationRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                    return errorResult;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();
                    using SqlCommand cmd = new SqlCommand("Report_GetModificationDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new ClosedXML.Excel.XLWorkbook();
                var ws = workbook.Worksheets.Add("Modification");

                int colCount = headerTableDB.Columns.Count;

                // ================= REPORT TITLE =================
                ws.Cell(1, 1).Value = reportName;
                ws.Range(1, 1, 1, colCount + 1).Merge();
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 16;
                ws.Cell(1, 1).Style.Alignment.Horizontal =
                    ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                // ================= HEADER =================
                ws.Cell(3, 1).Value = "Sr. No";
                ws.Cell(3, 1).Style.Font.Bold = true;
                ws.Cell(3, 1).Style.Fill.BackgroundColor =
                    ClosedXML.Excel.XLColor.LightBlue;

                for (int i = 0; i < colCount; i++)
                {
                    ws.Cell(3, i + 2).Value = headerTableDB.Rows[0][i].ToString();
                    ws.Cell(3, i + 2).Style.Font.Bold = true;
                    ws.Cell(3, i + 2).Style.Fill.BackgroundColor =
                        ClosedXML.Excel.XLColor.LightBlue;
                }

                // ================= DATA =================
                int row = 4;
                int srNo = 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    ws.Cell(row, 1).Value = srNo++;

                    for (int i = 0; i < colCount; i++)
                        ws.Cell(row, i + 2).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                ws.Columns().AdjustToContents();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Modification_Report_*.xlsx");

                string fileName = $"Modification_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Modification Excel Generated";
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

public class PdfPageEvents : PdfPageEventHelper
{
    private readonly string _username;

    public PdfPageEvents(string username)
    {
        _username = username;
    }

    Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
    BaseColor lineColor = new BaseColor(180, 180, 180);

    public override void OnEndPage(PdfWriter writer, Document document)
    {
        PdfContentByte cb = writer.DirectContent;

        float lineY = document.Bottom - 5;   // 👈 LINE position
        float textY = document.Bottom - 20;  // 👈 FOOTER TEXT position

        // ================= LINE (BORDER) =================
        cb.SetLineWidth(0.5f);
        cb.SetColorStroke(BaseColor.GRAY);
        cb.MoveTo(document.Left, lineY);
        cb.LineTo(document.Right, lineY);
        cb.Stroke();

        // ================= FOOTER TEXT =================
        cb.BeginText();

        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        cb.SetFontAndSize(bf, 8);
        cb.SetColorFill(BaseColor.DARK_GRAY);

        cb.ShowTextAligned(
            Element.ALIGN_LEFT,
            $"User : {_username}",
            document.Left,
            textY,
            0
        );

        cb.ShowTextAligned(
            Element.ALIGN_CENTER,
            "Page " + writer.PageNumber,
            (document.Left + document.Right) / 2,
            textY,
            0
        );

        cb.ShowTextAligned(
            Element.ALIGN_RIGHT,
            DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"),
            document.Right,
            textY,
            0
        );

        cb.EndText();
    }
}
