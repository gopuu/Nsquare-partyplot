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

        #region Booking Calendar - reports
        [HttpPost]
        [Route("DownloadCustomerInvoiceReport")]
        public IActionResult DownloadCustomerInvoiceReport([FromBody] CustomerInvoiceReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
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
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // 🔹 Invoice Report
                    cmd = new SqlCommand("Report_CustomerInvoiceById", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                    cmd.Parameters.AddWithValue("@InvoiceNo", request.InvoiceNo);
                    cmd.Parameters.AddWithValue("@ProformaInvoiceNo", request.ProformaInvoiceNo);
                    new SqlDataAdapter(cmd).Fill(dtInvoice);
                }

                if (dtInvoice.Rows.Count == 0)
                    throw new Exception("Invoice data not found");

                // Safe Invoice Row
                DataRow inv = null;
                if (dtInvoice != null && dtInvoice.Rows.Count > 0)
                {
                    inv = dtInvoice.Rows[0];
                }

                // Safe Company Data
                string companyName = "";
                string companyAddress = "";
                string companyPhone = "";
                string logoFile = "";
                string userName = "";

                bool isCompanyAvailable = dtCompany != null && dtCompany.Rows.Count > 0;

                if (isCompanyAvailable)
                {
                    companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
                    companyAddress = dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "";
                    companyPhone = dtCompany.Rows[0]["CompanyMobile"]?.ToString() ?? "";
                    logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? "";
                    userName = dtCompany.Rows[0]["UserName"]?.ToString() ?? "";
                }

                // Page Event (safe even if userName empty)
                writer.PageEvent = new PdfPageEvents(userName);
                document.Open();

                // ================= HEADER =================
                if (request.WithHeader == true)
                {
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 20f, 80f });

                    if (isCompanyAvailable)
                    {
                        string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);

                        if (System.IO.File.Exists(logoPath))
                        {
                            Image logo = Image.GetInstance(logoPath);
                            logo.ScaleToFit(70f, 70f);

                            headerTable.AddCell(new PdfPCell(logo)
                            {
                                Border = Rectangle.BOTTOM_BORDER,
                                Padding = 10
                            });
                        }
                        else
                        {
                            headerTable.AddCell(new PdfPCell(new Phrase(""))
                            {
                                Border = Rectangle.BOTTOM_BORDER,
                                Padding = 10
                            });
                        }

                        PdfPCell infoCell = new PdfPCell
                        {
                            Border = Rectangle.BOTTOM_BORDER,
                            Padding = 10,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };

                        infoCell.AddElement(new Paragraph(companyName,
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));

                        infoCell.AddElement(new Paragraph(companyAddress,
                            FontFactory.GetFont(FontFactory.HELVETICA, 9)));

                        infoCell.AddElement(new Paragraph(companyPhone,
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                        headerTable.AddCell(infoCell);
                    }
                    else
                    {
                        // Same behavior as previous solution
                        PdfPCell messageCell = new PdfPCell(
                            new Phrase("[PAGE HEADER] Company Data Not Found",
                            FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.RED)))
                        {
                            Border = Rectangle.BOTTOM_BORDER,
                            Colspan = 2,
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 15
                        };

                        headerTable.AddCell(messageCell);
                    }

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

                infoTable.AddCell(Utils.CreateInfoCell("Invoice No", invoiceNoToShow));
                infoTable.AddCell(Utils.CreateInfoCell("Invoice Date", inv["InvoiceDate_Format"]));
                infoTable.AddCell(Utils.CreateInfoCell("Customer Name", inv["CustomerName"]));
                infoTable.AddCell(Utils.CreateInfoCell("GSTIN", inv["GSTIN"]));
                infoTable.AddCell(Utils.CreateInfoCell("Email", inv["EmailId"]));
                infoTable.AddCell(Utils.CreateInfoCell("Address", inv["Address"]));

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
                    itemTable.AddCell(Utils.CreateHeaderCell(h, thFont, thColor));

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
                        itemTable.AddCell(Utils.CreateBodyCell(r["SrNo"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(Utils.CreateBodyCell(r["Description"].ToString(), tdFont));
                        itemTable.AddCell(Utils.CreateBodyCell(r["HSN"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(Utils.CreateBodyCell(r["Rate"].ToString(), tdFont, Element.ALIGN_RIGHT));
                        itemTable.AddCell(Utils.CreateBodyCell(r["Qty"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(Utils.CreateBodyCell(r["TaxableAmount"].ToString(), tdFont, Element.ALIGN_RIGHT));
                        itemTable.AddCell(Utils.CreateBodyCell(r["GSTRate"].ToString(), tdFont, Element.ALIGN_CENTER));
                        itemTable.AddCell(Utils.CreateBodyCell(r["Total"].ToString(), tdFont, Element.ALIGN_RIGHT));
                    }
                }

                document.Add(itemTable);

                // ================= TOTALS =================
                document.Add(new Paragraph("\n"));
                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 40;
                totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalTable.SetWidths(new float[] { 60f, 40f });

                totalTable.AddCell(Utils.CreateTotalLabelCell("Sub Total"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["SubTotal"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Booking Discount"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["BookingDiscount"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Total Taxable"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["TotalTaxableAmount"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Total GST"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["TotalGST"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Round Off"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["RoundOff"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Advance Paid"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["TotalAdvancePaid"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Grand Total", true));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["GrandTotal"], true));

                document.Add(totalTable);

                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder))
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
        [Route("DownloadBookingEventReport")]
        public IActionResult DownloadBookingEventReport([FromBody] DownloadBookingEventReportRequest request)
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
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // ================= BOOKING EVENT DETAILS =================
                    cmd = new SqlCommand("Report_GetBookingEventDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    // 🔹 BookingId Parameter
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    SqlDataAdapter daReport = new SqlDataAdapter(cmd);
                    daReport.Fill(dsReport);
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
                /* ================= HEADER ================= */
                PdfPTable header = new PdfPTable(2);
                header.WidthPercentage = 100;
                header.SetWidths(new float[] { 20, 80 });

                if (dtCompany != null && dtCompany.Rows.Count > 0)
                {
                    string logoPath = Path.Combine(
                        _env.WebRootPath,
                        "assets2",
                        "img",
                        dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? ""
                    );

                    if (System.IO.File.Exists(logoPath))
                    {
                        Image logo = Image.GetInstance(logoPath);
                        logo.ScaleToFit(60, 60);
                        header.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER });
                    }
                    else
                    {
                        header.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER });
                    }

                    PdfPCell compCell = new PdfPCell
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };

                    compCell.AddElement(new Paragraph(
                        dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));

                    compCell.AddElement(new Paragraph(
                        dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "",
                        FontFactory.GetFont(FontFactory.HELVETICA, 9)));

                    header.AddCell(compCell);
                }
                else
                {
                    // No company data found
                    PdfPCell messageCell = new PdfPCell(
                        new Phrase("[PAGE HEADER] Company Data Not Found",
                        FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.RED)))
                    {
                        Border = Rectangle.NO_BORDER,
                        Colspan = 2,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };

                    header.AddCell(messageCell);
                }

                document.Add(header);
                document.Add(new Paragraph("\n"));

                /* ================= EVENT FORM TITLE ================= */
                PdfPTable titleTable = new PdfPTable(2);
                titleTable.WidthPercentage = 100;
                titleTable.SetWidths(new float[] { 70, 30 });

                PdfPCell titleCell = Utils.CreateCell(
                    dtFormTitle.Rows[0]["FormTitle"].ToString(),
                    titleFont,
                    Element.ALIGN_CENTER);
                titleCell.BackgroundColor = new BaseColor(230, 230, 230); // light gray
                titleTable.AddCell(titleCell);

                PdfPCell dateCell = Utils.CreateCell(dtFormTitle.Rows[0]["DateTitle"].ToString(),titleFont);
                dateCell.BackgroundColor = new BaseColor(230, 230, 230);
                titleTable.AddCell(dateCell);

                document.Add(titleTable);


                /* ================= GUEST DETAILS ================= */
                PdfPTable guestTable = new PdfPTable(2);
                guestTable.WidthPercentage = 100;
                guestTable.SetWidths(new float[] { 30, 70 });

                guestTable.AddCell(Utils.CreateCell(dtBasic.Rows[0]["GuestNameTitle"].ToString(), boldFont));
                guestTable.AddCell(Utils.CreateCell(dtBasic.Rows[0]["GuestName"].ToString(), normalFont));

                guestTable.AddCell(Utils.CreateCell(dtBasic.Rows[0]["EventDetailsTitle"].ToString(), boldFont));
                guestTable.AddCell(Utils.CreateCell(dtBasic.Rows[0]["EventDetails"].ToString(), normalFont));

                document.Add(guestTable);

                /* ================= PLATE TABLE ================= */
                PdfPTable plateTable = new PdfPTable(4);
                plateTable.WidthPercentage = 100;
                plateTable.SetWidths(new float[] { 30, 20, 25, 25 });

                plateTable.AddCell(Utils.CreateCell("", boldFont));
                plateTable.AddCell(Utils.CreateCell(dtHeaders.Rows[0]["Col_NoOfPlates"].ToString(), boldFont, Element.ALIGN_CENTER));
                plateTable.AddCell(Utils.CreateCell(dtHeaders.Rows[0]["Col_AuthorisedSignature"].ToString(), boldFont, Element.ALIGN_CENTER));
                plateTable.AddCell(Utils.CreateCell(dtHeaders.Rows[0]["Col_GuestSignature"].ToString(), boldFont, Element.ALIGN_CENTER));

                foreach (DataRow r in dtPlateRows.Rows)
                {
                    plateTable.AddCell(Utils.CreateCell(r["RowTitle"].ToString(), normalFont));
                    plateTable.AddCell(Utils.CreateCell(r["NoOfPlates"].ToString(), normalFont));
                    plateTable.AddCell(Utils.CreateCell(r["AuthSign"].ToString(), normalFont));
                    plateTable.AddCell(Utils.CreateCell(r["GuestSignature"].ToString(), normalFont));
                }

                document.Add(plateTable);

                /* ================= SUMMARY ================= */
                PdfPTable summaryTable = new PdfPTable(4);
                summaryTable.WidthPercentage = 100;

                summaryTable.AddCell(Utils.CreateCell(
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
                paymentTable.AddCell(Utils.CreateCell(dtPayment.Rows[0]["RatePerPlateTitle"].ToString(), boldFont));
                paymentTable.AddCell(Utils.CreateCell(dtPayment.Rows[0]["RatePerPlate"].ToString(), normalFont));

                paymentTable.AddCell(Utils.CreateTitleWithValueCell(
                    dtPayment.Rows[0]["ExtraAmountTitle"].ToString(),
                    dtPayment.Rows[0]["ExtraAmount"].ToString(),   // 👈 VALUE HERE
                    boldFont,
                    normalFont,
                    55f
                ));
                /* ---------- Row 2 ---------- */
                paymentTable.AddCell(Utils.CreateCell(dtPayment.Rows[0]["TotalPayableTitle"].ToString(), boldFont));
                PdfPCell totalPayableCell = Utils.CreateCell(dtPayment.Rows[0]["TotalPayable"].ToString(), normalFont);
                totalPayableCell.Colspan = 2;
                paymentTable.AddCell(totalPayableCell);

                /* ---------- Row 3 ---------- */
                paymentTable.AddCell(Utils.CreateCell(dtPayment.Rows[0]["AdvanceAmountTitle"].ToString(), boldFont));
                paymentTable.AddCell(Utils.CreateCell(dtPayment.Rows[0]["AdvanceAmount"].ToString(), normalFont));

                paymentTable.AddCell(Utils.CreateTitleWithValueCell(
                    dtPayment.Rows[0]["AdvancePaymentModeTitle"].ToString(),
                    dtPayment.Rows[0]["AdvancePaymentMode"].ToString(),   // 👈 VALUE HERE
                    boldFont,
                    normalFont,
                    55f
                ));

                /* ---------- Row 4 ---------- */
                paymentTable.AddCell(Utils.CreateCell(dtPayment.Rows[0]["FinalRemainingPaymentTitle"].ToString(), boldFont));
                paymentTable.AddCell(Utils.CreateCell(dtPayment.Rows[0]["FinalRemainingPayment"].ToString(), normalFont));

                paymentTable.AddCell(Utils.CreateTitleWithValueCell(
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

                footer.AddCell(Utils.CreateCell(
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
                string imgPath = Path.Combine(_env.WebRootPath, "assets2", "img", "feedback-stars.png");

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
                titleTbl.AddCell(Utils.CreateCell2(dtStaffTitle.Rows[0]["StaffUseOnlyTitle"].ToString(), titleFont, Element.ALIGN_CENTER, 1, 30));
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
                rightBox.AddCell(Utils.CreateCell2(
                    dtFoodTitles.Rows[0]["LocationTimeTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(Utils.CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));

                // Row 2
                rightBox.AddCell(Utils.CreateCell2(
                    dtFoodTitles.Rows[0]["WeightDoneByTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(Utils.CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));

                // Row 3
                rightBox.AddCell(Utils.CreateCell2(
                    dtFoodTitles.Rows[0]["WeightCrossCheckByTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(Utils.CreateCell2(
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
                itemTable.AddCell(Utils.CreateCell2(
                    dtFoodTitles.Rows[0]["ItemNameTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 1, 25));

                itemTable.AddCell(Utils.CreateCell2(
                    dtFoodTitles.Rows[0]["WeightKgTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 1, 25));

                // 7 blank rows
                for (int i = 0; i < 7; i++)
                {
                    itemTable.AddCell(Utils.CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 25));
                    itemTable.AddCell(Utils.CreateCell2("", normalFont, Element.ALIGN_CENTER, 1, 25));
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
                staffTbl.AddCell(Utils.CreateCell2(
                    dtStaffDuty.Rows[0]["StaffDutyTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 2, 25));

                // --- PRESENT STAFF 
                staffTbl.AddCell(Utils.CreateCell2(
                    dtStaffDuty.Rows[0]["StaffPresentTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 50)); // Label height: 50

                staffTbl.AddCell(Utils.CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 80)); 

                // --- ABSENT STAFF 
                staffTbl.AddCell(Utils.CreateCell2(
                    dtStaffDuty.Rows[0]["StaffAbsentTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 30)); // Label height: 30

                staffTbl.AddCell(Utils.CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 30)); 

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

                chkTbl.AddCell(Utils.Cell(
                    dtChkTitle.Rows[0]["ChecklistTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    2,
                    30
                ));

                foreach (DataRow r in dtChkItems.Rows)
                {
                    chkTbl.AddCell(Utils.Cell(r["ChecklistItem"].ToString(), normalFont));
                    chkTbl.AddCell(Utils.Cell("", normalFont));
                }

                document.Add(chkTbl);

                PdfPTable notesTbl = new PdfPTable(1);
                notesTbl.WidthPercentage = 100;

                notesTbl.AddCell(Utils.Cell(
                    dtNotes.Rows[0]["EventNotesTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    1,
                    30
                ));

                notesTbl.AddCell(Utils.Cell("", normalFont, Element.ALIGN_LEFT, 1, 120));
                document.Add(notesTbl);

                document.Close();

                /* ================= SAVE ================= */
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"Booking_EventReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                Utils.deleteOldFiles(folder, "Booking_EventReport_*.pdf", 60);

                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "PDF Generated Successfully";
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
        [Route("DownloadBookingReceipt")]
        public IActionResult DownloadBookingReceipt([FromBody] DownloadBookingReceiptRequest request)
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

                //Document document = new Document(PageSize.A4, 25, 25, 20, 30);
                Rectangle customSize = new Rectangle(PageSize.A4.Width, 390); // 🔥 reduce height
                Document document = new Document(customSize, 25, 25, 20, 30);

                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                document.Open();

                DataTable dtCompany = new DataTable();
                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // Header
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // Receipt
                    cmd = new SqlCommand("Report_GetBookingReceipt", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    new SqlDataAdapter(cmd).Fill(dt);
                }

                if (dt.Rows.Count == 0)
                    throw new Exception("No data found");

                DataRow r = dt.Rows[0];

                /* ================= VARIABLES ================= */
                string receiptNo = r["ReceiptNo"].ToString();
                string receiptDate = r["PaymentDate"].ToString();

                string customerName = r["Client_Name"].ToString();
                string eventDate = r["EventDate"].ToString();
                string eventName = r["Booking_SubEventName"].ToString();
                string banquet = r["BanquetName"].ToString();
                string startTime = r["StartTime_Full"].ToString();
                string shift = r["ShiftFullName"].ToString();

                string paymentMode = r["PaymentMode"].ToString();
                string amount = Convert.ToDecimal(r["PaymentAmount"]).ToString("N0");
                string amountWords = r["PaymentAmountInWords"].ToString() + " Only";
                string createdBy = r["Payment_CreatedBy"].ToString();

                /* ================= FONTS ================= */
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13);
                Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                /* ================= HEADER ================= */
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                // 🔹 Logo
                Image logo = null;
                string logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString();

                if (!string.IsNullOrWhiteSpace(logoFile))
                {
                    string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);

                    if (System.IO.File.Exists(logoPath))
                    {
                        logo = Image.GetInstance(logoPath);
                    }
                }

                PdfPCell logoCell;

                if (logo != null)
                {
                    logo.ScaleToFit(70f, 70f);
                    logoCell = new PdfPCell(logo);
                }
                else
                {
                    logoCell = new PdfPCell(new Phrase(""));
                }

                logoCell.Border = Rectangle.BOTTOM_BORDER;
                logoCell.Padding = 10;
                headerTable.AddCell(logoCell);

                // 🔹 Company Details
                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyName"].ToString(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));

                detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyAddress"].ToString(),
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));

                detailCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyMobile"].ToString(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);

                document.Add(headerTable);

                //document.Add(new Paragraph("\n"));

                /* ================= TOP ROW ================= */
                PdfPTable titleTable = new PdfPTable(1);
                titleTable.WidthPercentage = 100;

                PdfPCell titleCell = new PdfPCell(
                    new Phrase("BOOKING RECEIPT",
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.WHITE))
                );

                titleCell.BackgroundColor = new BaseColor(0, 102, 204);
                titleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                titleCell.Padding = 10;
                titleCell.Border = Rectangle.NO_BORDER;

                titleTable.AddCell(titleCell);
                document.Add(titleTable);

                //document.Add(new Paragraph("\n"));

                PdfPTable top = new PdfPTable(2);
                top.WidthPercentage = 100;
                top.SetWidths(new float[] { 70, 30 });

                top.AddCell(new PdfPCell(new Phrase($"RECEIPT NO: {receiptNo}", boldFont))
                { Border = Rectangle.NO_BORDER });

                top.AddCell(new PdfPCell(new Phrase($"DATE:  {receiptDate}", boldFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });

                document.Add(top);
                //document.Add(new Paragraph("\n"));

                /* ================= MAIN TABLE ================= */
                PdfPTable tbl = new PdfPTable(3);
                tbl.WidthPercentage = 100;
                tbl.SetWidths(new float[] { 30, 5, 65 });

                void AddRow(string label, string value)
                {
                    tbl.AddCell(new PdfPCell(new Phrase(label, boldFont)) { Border = Rectangle.NO_BORDER });

                    tbl.AddCell(new PdfPCell(new Phrase(":", boldFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    tbl.AddCell(new PdfPCell(new Phrase(value, normalFont))
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        PaddingBottom = 5
                    });
                }

                AddRow("Received with thanks from", customerName);

                // EVENT DETAILS (multi-line like image)
                AddRow("Event details", $"Event Date : {eventDate}        Event : {eventName}");
                AddRow("", $"Banquet(s) : {banquet} at {startTime} ({shift})");

                AddRow("Mode of Payment", paymentMode);
                AddRow("Reference No", "");
                AddRow("The sum of rupees", amountWords);
                AddRow("Receipt created by", createdBy);
                AddRow("Note", "");

                document.Add(tbl);

                document.Add(new Paragraph("\n"));

                /* ================= AMOUNT + SIGN ================= */
                PdfPTable amtTbl = new PdfPTable(2);
                amtTbl.WidthPercentage = 100;
                amtTbl.SetWidths(new float[] { 30, 70 });

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font AmtFont = new Font(bf, 13, Font.BOLD);
                PdfPCell amtCell = new PdfPCell(new Phrase($"₹ {amount}", AmtFont))
                {
                    Border = Rectangle.BOX,
                    FixedHeight = 45,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                amtTbl.AddCell(amtCell);

                amtTbl.AddCell(new PdfPCell(new Phrase("Authorised Signatory", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_BOTTOM
                });

                document.Add(amtTbl);

                document.Close();

                /* ================= SAVE ================= */
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Booking_Receipt_*.pdf");

                string fileName = $"Booking_Receipt_{DateTime.Now:yyyyMMddHHmmss}.pdf";

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
        #endregion

        #region REPORT 12 - Emergency Contact REPORT

        [HttpPost]
        [Route("Report_EmergencyContact")]
        public IActionResult Report_EmergencyContact([FromBody] EmergencyContactReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                // Ensure JSON columns
                if (string.IsNullOrWhiteSpace(request.Columns))
                {
                    request.Columns = "[\"CategoryName\",\"SubCategoryName\",\"ContactType\",\"ContactName\",\"MobileNo\",\"MobileNo2\",\"Location\",\"Remarks\",\"IsActive\"]";
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

                    // Company Header
                    using (SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                        new SqlDataAdapter(cmd).Fill(dtCompany);
                    }

                    // Report Data
                    using (SqlCommand cmd = new SqlCommand("Report_GetEmergencyContact", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = request.Columns;
                        cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id;
                        cmd.Parameters.Add("@CategoryId", SqlDbType.BigInt).Value = request.CategoryId;
                        cmd.Parameters.Add("@SubCategoryId", SqlDbType.BigInt).Value = request.SubCategoryId;
                        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value =
                            request.IsActive == null ? DBNull.Value : request.IsActive;

                        new SqlDataAdapter(cmd).Fill(dsReport);
                    }
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data is incomplete.");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                writer.PageEvent = new PdfPageEvents(request.ActionBy.ToString() ?? "");

                document.Open();

                // ================= HEADER =================
                if (request.WithHeader ?? false)
                {
                    string companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
                    string companyAddress = dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "";
                    string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"]?.ToString() ?? "";
                    string logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? "";

                    Image logo = null;

                    if (!string.IsNullOrWhiteSpace(logoFile))
                    {
                        string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);

                        if (System.IO.File.Exists(logoPath))
                            logo = Image.GetInstance(logoPath);
                    }

                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 20f, 80f });

                    PdfPCell logoCell;

                    if (logo != null)
                    {
                        logo.ScaleToFit(70f, 70f);
                        logoCell = new PdfPCell(logo);
                    }
                    else
                    {
                        logoCell = new PdfPCell(new Phrase(""));
                    }

                    logoCell.Border = Rectangle.BOTTOM_BORDER;
                    logoCell.Padding = 10;
                    headerTable.AddCell(logoCell);

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

                    document.Add(headerTable);
                }

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= REPORT TITLE =================

                Font reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
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

                // Header
                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                {
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));
                }

                table.HeaderRows = 1;

                // Data
                if (dataTableDB.Rows.Count == 0)
                {
                    PdfPCell noDataCell = new PdfPCell(new Phrase("No Data Found"))
                    {
                        Colspan = columnCount,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 12f
                    };

                    table.AddCell(noDataCell);
                }
                else
                {
                    foreach (DataRow row in dataTableDB.Rows)
                    {
                        for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        {
                            table.AddCell(Utils.CreateBodyCell(row[i]?.ToString() ?? "-", tdFont));
                        }
                    }
                }

                document.Add(table);
                document.Close();

                // Save file
                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "EmergencyContactReport_*.pdf");

                string fileName = $"EmergencyContactReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Emergency Contact Report Generated";
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
        [Route("Report_EmergencyContact_Excel")]
        public IActionResult Report_EmergencyContact_Excel([FromBody] EmergencyContactReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                if (string.IsNullOrWhiteSpace(request.Columns))
                {
                    request.Columns = "[\"CategoryName\",\"SubCategoryName\",\"ContactType\",\"ContactName\",\"MobileNo\",\"MobileNo2\",\"Location\",\"Remarks\",\"IsActive\"]";
                }

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("Report_GetEmergencyContact", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = request.Columns;
                        cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id;
                        cmd.Parameters.Add("@CategoryId", SqlDbType.BigInt).Value = request.CategoryId;
                        cmd.Parameters.Add("@SubCategoryId", SqlDbType.BigInt).Value = request.SubCategoryId;
                        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value =
                            request.IsActive == null ? DBNull.Value : request.IsActive;

                        new SqlDataAdapter(cmd).Fill(dsReport);
                    }
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("EmergencyContacts");

                int colCount = headerTableDB.Rows.Count;

                // Headers
                for (int i = 0; i < colCount; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headerTableDB.Rows[i][0].ToString();
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data
                int row = 2;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                string reportsDir = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(reportsDir))
                    Directory.CreateDirectory(reportsDir);

                Utils.deleteOldFiles(reportsDir, "EmergencyContactReport_*.xlsx");

                string fileName = $"EmergencyContactReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                string filePath = Path.Combine(reportsDir, fileName);

                workbook.SaveAs(filePath);

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Emergency Contact Report Excel generated successfully.";
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
        #endregion
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
