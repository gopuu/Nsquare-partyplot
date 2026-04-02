using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using SixLabors.Fonts;
using System.Data;
using System.Data.SqlClient;
using iTextSharpFont = iTextSharp.text.Font;

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

        // ─────────────────────────────────────────────
        //  POST  /api/Invoice/DownloadGSTInvoice
        // ─────────────────────────────────────────────
        [HttpPost]
        [Route("DownloadCustomerInvoiceReport")]
        public IActionResult DownloadCustomerInvoiceReport([FromBody] CustomerInvoiceReportRequest request)
        {
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
                string createdBy = "";

                CompanyInfo company = new CompanyInfo();
                if (dtCompany != null && dtCompany.Rows.Count > 0)
                {
                    DataRow row = dtCompany.Rows[0];
                    company = new CompanyInfo
                    {
                        Name = row["CompanyName"]?.ToString() ?? "",
                        Address = row["CompanyAddress"]?.ToString() ?? "",
                        Phone = row["CompanyMobile"]?.ToString() ?? "",
                        Email = row["CompanyEmail"]?.ToString() ?? "",
                        GST = row["CompanyGST"]?.ToString() ?? "",
                        PAN = row["CompanyPAN"]?.ToString() ?? "",
                        State = row["StateName"]?.ToString() ?? "",
                        StateCode = row["StateCode"]?.ToString() ?? "",
                        BankName = row["BankName"]?.ToString() ?? "",
                        AccountNo = row["AccountNo"]?.ToString() ?? "",
                        Branch = row["BranchName"]?.ToString() ?? "",
                        IFSC = row["IFSCCode"]?.ToString() ?? "",
                        UPI = row["UPI"]?.ToString() ?? "",
                        Logo = row["CompanyLogo"]?.ToString() ?? "",
                    };
                    createdBy = row["UserName"]?.ToString() ?? "";
                }

                var header = new InvoiceHeader();
                var billTo = new BillToInfo();
                var totals = new InvoiceTotals();
                var items = new List<InvoiceItem>();

                if (dtInvoice != null && dtInvoice.Rows.Count > 0)
                {
                    DataRow row = dtInvoice.Rows[0];

                    // 🔹 Header
                    header = new InvoiceHeader
                    {
                        ProformaInvoiceNo = Convert.ToInt64(row["ProformaInvoiceNo"]),
                        InvoiceNo = Convert.ToInt64(row["InvoiceNo"]),
                        Date = row["InvoiceDate_Format"].ToString() ?? "",
                        BookingRef = row["BookingRef"].ToString() ?? "",
                        EventDate = row["EventDate_Format"].ToString() ?? "",
                        EventType = row["EventType"].ToString() ?? "",
                        Hall = row["Hall"].ToString() ?? ""
                    };

                    // 🔹 Bill To
                    billTo = new BillToInfo
                    {
                        Name = row["CustomerName"].ToString() ?? "",
                        Website = row["Website"]?.ToString() ?? "",
                        Address = row["Address"].ToString() ?? "",
                        Phone = row["Phone"].ToString() ?? "",
                        GSTIN = row["GSTIN"].ToString() ?? "",
                        PAN = row["PAN"]?.ToString() ?? ""
                    };

                    // 🔹 Totals
                    totals = new InvoiceTotals
                    {
                        SubTotal = Convert.ToDecimal(row["SubTotal"]),
                        Taxable = Convert.ToDecimal(row["Taxable"]),
                        TotalDiscount = Convert.ToDecimal(row["TotalDiscount"]),
                        TotalCGST = Convert.ToDecimal(row["TotalCGST"]),
                        TotalSGST = Convert.ToDecimal(row["TotalSGST"]),
                        TotalIGST = Convert.ToDecimal(row["TotalIGST"]),
                        InvoiceTotal = Convert.ToDecimal(row["InvoiceTotal"]),
                        RoundOff = Convert.ToDecimal(row["RoundOff"]),
                        TotalAdvance = Convert.ToDecimal(row["TotalAdvance"]),
                        Balance = Convert.ToDecimal(row["Balance"]),
                        AmountInWords = row["AmountInWords"].ToString() ?? ""
                    };

                    string? json = dtInvoice.Rows[0]["JSON_Items"]?.ToString();
                    if (!string.IsNullOrEmpty(json))
                    {
                        items = JsonConvert.DeserializeObject<List<InvoiceItem>>(json);
                    }
                }

                // ══════════════════════════════════════════
                //  BUILD PDF
                // ══════════════════════════════════════════
                using var stream = new MemoryStream();
                var doc = new Document(PageSize.A4, 28, 28, 36, 50);
                var writer = PdfWriter.GetInstance(doc, stream);
                writer.CloseStream = false;

                // 👉 Add this line
                writer.PageEvent = new PageBorder();

                doc.Open();

                // ── Fonts ──────────────────────────────────
                var fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharpFont.NORMAL);
                var fBold = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextSharpFont.BOLD);
                var fSmall7 = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharpFont.NORMAL);
                var fSmall7Bold = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharpFont.BOLD);
                var fSmall = FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharpFont.NORMAL);
                var fSmallB = FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharpFont.BOLD);
                var fTitle = FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextSharpFont.BOLD);
                var fHeader = FontFactory.GetFont(FontFactory.HELVETICA, 11, iTextSharpFont.BOLD);
                var fWhite = FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextSharpFont.BOLD, BaseColor.WHITE);
                //var fGrayHdr = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextSharpFont.BOLD, BaseColor.WHITE);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arialbd.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharpFont fBoldRupee = new iTextSharpFont(bf, 8, iTextSharpFont.NORMAL);
                iTextSharpFont fBoldRupee7 = new iTextSharpFont(bf, 7, iTextSharpFont.NORMAL);  

                // ── Colors ─────────────────────────────────
                var colWhite = BaseColor.WHITE;   // header blue
                var colBlue = new BaseColor(0, 102, 204);   // header blue
                var colLightBlue = new BaseColor(173, 216, 230);  // sub-header light blue
                var colTableHead = colWhite; // new BaseColor(30, 144, 255);  // item-table header
                var colGray = new BaseColor(245, 245, 245);  // alternating row bg
                var colBorder = BaseColor.LIGHT_GRAY; //new BaseColor(200, 200, 200);

                #region PAGE HEADER
                // ══════════════════════════════════════════
                //  1. TOP HEADER BAR  (Tax Invoice + meta + image)
                // ══════════════════════════════════════════
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 75f, 25f });

                // Light gray background (like image)
                BaseColor headerBg = BaseColor.WHITE; //new BaseColor(240, 240, 240);

                // ── LEFT SIDE (Title + Meta) ──
                PdfPTable leftTable = new PdfPTable(1);
                leftTable.WidthPercentage = 100;

                // Title
                var titleText = header.ProformaInvoiceNo > 0 ? "Proforma Invoice" : "Tax Invoice";
                leftTable.AddCell(new PdfPCell(new Phrase(titleText, fTitle))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 6
                });

                // Meta table
                PdfPTable metaTable = new PdfPTable(2);
                metaTable.SetWidths(new float[] { 20f, 80f });
                metaTable.WidthPercentage = 100;

                void AddMeta(string k, string v)
                {
                    metaTable.AddCell(new PdfPCell(new Phrase(k, fSmall))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 1
                    });

                    metaTable.AddCell(new PdfPCell(new Phrase(": " + v, fSmall))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 1
                    });
                }

                AddMeta(titleText, header.InvoiceNo.ToString() ?? "-");
                AddMeta("Date", header.Date);
                AddMeta("Booking Ref#", header.BookingRef);
                AddMeta("Event Date", header.EventDate);
                AddMeta("Event Type", header.EventType);
                AddMeta("Hall(s)", header.Hall);

                // Add meta into left block
                leftTable.AddCell(new PdfPCell(metaTable)
                {
                    Border = Rectangle.NO_BORDER
                });

                // Wrap LEFT
                headerTable.AddCell(new PdfPCell(leftTable)
                {
                    BackgroundColor = headerBg,
                    Border = Rectangle.NO_BORDER,
                    BorderWidth = 0.75f,
                    Padding = 4,
                    PaddingTop = 2
                });

                // ── RIGHT SIDE (LOGO) ──
                string? logoFile = company.Logo ?? String.Empty; 
                PdfPCell logoCell;

                string logoPath = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "CompanyInvoice_CompanyLogo_Image", logoFile);

                if (System.IO.File.Exists(logoPath))
                {
                    Image logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(120f, 120f);

                    logoCell = new PdfPCell(logo);
                }
                else
                {
                    logoCell = new PdfPCell(new Phrase(""));
                }

                logoCell.BackgroundColor = headerBg;
                logoCell.Border = Rectangle.NO_BORDER;
                logoCell.BorderWidth = 0.75f;
                logoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                logoCell.VerticalAlignment = Element.ALIGN_TOP;
                logoCell.Padding = 4;
                logoCell.PaddingTop = 2;

                headerTable.AddCell(logoCell);

                // Add to document
                doc.Add(headerTable);
                #endregion

                #region CUSTOMER DETAILS
                // ══════════════════════════════════════════
                //  3. BILL TO SECTION (MATCH IMAGE STYLE)
                // ══════════════════════════════════════════
                PdfPTable billToTable = new PdfPTable(1);
                billToTable.WidthPercentage = 100;

                // Container cell
                PdfPCell billCell = new PdfPCell();
                billCell.BackgroundColor = BaseColor.WHITE;
                billCell.Border = Rectangle.NO_BORDER;
                billCell.BorderWidth = 0.75f;
                billCell.Padding = 6;

                // ── Content ──
                // Title
                billCell.AddElement(new Paragraph("Bill To:", fSmall));

                // Company Name (Bold & Bigger)
                billCell.AddElement(new Paragraph(billTo.Name.ToUpper(), fBold));
                billCell.AddElement(new Paragraph(billTo.Website, fSmall7));
                billCell.AddElement(new Paragraph(billTo.Address, fSmall));
                billCell.AddElement(new Paragraph("Phone: " + billTo.Phone, fSmall));
                billCell.AddElement(new Paragraph("GSTIN: " + billTo.GSTIN, fSmall));
                billCell.AddElement(new Paragraph("PAN: " + billTo.PAN, fSmall));
                billToTable.AddCell(billCell);

                doc.Add(billToTable);
                #endregion

                #region ITEMS
                // ══════════════════════════════════════════
                //  4. ITEMS TABLE
                // ══════════════════════════════════════════
                // Columns: HSN/SAC | Name of Service | Rate | Qty | Subtotal | Taxable Value
                //          | CGST Tax% | CGST Amt | SGST Tax% | SGST Amt | IGST Tax% | IGST Amt
                PdfPTable itemTable = new PdfPTable(12);
                itemTable.WidthPercentage = 100;
                itemTable.SetWidths(new float[] { 7, 18, 6, 5, 8, 8, 5, 6, 5, 6, 5, 6 });

                // ── Helper for header cell ──
                PdfPCell TH(string text, int colspan = 1, int rowspan = 1)
                {
                    return new PdfPCell(new Phrase(text, fSmall7Bold))
                    {
                        BackgroundColor = colTableHead,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Colspan = colspan,
                        Rowspan = rowspan,
                        BorderColor = colBorder,
                        BorderWidth = 0.5f,
                        Padding = 4
                    };
                }

                // Row 1 of header
                itemTable.AddCell(TH("HSN /\nSAC", 1, 2));
                itemTable.AddCell(TH("Name of the Service", 1, 2));
                itemTable.AddCell(TH("Rate", 1, 2));
                itemTable.AddCell(TH("Qty", 1, 2));
                itemTable.AddCell(TH("Subtotal", 1, 2));
                itemTable.AddCell(TH("Taxable Value", 1, 2));
                itemTable.AddCell(TH("CGST", 2));
                itemTable.AddCell(TH("SGST", 2));
                itemTable.AddCell(TH("IGST", 2));

                // Row 2 of header
                itemTable.AddCell(TH("Tax %"));
                itemTable.AddCell(TH("Amt."));
                itemTable.AddCell(TH("Tax %"));
                itemTable.AddCell(TH("Amt."));
                itemTable.AddCell(TH("Tax %"));
                itemTable.AddCell(TH("Amt."));

                itemTable.HeaderRows = 2;

                // ── Helper for data cell ──
                PdfPCell TD(string text, int align = Element.ALIGN_CENTER, BaseColor bg = null)
                {
                    return new PdfPCell(new Phrase(text, fSmall))
                    {
                        HorizontalAlignment = align,
                        BackgroundColor = bg ?? BaseColor.WHITE,
                        BorderColor = colBorder,
                        BorderWidth = 0.4f,
                        Padding = 4
                    };
                }

                bool alt = false;
                foreach (var item in items)
                {
                    //var rowBg = alt ? colGray : BaseColor.WHITE;
                    var rowBg = BaseColor.WHITE;
                    alt = !alt;

                    itemTable.AddCell(TD(item.HSN, Element.ALIGN_CENTER, rowBg));
                    itemTable.AddCell(TD(item.Description, Element.ALIGN_LEFT, rowBg));
                    itemTable.AddCell(TD(item.Rate.ToString("0.##"), Element.ALIGN_RIGHT, rowBg));
                    itemTable.AddCell(TD(item.Qty.ToString("0.##"), Element.ALIGN_CENTER, rowBg));
                    itemTable.AddCell(TD(item.SubTotal.ToString("0.##"), Element.ALIGN_RIGHT, rowBg));
                    itemTable.AddCell(TD(item.TaxableValue.ToString("0.##"), Element.ALIGN_RIGHT, rowBg));

                    var gst = company.GST?.Trim();
                    bool isGujaratGST = !string.IsNullOrEmpty(gst) && gst.StartsWith("24");

                    // CGST %
                    var cgstPercent = isGujaratGST ? $"{item.CGST_Percent.ToString("0.##")}%" : "-";
                    // CGST Amount
                    var cgstAmt = isGujaratGST ? (item.CGST_Amount == 0 ? "-" : item.CGST_Amount.ToString("#,##0.##")) : "-";
                    // SGST %
                    var sgstPercent = isGujaratGST ? $"{item.SGST_Percent.ToString("0.##")}%" : "-";
                    // SGST Amount
                    var sgstAmt = isGujaratGST ? (item.SGST_Amount == 0 ? "-" : item.SGST_Amount.ToString("#,##0.##")) : "-";
                    // IGST %
                    var igstPercent = isGujaratGST ? "-" : $"{item.IGST_Percent.ToString("0.##")}%";
                    // IGST Amount
                    var igstAmt = isGujaratGST ? "-" : (item.IGST_Amount == 0 ? "-" : item.IGST_Amount.ToString("#,##0.##"));

                    // Add to table
                    itemTable.AddCell(TD(cgstPercent, Element.ALIGN_CENTER, rowBg));
                    itemTable.AddCell(TD(cgstAmt, Element.ALIGN_RIGHT, rowBg));
                    itemTable.AddCell(TD(sgstPercent, Element.ALIGN_CENTER, rowBg));
                    itemTable.AddCell(TD(sgstAmt, Element.ALIGN_RIGHT, rowBg));
                    itemTable.AddCell(TD(igstPercent, Element.ALIGN_CENTER, rowBg));
                    itemTable.AddCell(TD(igstAmt, Element.ALIGN_CENTER, rowBg));
                }

                doc.Add(itemTable);
                #endregion

                #region TOTALs
                // ══════════════════════════════════════════
                //  5. TOTALS + AMOUNT IN WORDS  (side by side)
                // ══════════════════════════════════════════
                PdfPTable summaryRow = new PdfPTable(2);
                summaryRow.WidthPercentage = 100;
                summaryRow.SetWidths(new float[] { 61.2f, 38.8f });

                // ── Left: Amount in Words + company/bank info ──
                PdfPTable leftInfo = new PdfPTable(1);
                leftInfo.DefaultCell.Border = Rectangle.NO_BORDER;

                var wordsPara = new Paragraph();
                wordsPara.Add(new Chunk("Invoice Value in Words :\n", fSmall7));
                wordsPara.Add(new Chunk(totals.AmountInWords, fSmallB));
                leftInfo.AddCell(new PdfPCell(wordsPara) { 
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_BOTTOM,
                    Padding = 4 
                });

                // ── Right: totals table ──
                PdfPTable totalsTable = new PdfPTable(2);
                totalsTable.WidthPercentage = 100;
                totalsTable.SetWidths(new float[] { 66.7f, 33.3f });

                void AddTotal(string label, string value, bool bold, iTextSharpFont lf, iTextSharpFont vf)
                {
                    //var lf = fBoldRupee; // bold ? fBold : fSmall;
                    //var vf = fBoldRupee; // bold ? fBold : fSmall;
                    totalsTable.AddCell(new PdfPCell(new Phrase(label, lf))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        BorderColor = colBorder,
                        BorderWidth = 0.4f,
                        Padding = 4,
                        BackgroundColor = bold ? colGray : BaseColor.WHITE
                    });
                    totalsTable.AddCell(new PdfPCell(new Phrase(value, vf))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        BorderColor = colBorder,
                        BorderWidth = 0.4f,
                        Padding = 4,
                        BackgroundColor = bold ? colGray : BaseColor.WHITE
                    });
                }

                AddTotal("Subtotal", "₹ " + totals.SubTotal.ToString("N0"), false, fBoldRupee, fBoldRupee);
                AddTotal("Taxable", "₹ " + totals.Taxable.ToString("N0"), false, fBoldRupee, fBoldRupee);
                AddTotal("Total Discount Across All Services", "₹ " + totals.TotalDiscount.ToString("N0"), false, fBoldRupee7, fBoldRupee);
                AddTotal("Total CGST in Rs.", "₹ " + totals.TotalCGST.ToString("N2"), false, fBoldRupee, fBoldRupee);
                AddTotal("Total SGST in Rs.", "₹ " + totals.TotalSGST.ToString("N2"), false, fBoldRupee, fBoldRupee);
                AddTotal("Total IGST in Rs.", "₹ " + totals.TotalIGST.ToString("N2"), false, fBoldRupee, fBoldRupee);
                AddTotal("Invoice Total", "₹ " + totals.InvoiceTotal.ToString("N2"), false, fBoldRupee, fBoldRupee);
                AddTotal("Round off", "₹ " + totals.RoundOff.ToString("N0"), false, fBoldRupee, fBoldRupee);
                AddTotal("Total Adavance Paid", "₹ " + totals.TotalAdvance.ToString("N0"), false, fBoldRupee, fBoldRupee);
                AddTotal("Balance Amount", "₹ " + totals.Balance.ToString("N0"), true, fBoldRupee, fBoldRupee);

                // LEFT SIDE (Amount in Words)
                summaryRow.AddCell(new PdfPCell(leftInfo)
                {
                    Border = Rectangle.BOX,
                    BorderColor = colBorder,
                    BorderWidth = 0.5f,
                    Padding = 6
                });

                // RIGHT SIDE (Totals)
                summaryRow.AddCell(new PdfPCell(totalsTable)
                {
                    Border = Rectangle.BOX,
                    BorderColor = colBorder,
                    BorderWidth = 0.5f,
                    Padding = 0
                });

                doc.Add(summaryRow);

                // ══════════════════════════════════════════
                //  CERTIFICATION ROW (FULL WIDTH)
                // ══════════════════════════════════════════
                PdfPTable certRow = new PdfPTable(1); // ✅ 1 column
                certRow.WidthPercentage = 100;

                certRow.AddCell(new PdfPCell(new Phrase("Certified that the particulars given above are true and correct", fSmallB))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 5
                });
                doc.Add(certRow);
                #endregion

                #region COMPANY & SIGNATURE
                // ══════════════════════════════════════════
                //  6. COMPANY + SIGNATURE ROW (MATCH IMAGE)
                // ══════════════════════════════════════════

                PdfPTable finalRow = new PdfPTable(2);
                finalRow.WidthPercentage = 100;
                finalRow.SetWidths(new float[] { 50f, 50f });

                BaseColor boxBorder = colBorder;

                // ── LEFT SIDE (Company + Bank Details) ──
                PdfPTable leftBlock = new PdfPTable(1);
                leftBlock.WidthPercentage = 100;

                void AddLine(string text, bool bold = false)
                {
                    leftBlock.AddCell(new PdfPCell(new Phrase(text, bold ? fSmallB : fSmall))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 2
                    });
                }

                AddLine("COMPANY: " + company.Name);
                AddLine("GST: " + company.GST);
                AddLine("PAN: " + company.PAN);
                AddLine("State: " + company.State);
                AddLine("State code: " + company.StateCode);
                AddLine("Bank: " + company.BankName);
                AddLine("A/C No.: " + company.AccountNo);
                AddLine("Branch: " + company.Branch);
                AddLine("IFSC: " + company.IFSC);
                AddLine("UPI: " + company.UPI);

                // Wrap LEFT
                finalRow.AddCell(new PdfPCell(leftBlock)
                {
                    Border = Rectangle.BOX,
                    BorderColor = boxBorder,
                    BorderWidth = 0.5f,
                    Padding = 8
                });

                // ── RIGHT SIDE (Signature Section) ──
                PdfPTable rightBlock = new PdfPTable(1);
                rightBlock.WidthPercentage = 100;

                // Top: Company Name (Right aligned)
                rightBlock.AddCell(new PdfPCell(new Phrase("For " + company.Name, fSmall))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingBottom = 25
                });

                // ── Bottom Row (Split Left + Right) ──
                PdfPTable bottomTable = new PdfPTable(2);
                bottomTable.WidthPercentage = 100;
                bottomTable.SetWidths(new float[] { 65f, 35f });

                // Left: Invoice Created By
                bottomTable.AddCell(new PdfPCell(new Phrase("Invoice created by: " + createdBy, fSmall))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_BOTTOM
                });

                // Right: Authorised Signatory
                bottomTable.AddCell(new PdfPCell(new Phrase("Authorised Signatory", fSmall))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_BOTTOM
                });

                // Add bottom table into right block
                rightBlock.AddCell(new PdfPCell(bottomTable)
                {
                    Border = Rectangle.NO_BORDER
                });

                // Wrap RIGHT
                finalRow.AddCell(new PdfPCell(rightBlock)
                {
                    Border = Rectangle.BOX,
                    BorderColor = boxBorder,
                    BorderWidth = 0.5f,
                    Padding = 8
                });

                // Add to document
                doc.Add(finalRow);

                // ══════════════════════════════════════════
                //  CERTIFICATION ROW (FULL WIDTH)
                // ══════════════════════════════════════════
                PdfPTable thankyouRow = new PdfPTable(1); // ✅ 1 column
                thankyouRow.WidthPercentage = 100;

                thankyouRow.AddCell(new PdfPCell(new Phrase("Thank you for your business!", fSmallB))
                {
                    Border = Rectangle.BOX,
                    BorderColorBottom = BaseColor.BLACK,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 5
                });
                doc.Add(thankyouRow);

                #endregion

                #region FOOTER - Compnay Address & Contacts
                // ── Footer address line ──
                doc.Add(new Paragraph(company.Address, fSmall)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 4
                });
                doc.Add(new Paragraph($"Tel: {company.Phone}  Email: {company.Email}", fSmall)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 4,
                    //Border = Rectangle.BOX
                });
                #endregion

                doc.Close();

                // ══════════════════════════════════════════
                //  SAVE & RETURN URL
                // ══════════════════════════════════════════
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = string.Empty; // $"Invoice_{header.InvoiceNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

                if (header.ProformaInvoiceNo > 0)
                {
                    Utils.deleteOldFiles(folder, $"ProformaInvoice*.pdf");
                    fileName = $"ProformaInvoice_{header.ProformaInvoiceNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                }
                else {
                    Utils.deleteOldFiles(folder, $"Invoice_*.pdf");
                    fileName = $"Invoice_{header.InvoiceNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                }

                string filePath = Path.Combine(folder, fileName);

                stream.Position = 0;
                using (var fs = new FileStream(filePath, FileMode.Create))
                    stream.CopyTo(fs);

                return Ok(new
                {
                    status = "Success",
                    message = "Invoice Generated",
                    data = new { FileUrl = $"{domainUrl}/reports/{fileName}" }
                });
            }
            catch (Exception ex)
            {
                return Ok(new { status = "Fail", message = ex.Message });
            }
        }

        [HttpPost]
        [Route("Report_BookingEventForm")]
        public IActionResult Report_BookingEventForm([FromBody] DownloadBookingEventReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dtCompany = new DataTable();
                DataSet ds = new DataSet(); // ✅ IMPORTANT
                DataTable dtPayments = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // COMPANY
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // MAIN SP (MULTIPLE RESULT SETS)
                    cmd = new SqlCommand("Report_GetBookingEventForm", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    new SqlDataAdapter(cmd).Fill(ds);

                    // PAYMENT TRAIL
                    cmd = new SqlCommand("Report_GetBooking_Payments", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@PaymentId", SqlDbType.BigInt).Value = 0;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    new SqlDataAdapter(cmd).Fill(dtPayments);
                }

                if (ds.Tables.Count < 10)
                    throw new Exception("Invalid report data.");

                /* ================= MAP RESULT SETS ================= */

                DataTable dtTitle = ds.Tables[0];
                DataTable dtDetails = ds.Tables[1];
                DataTable dtPlatesHeader = ds.Tables[2];
                DataTable dtPlatesDetails = ds.Tables[3];
                DataTable dtFooter = ds.Tables[4];

                DataTable dtChecklistTitle = ds.Tables[5];
                DataTable dtChecklist = ds.Tables[6];
                DataTable dtEventNotesTitle = ds.Tables[7];
                DataTable dtEventNotes = ds.Tables[8];
                DataTable dtFoodWasteTitle = ds.Tables[9];
                DataTable dtFoodWaste = ds.Tables[10];

                /* ================= PDF INIT ================= */
                // ================= ASSIGN & VALIDATION =================
                #region ASSIGN & VALIDATIONS
                string propertyName = string.Empty, companyName = string.Empty, companyAddress = string.Empty,
                       companyphoneNo = string.Empty, companygstin = "", companyemail = "",
                       logoFile = string.Empty, userName = string.Empty,
                       stateCode = string.Empty, logoFullPath = string.Empty, generatedDate = "";

                bool isCompanyAvailable = dtCompany != null && dtCompany.Rows.Count > 0;
                if (isCompanyAvailable)
                {
                    userName = dtCompany.Rows[0]["UserName"]?.ToString() ?? "";
                    generatedDate = dtCompany.Rows[0]["GeneratedDate"]?.ToString() ?? "";
                }

                if (request.WithHeader ?? false)
                {
                    if (isCompanyAvailable)
                    {
                        propertyName = dtCompany.Rows[0]["PropertyName"]?.ToString() ?? ""; companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
                        companyAddress = dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "";
                        companyphoneNo = dtCompany.Rows[0]["CompanyMobile"]?.ToString() ?? "";
                        logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? "";
                        companygstin = dtCompany.Rows[0]["CompanyGST"]?.ToString();
                        companyemail = dtCompany.Rows[0]["CompanyEmail"]?.ToString();
                        stateCode = dtCompany.Rows[0]["StateCode"]?.ToString() ?? ""; // adjust column name if needed

                        // ── Resolve logo path (same logic as before, just storing the path string) ──
                        if (!string.IsNullOrWhiteSpace(logoFile))
                        {
                            string p1 = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "CompanyInvoice_CompanyLogo_Image", logoFile);
                            if (System.IO.File.Exists(p1))
                                logoFullPath = p1;
                            else
                            {
                                string p2 = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);
                                if (System.IO.File.Exists(p2))
                                    logoFullPath = p2;
                            }
                        }
                    }
                    else
                    {
                        companyName = "[PAGE HEADER] Company Data Not Found";
                    }
                }

                //if (dsReport.Tables.Count < 3)
                //throw new Exception("Report data is incomplete. Required tables are missing.");

                // ================= EXTRACT DATA =================
                //string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                float topMargin = (request.WithHeader ?? false) ? 80f : 36f;

                using var stream = new MemoryStream();

                bool isA4size = true; // default to A4
                Document document = null;
                //if (dsReport.Tables[2].Columns.Count > 7)
                //{
                //    document = new Document(PageSize.A4.Rotate(), 16, 16, topMargin, 50);
                //    isA4size = false;
                //}
                //else
                //{
                document = new Document(PageSize.A4, 16, 16, topMargin, 50);
                //isA4size = true;
                //}

                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                #endregion

                // ── Attach StandardReport header-footer ──
                #region HEADER_FOOTER
                if (request.WithHeader ?? false)
                {
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4(
                            propertyName, companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companyemail,
                            stateCode,
                            logoFullPath,
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            propertyName, companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companyemail,
                            stateCode,
                            logoFullPath,
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }
                else
                {
                    // no company header, but still want page number / generated-by footer
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4(
                            "", "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            "", "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }
                #endregion

                document.Open();

                #region FONT DECLARATIONS
                iTextSharp.text.Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

                iTextSharp.text.Font boldFont9 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                iTextSharp.text.Font normalFont9 = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                iTextSharp.text.Font boldFont7 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
                iTextSharp.text.Font normalFont7 = FontFactory.GetFont(FontFactory.HELVETICA, 8);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 9, iTextSharp.text.Font.NORMAL);

                string fontPath_ArialUnicode = Path.Combine(_env.WebRootPath, "fonts", "arialuni.ttf"); // Segoe UI Symbol
                BaseFont bfSymbol = BaseFont.CreateFont(fontPath_ArialUnicode, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font checkboxFont = new iTextSharp.text.Font(bfSymbol, 10, iTextSharp.text.Font.NORMAL);

                #endregion

                #region PAGE 1
                Paragraph titleForm = new Paragraph("Event Form", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 3,
                    SpacingAfter = 8
                };
                document.Add(titleForm);

                PdfPTable lineBottom = new PdfPTable(1) { WidthPercentage = 100 };
                lineBottom.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, FixedHeight = 1, BackgroundColor = BaseColor.LIGHT_GRAY });
                document.Add(lineBottom);

                /* ================= CLIENT + EVENT DETAILS ================= */
                #region CLIENT + EVENT DETAILS
                PdfPTable details = new PdfPTable(2);
                details.WidthPercentage = 100;
                details.SetWidths(new float[] { 35, 65 }); // better proportion
                details.SpacingBefore = 5;
                details.SpacingAfter = 5;

                foreach (DataColumn col in dtDetails.Columns)
                {
                    if (col.ColumnName.EndsWith("Title"))
                    {
                        string title = dtDetails.Rows[0][col]?.ToString() ?? "-";
                        string value = dtDetails.Rows[0][col.ColumnName.Replace("Title", "")]?.ToString() ?? "-";

                        // 🔹 TITLE CELL
                        details.AddCell(new PdfPCell(new Phrase(title, boldFont9))
                        {
                            BackgroundColor = new BaseColor(245, 245, 245), // light gray
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Padding = 6,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        // 🔹 VALUE CELL
                        details.AddCell(new PdfPCell(new Phrase(value, normalFont9))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            Padding = 6,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });
                    }
                }
                document.Add(details);
                #endregion

                /* ================= PAYMENT TRAIL ================= */
                #region PAYMENTS TRIAL
                if (dtPayments.Rows.Count > 0)
                {
                    document.Add(new Paragraph("\n"));

                    // HISTORY TITLE
                    PdfPTable title2 = new PdfPTable(1);
                    title2.WidthPercentage = 100;

                    PdfPCell titleCell2 = new PdfPCell(new Phrase("ALL PAYMENT HISTORY",boldFont9));
                    titleCell2.BackgroundColor = new BaseColor(240, 240, 240);
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
                        payTbl.AddCell(new PdfPCell(new Phrase(t, boldFont9))
                        {
                            BackgroundColor = new BaseColor(240, 240, 240),
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            Padding = 4f,
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

                        payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentId"]?.ToString() ?? "-", normalFont9))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            Padding = 4f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentDateTimeFormat"]?.ToString() ?? "-", normalFont9))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentModeName"]?.ToString() ?? "-", normalFont9))
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

                        payTbl.AddCell(new PdfPCell(new Phrase(pr["PaymentNotes"]?.ToString() ?? "-", normalFont9))
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
                        Colspan = 3,
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
                #endregion

                /* ================= PLATES ================= */
                #region PLATES SUMMARY
                // 🔹 Filter only display columns (exclude signatures if needed)
                var displayColumns = dtPlatesHeader.Columns
                    .Cast<DataColumn>()
                    //.Where(c => c.ColumnName != "AuthSign" && c.ColumnName != "ClientSign")
                    .ToList();

                // 🔹 TITLE
                PdfPTable plateTitle = new PdfPTable(1);
                plateTitle.WidthPercentage = 100;
                plateTitle.SpacingBefore = 10f;

                plateTitle.AddCell(new PdfPCell(new Phrase("PLATES SUMMARY", boldFont9))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY
                });

                document.Add(plateTitle);

                // 🔹 TABLE (dynamic column count)
                PdfPTable plateTbl = new PdfPTable(dtPlatesHeader.Columns.Count);
                plateTbl.WidthPercentage = 100;
                plateTbl.SpacingBefore = 0;
                plateTbl.HeaderRows = 1;

                // 🔹 HEADER ROW (dynamic)
                foreach (var col in displayColumns)
                {
                    string headerText = dtPlatesHeader.Rows[0][col.ColumnName]?.ToString() ?? col.ColumnName;

                    plateTbl.AddCell(new PdfPCell(new Phrase(headerText, boldFont9))
                    {
                        BackgroundColor = new BaseColor(230, 230, 230),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Border = Rectangle.BOX,
                        BorderWidth = 0.2f,
                        Padding = 4f,
                        BorderColor = BaseColor.LIGHT_GRAY
                    });
                }

                // 🔹 DATA ROWS
                foreach (DataRow r in dtPlatesDetails.Rows)
                {
                    string style = r["FontStyle"]?.ToString();
                    var font = style == "bold" ? boldFont9 : normalFont9;

                    foreach (var col in displayColumns)
                    {
                        string? value = "";

                        // Mapping logic
                        if (col.ColumnName == "Title")
                            value = r["PlatesTitle"]?.ToString();

                        else if (col.ColumnName == "TotalPlates")
                            value = r["PlatesValue"]?.ToString();

                        else if (col.ColumnName == "AuthSign")
                            value = r["AuthSign"]?.ToString();

                        else if (col.ColumnName == "ClientSign")
                            value = r["ClientSign"]?.ToString();

                        plateTbl.AddCell(new PdfPCell(new Phrase(value, font))
                        {
                            HorizontalAlignment = col.ColumnName == "Title"
                                ? Element.ALIGN_LEFT
                                : Element.ALIGN_CENTER,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            Padding = 6f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });
                    }
                }

                document.Add(plateTbl);
                #endregion

                /* ================= FOOTER - sign ================= */
                #region FOOTER - SIGN & FEEDBACK
                /* ================= WRAPPER TABLE (50 : 50) ================= */

                PdfPTable wrapperTbl = new PdfPTable(2);
                wrapperTbl.WidthPercentage = 100;
                wrapperTbl.SetWidths(new float[] { 50, 50 });
                wrapperTbl.SpacingBefore = 15;

                /* ================= LEFT SIDE : FOOTER ================= */
                PdfPTable footerTbl = new PdfPTable(2);
                footerTbl.WidthPercentage = 100;
                footerTbl.SetWidths(new float[] { 30, 70 });

                // 🔹 Helper
                void AddFooterRow(string title, string value)
                {
                    footerTbl.AddCell(new PdfPCell(new Phrase(title, boldFont7))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 10
                    });

                    footerTbl.AddCell(new PdfPCell(new Phrase(value, normalFont7))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 10
                    });
                }

                // 🔹 DATA
                DataRow f = dtFooter.Rows[0];

                AddFooterRow(f["FeedbackTitle"]?.ToString(), "_______________________________");
                AddFooterRow(f["Comments"]?.ToString(), "_______________________________");
                AddFooterRow(f["GuestSignature"]?.ToString(), "_______________________________");

                // 🔹 Add to wrapper (LEFT)
                wrapperTbl.AddCell(new PdfPCell(footerTbl)
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingRight = 10
                });


                /* ================= RIGHT SIDE : FEEDBACK ================= */
                PdfPTable feedbackTbl = new PdfPTable(1);
                feedbackTbl.WidthPercentage = 100;
                // 🔹 Title
                feedbackTbl.AddCell(new PdfPCell(new Phrase("FEEDBACK:", boldFont7))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 8
                });

                // 🔹 Helper
                void AddStarRow(string label)
                {
                    // 🔹 INNER TABLE (2 columns: label | stars)
                    PdfPTable rowTbl = new PdfPTable(2);
                    rowTbl.WidthPercentage = 100;
                    rowTbl.SetWidths(new float[] { 60, 40 });

                    // LEFT → LABEL
                    rowTbl.AddCell(new PdfPCell(new Phrase(label, boldFont7))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 4
                    });

                    // RIGHT → STARS (right aligned)
                    Phrase starPhrase = new Phrase();
                    for (int i = 0; i < 5; i++)
                    {
                        starPhrase.Add(new Chunk("☆   ", checkboxFont));
                    }

                    rowTbl.AddCell(new PdfPCell(starPhrase)
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        PaddingBottom = 4
                    });

                    // 🔹 WRAP WITH BOTTOM BORDER
                    feedbackTbl.AddCell(new PdfPCell(rowTbl)
                    {
                        Border = Rectangle.BOTTOM_BORDER,
                        BorderColorBottom = BaseColor.LIGHT_GRAY,
                        BorderWidthBottom = 0.5f,
                        PaddingBottom = 6
                    });
                }

                // 🔹 Rows
                AddStarRow("Food Quality");
                AddStarRow("Service");
                AddStarRow("Ambience");
                AddStarRow("Management");
                AddStarRow("Overall Experience");

                // 🔹 Add to wrapper
                wrapperTbl.AddCell(new PdfPCell(feedbackTbl)
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingLeft = 10
                });

                /* ================= ADD TO DOCUMENT ================= */
                document.Add(wrapperTbl);
                #endregion

                #endregion

                #region PAGE 2
                document.NewPage();

                /* ================= CHECKLIST ================= */
                #region EVENT DAY SMART CHECKLIST 1
                // 🔹 TABLE 1 : TITLE
                PdfPTable checklistTitleTbl = new PdfPTable(1);
                checklistTitleTbl.WidthPercentage = 100;
                checklistTitleTbl.AddCell(new PdfPCell(new Phrase(dtChecklistTitle.Rows[0][0].ToString(), boldFont7))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY
                });
                document.Add(checklistTitleTbl);

                // 🔹 GROUP DATA
                var groupedChecklist = dtChecklist.AsEnumerable().GroupBy(r => r["Title"].ToString());

                // 🔹 TABLE 2 : MAIN CHECKLIST TABLE
                PdfPTable checklistTbl = new PdfPTable(2);
                checklistTbl.WidthPercentage = 100;
                checklistTbl.SetWidths(new float[] { 30, 70 });
                checklistTbl.SpacingBefore = 0;

                // 🔹 LOOP GROUPS
                foreach (var grp in groupedChecklist)
                {
                    // LEFT COLUMN (KEY)
                    checklistTbl.AddCell(new PdfPCell(new Phrase(grp.Key, boldFont7))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 2,
                        Border = Rectangle.BOX,
                        BorderWidth = 0.2f,
                        BorderColor = BaseColor.LIGHT_GRAY
                    });

                    // RIGHT COLUMN (MULTIPLE CHECKBOXES IN SAME CELL)
                    PdfPCell optionsCell = new PdfPCell()
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 2,
                        Border = Rectangle.BOX,
                        BorderWidth = 0.2f,
                        BorderColor = BaseColor.LIGHT_GRAY
                    };

                    // 🔹 Create inner flow (same line wrapping)
                    Phrase optionsPhrase = new Phrase();
                    foreach (var item in grp)
                    {
                        optionsPhrase.Add(new Chunk("☐ ", checkboxFont)); // checkbox
                        optionsPhrase.Add(new Chunk(item["OptionText"] + "    ", normalFont7));
                    }
                    optionsCell.AddElement(optionsPhrase);
                    checklistTbl.AddCell(optionsCell);
                }

                document.Add(checklistTbl);
                #endregion

                /* ================= INTERNAL EVENT NOTES ================= */
                #region INTERNAL EVENT DAY NOTES

                /* ================= TITLE ================= */
                // 🔹 TITLE
                PdfPTable internalTitleTbl = new PdfPTable(1);
                internalTitleTbl.WidthPercentage = 100;
                internalTitleTbl.SpacingBefore = 5f;
                internalTitleTbl.AddCell(new PdfPCell(new Phrase(dtEventNotesTitle.Rows[0][0].ToString(), boldFont7))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY
                });
                document.Add(internalTitleTbl);

                /* ================= EVENT NOTES (2 COLUMN LAYOUT) ================= */
                // 🔹 GROUP DATA
                var groupedNotes = dtEventNotes.AsEnumerable()
                    .GroupBy(r => r["Section"].ToString())
                    .ToList();

                // 🔹 OUTER TABLE (2 columns)
                PdfPTable outerTbl = new PdfPTable(2);
                outerTbl.WidthPercentage = 100;
                outerTbl.SetWidths(new float[] { 50, 50 });
                outerTbl.SpacingBefore = 5;

                int colIndex = 0;

                foreach (var sec in groupedNotes)
                {
                    // 🔹 INNER CONTENT TABLE (1 column)
                    PdfPTable inner = new PdfPTable(1);
                    inner.WidthPercentage = 100;

                    // SECTION HEADER
                    inner.AddCell(new PdfPCell(new Phrase("* " + sec.Key, boldFont7))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 3
                    });

                    // ITEMS
                    foreach (var item in sec)
                    {
                        string text = item["Item"].ToString();
                        if (text == "Issue")
                        {
                            Phrase phrase = new Phrase();

                            phrase.Add(new Chunk("  ", normalFont7));          // spacing
                            phrase.Add(new Chunk("☐ ", checkboxFont));         // ✅ checkbox font
                            phrase.Add(new Chunk("Issue: ____________________", normalFont7)); // normal text

                            inner.AddCell(new PdfPCell(phrase)
                            {
                                Border = Rectangle.NO_BORDER,
                                PaddingBottom = 5
                            });
                        }
                        else
                        {
                            Phrase phrase = new Phrase();

                            phrase.Add(new Chunk("  ", normalFont7));          // spacing
                            phrase.Add(new Chunk("☐ ", checkboxFont));         // ✅ checkbox font
                            phrase.Add(new Chunk(text, normalFont7));          // normal text

                            inner.AddCell(new PdfPCell(phrase)
                            {
                                Border = Rectangle.NO_BORDER,
                                PaddingBottom = 2
                            });
                        }
                    }

                    // DIVIDER LINE
                    inner.AddCell(new PdfPCell()
                    {
                        FixedHeight = 1,
                        BackgroundColor = BaseColor.LIGHT_GRAY,
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10
                    });

                    // 🔹 ADD TO OUTER TABLE
                    outerTbl.AddCell(new PdfPCell(inner)
                    {
                        Border = Rectangle.NO_BORDER,
                        Padding = 5
                    });

                    colIndex++;

                    // 🔹 HANDLE ODD COUNT (fill empty cell)
                    if (colIndex % 2 != 0 && sec.Equals(groupedNotes.Last()))
                    {
                        outerTbl.AddCell(new PdfPCell(new Phrase(""))
                        {
                            Border = Rectangle.NO_BORDER
                        });
                    }
                }

                // 🔹 ADD TO DOCUMENT
                document.Add(outerTbl);
                #endregion

                /* ================= FOOD WASTE ================= */
                #region FOOD WASTE
                // 🔹 TITLE
                PdfPTable foodTitleTbl = new PdfPTable(1);
                foodTitleTbl.WidthPercentage = 100;
                foodTitleTbl.SpacingBefore = 5f;

                foodTitleTbl.AddCell(new PdfPCell(new Phrase(dtFoodWasteTitle.Rows[0][0].ToString(), boldFont7))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY
                });
                document.Add(foodTitleTbl);

                // 🔹 MAIN TABLE (5 columns)
                PdfPTable wasteTbl = new PdfPTable(3);
                wasteTbl.WidthPercentage = 100;
                wasteTbl.SetWidths(new float[] { 6, 74, 20});
                wasteTbl.SpacingBefore = 0;
                wasteTbl.HeaderRows = 1;

                // 🔹 HEADER FUNCTION
                void FH(string text)
                {
                    wasteTbl.AddCell(new PdfPCell(new Phrase(text, boldFont7))
                    {
                        BackgroundColor = new BaseColor(240, 240, 240),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5,
                        Border = Rectangle.BOX,
                        BorderWidth = 0.2f,
                        BorderColor = BaseColor.LIGHT_GRAY
                    });
                }

                // 🔹 HEADER ROW
                FH("Sr No.");
                FH("Items");
                FH("Weight (Kg)");

                int srno = 1;

                // 🔥 CHECK IF EMPTY
                if (dtFoodWaste.Rows.Count == 0)
                {
                    // 🔹 Empty Row (Full Width Message)
                    wasteTbl.AddCell(new PdfPCell(new Phrase("No food items found", normalFont7))
                    {
                        Colspan = 3, // 🔥 merge all columns
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 6,
                        Border = Rectangle.BOX,
                        BorderWidth = 0.2f,
                        BorderColor = BaseColor.LIGHT_GRAY
                    });
                }
                else
                {
                    foreach (DataRow r in dtFoodWaste.Rows)
                    {
                        // Sr No
                        wasteTbl.AddCell(new PdfPCell(new Phrase(srno.ToString(), normalFont7))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 4,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        // Item Name
                        wasteTbl.AddCell(new PdfPCell(new Phrase(r["ItemName"]?.ToString() ?? "-", normalFont7))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Padding = 4,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        // Weight
                        wasteTbl.AddCell(new PdfPCell(new Phrase(r["FoodWasteWeight"]?.ToString() ?? "0", normalFont7))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            Padding = 4,
                            Border = Rectangle.BOX,
                            BorderWidth = 0.2f,
                            BorderColor = BaseColor.LIGHT_GRAY
                        });

                        srno++;
                    }
                }

                document.Add(wasteTbl);
                #endregion

                #endregion

                document.Close();

                /* ================= SAVE ================= */
                #region SAVE FILE
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                string fileName = $"EventForm_BK{request.BookingId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fs);
                }
                #endregion

                response.status = "Success";
                response.message = "Event Form Generated";
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
        [Route("Report_FunctionProspectusReport")]
        public IActionResult Report_FunctionProspectusReport([FromBody] PackageReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dtCompany = new DataTable();
                //DataSet dsReport = new DataSet();

                BookingReportModel report = new BookingReportModel();
                report.PackageItems = new List<PackageItem>();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // Company Header
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // Report Data
                    cmd = new SqlCommand("Report_GetBooking_FunctionProspectusReport", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.Int).Value = (object?)request.BookingId ?? DBNull.Value;
                    //new SqlDataAdapter(cmd).Fill(dsReport);

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

                // ================= ASSIGN & VALIDATION =================
                #region ASSIGN & VALIDATIONS
                string propertyName = string.Empty, companyName = string.Empty, companyAddress = string.Empty,
                       companyphoneNo = string.Empty, companygstin = "", companyemail = "",
                       logoFile = string.Empty, userName = string.Empty,
                       stateCode = string.Empty, logoFullPath = string.Empty, generatedDate = "";

                bool isCompanyAvailable = dtCompany != null && dtCompany.Rows.Count > 0;
                if (isCompanyAvailable)
                {
                    userName = dtCompany.Rows[0]["UserName"]?.ToString() ?? "";
                    generatedDate = dtCompany.Rows[0]["GeneratedDate"]?.ToString() ?? "";
                }

                if (request.WithHeader ?? false)
                {
                    if (isCompanyAvailable)
                    {
                        propertyName = dtCompany.Rows[0]["PropertyName"]?.ToString() ?? ""; companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
                        companyAddress = dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "";
                        companyphoneNo = dtCompany.Rows[0]["CompanyMobile"]?.ToString() ?? "";
                        logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? "";
                        companygstin = dtCompany.Rows[0]["CompanyGST"]?.ToString();
                        companyemail = dtCompany.Rows[0]["CompanyEmail"]?.ToString();
                        stateCode = dtCompany.Rows[0]["StateCode"]?.ToString() ?? ""; // adjust column name if needed

                        // ── Resolve logo path (same logic as before, just storing the path string) ──
                        if (!string.IsNullOrWhiteSpace(logoFile))
                        {
                            string p1 = Path.Combine(_env.WebRootPath, "uploads", "org", domain, "CompanyInvoice_CompanyLogo_Image", logoFile);
                            if (System.IO.File.Exists(p1))
                                logoFullPath = p1;
                            else
                            {
                                string p2 = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);
                                if (System.IO.File.Exists(p2))
                                    logoFullPath = p2;
                            }
                        }
                    }
                    else
                    {
                        companyName = "[PAGE HEADER] Company Data Not Found";
                    }
                }

                //if (dsReport.Tables.Count < 3)
                    //throw new Exception("Report data is incomplete. Required tables are missing.");

                // ================= EXTRACT DATA =================
                //string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                float topMargin = (request.WithHeader ?? false) ? 80f : 36f;

                using var stream = new MemoryStream();

                bool isA4size = true; // default to A4
                Document document = null;
                //if (dsReport.Tables[2].Columns.Count > 7)
                //{
                //    document = new Document(PageSize.A4.Rotate(), 16, 16, topMargin, 50);
                //    isA4size = false;
                //}
                //else
                //{
                    document = new Document(PageSize.A4, 16, 16, topMargin, 50);
                    //isA4size = true;
                //}

                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                #endregion

                // ── Attach StandardReport header-footer ──
                #region HEADER_FOOTER
                if (request.WithHeader ?? false)
                {
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4(
                            propertyName, companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companyemail,
                            stateCode,
                            logoFullPath,
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            propertyName, companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companyemail,
                            stateCode,
                            logoFullPath,
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }
                else
                {
                    // no company header, but still want page number / generated-by footer
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4(
                            "", "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            "", "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }
                #endregion

                document.Open();

                // ===== TITLE LINES =====
                //PdfPTable lineTop = new PdfPTable(1) { WidthPercentage = 100 };
                //lineTop.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, FixedHeight = 1, BackgroundColor = BaseColor.BLACK });
                //document.Add(lineTop);

                Paragraph title = new Paragraph("Function Prospectus", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 3,
                    SpacingAfter = 8
                };
                document.Add(title);

                PdfPTable lineBottom = new PdfPTable(1) { WidthPercentage = 100 };
                lineBottom.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, FixedHeight = 1, BackgroundColor = BaseColor.LIGHT_GRAY });
                document.Add(lineBottom);

                // ===== EVENT DETAILS =====
                iTextSharpFont labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                iTextSharpFont labelFontnormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                iTextSharpFont valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
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

                AddLeftRow("Booking Ref.", '#' + report.BookingRef);
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
                    p.Add(new Chunk(label + " : ", labelFont));
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
                document.Add(detailsTable);
                document.Add(new Paragraph("\n"));

                // ===== PRIMARY FOOD PACKAGE =====
                iTextSharpFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                iTextSharpFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                PdfPTable packageTable = new PdfPTable(1) { WidthPercentage = 100 };
                packageTable.AddCell(new PdfPCell(new Phrase($"PRIMARY FOOD PACKAGE : {report.PrimaryPackage}@ Rs.{report.RatePerPlate:N0} /Per Plate", boldFont))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingTop = 6,
                    PaddingBottom = 6,
                    HorizontalAlignment = Element.ALIGN_LEFT
                });
                document.Add(packageTable);

                // ===== SUMMARY SECTION =====
                // ===== LEFT + RIGHT SECTION (Details + Summary) =====

                iTextSharpFont labelFontLeft = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                iTextSharpFont labelFontBoldLeft = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.BLACK);

                iTextSharpFont summaryLabelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                iTextSharpFont summaryValueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);

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

                document.Add(mainTable);
                document.Add(new Paragraph("\n"));

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
                document.Add(signTable);
                document.Close();

                #region SAVE_FILE
                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "FunctionProspectus_Report_*.pdf");

                string fileName = $"FunctionProspectus_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);
                #endregion

                response.status = "Success";
                response.message = "Function Prospectus Report Generated";
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
    }
}

public class PageBorder : PdfPageEventHelper
{
    public override void OnEndPage(PdfWriter writer, Document document)
    {
        PdfContentByte cb = writer.DirectContent;

        Rectangle rect = new Rectangle(
            document.LeftMargin,
            document.BottomMargin,
            document.PageSize.Width - document.RightMargin,
            document.PageSize.Height - document.TopMargin
        );

        rect.Border = Rectangle.BOX;
        rect.BorderWidth = 0.5f; // thickness
        rect.BorderColor = BaseColor.BLACK;

        cb.Rectangle(rect);
    }
}

public class PdfPageEvents : PdfPageEventHelper
{
    private readonly string _username;

    public PdfPageEvents(string username)
    {
        _username = username;
    }

    iTextSharpFont footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
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
