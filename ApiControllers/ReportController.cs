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
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Office2010.Excel;
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
                // ══════════════════════════════════════════
                //  STATIC DATA  (replace with SP calls later)
                // ══════════════════════════════════════════
                var company = new CompanyInfo
                {
                    Name = "Shreenidhi Enterprise",
                    Address = "1st Floor, Crosswaymall, Above Croma, Near Ramchowk, Ghoddod Road, Surat",
                    Phone = "Tel: 8799546155",
                    Email = "ramayanarestroandbanquet@gmail.com",
                    GST = "24AEPFS5945D1ZR",
                    PAN = "AEPFS5945D",
                    State = "Gujarat",
                    StateCode = "24",
                    BankName = "SHREENIDHI ENTERPRISE",
                    AccountNo = "777705007497",
                    Branch = "GHODDOD ROAD",
                    IFSC = "ICIC0001388",
                    UPI = "8799546155@okbizaxis",
                    Logo = "banquatlogo.png"
                };

                var header = new InvoiceHeader
                {
                    InvoiceNo = "459",
                    Date = "12-Jul-2025",
                    BookingRef = "83894",
                    EventDate = "12-Jul-2025",
                    EventType = "Meeting",
                    Hall = "Laxman"
                };

                var billTo = new BillToInfo
                {
                    Name = "GURUKUL TRAINING CONSULTANCY SERVICES PVT LTD",
                    Website = "www.gurukultraining.in",
                    Address = "OFFICE NO 1018 3RD FLOOR WEST FIELD SHOWROOM NEAR TANISHQ\nSHOWROOM GHOD DOD ROAD SURAT GUJRAT INDIA",
                    Phone = "Phone: 7383797600",
                    GSTIN = "GSTIN: 24AAKCG4738L1Z8",
                    PAN = "PAN: AAKCG4738L"
                };

                var items = new List<InvoiceItem>
                {
                    new InvoiceItem { HSN="1", Description = "Banquet Rent",        Rate = 1800,  Qty = 4,  SubTotal = 7200,  TaxableValue = 7200,  CgstPct = 9,   CgstAmt = 648,   SgstPct = 9,   SgstAmt = 648, IgstPct = 0,   IgstAmt = 0   },
                    new InvoiceItem { HSN="2", Description = "Customised Package",  Rate = 55,    Qty = 30, SubTotal = 1650,  TaxableValue = 1650,  CgstPct = 2.5m, CgstAmt = 41.25m, SgstPct = 2.5m, SgstAmt = 41.25m, IgstPct = 0,   IgstAmt = 0 },
                    new InvoiceItem { HSN="", Description = "Sound System",        Rate = 2000,  Qty = 1,  SubTotal = 2000,  TaxableValue = 2000,  CgstPct = 9,   CgstAmt = 180,   SgstPct = 9,   SgstAmt = 180, IgstPct = 0,   IgstAmt = 0   },
                };

                var totals = new InvoiceTotals
                {
                    SubTotal = 10850,
                    Taxable = 10850,
                    TotalDiscount = 0,
                    TotalCGST = 869.25m,
                    TotalSGST = 869.25m,
                    InvoiceTotal = 12588.50m,
                    RoundOff = 12589,
                    TotalAdvance = 0,
                    Balance = 12589,
                    AmountInWords = "Rupees Twelve Thousand Five Hundred Eighty Nine Only"
                };

                string createdBy = "Mr.Jitendra Saudiyal";

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
                leftTable.AddCell(new PdfPCell(new Phrase("Tax Invoice", fTitle))
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

                AddMeta("Tax Invoice", header.InvoiceNo);
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
                string logoFile = company.Logo; 
                PdfPCell logoCell;

                string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);

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
                    itemTable.AddCell(TD(item.Qty.ToString(), Element.ALIGN_CENTER, rowBg));
                    itemTable.AddCell(TD(item.SubTotal.ToString("0.##"), Element.ALIGN_RIGHT, rowBg));
                    itemTable.AddCell(TD(item.TaxableValue.ToString("0.##"), Element.ALIGN_RIGHT, rowBg));

                    var gst = company.GST?.Trim();
                    bool isGujaratGST = !string.IsNullOrEmpty(gst) && gst.StartsWith("24");

                    // CGST %
                    var cgstPercent = isGujaratGST ? $"{item.CgstPct}%" : "-";
                    // CGST Amount
                    var cgstAmt = isGujaratGST ? (item.CgstAmt == 0 ? "-" : item.CgstAmt.ToString("#,##0.##")) : "-";
                    // SGST %
                    var sgstPercent = isGujaratGST ? $"{item.SgstPct}%" : "-";
                    // SGST Amount
                    var sgstAmt = isGujaratGST ? (item.SgstAmt == 0 ? "-" : item.SgstAmt.ToString("#,##0.##")) : "-";
                    // IGST %
                    var igstPercent = isGujaratGST ? "-" : $"{item.IgstPct}%";
                    // IGST Amount
                    var igstAmt = isGujaratGST ? "-" : (item.IgstAmt == 0 ? "-" : item.IgstAmt.ToString("#,##0.##"));

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
                Directory.CreateDirectory(folder);

                string fileName = $"Invoice_{header.InvoiceNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadCustomerInvoiceReport2")]
        public IActionResult DownloadCustomerInvoiceReport2([FromBody] CustomerInvoiceReportRequest request)
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

                iTextSharpFont thFont = new iTextSharpFont(iTextSharpFont.FontFamily.HELVETICA, 10, iTextSharpFont.BOLD, BaseColor.WHITE);
                iTextSharpFont tdFont = new iTextSharpFont(iTextSharpFont.FontFamily.HELVETICA, 9);
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
                iTextSharpFont titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                iTextSharpFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                iTextSharpFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
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
        [Route("Report_FunctionProspectuseReport")]
        public IActionResult Report_FunctionProspectuseReport([FromBody] PackageReportRequest request)
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
                    data = new { FileUrl = pdfPath }
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
            iTextSharpFont headerFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            iTextSharpFont headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
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
            iTextSharpFont labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            iTextSharpFont labelFontnormal = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            iTextSharpFont valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
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
            iTextSharpFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
            iTextSharpFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
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

            iTextSharpFont labelFontLeft = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
            iTextSharpFont labelFontBoldLeft = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.BLACK);

            iTextSharpFont summaryLabelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
            iTextSharpFont summaryValueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);

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

                iTextSharpFont reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);

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

                iTextSharpFont thFont = new iTextSharpFont(iTextSharpFont.FontFamily.HELVETICA, 10, iTextSharpFont.BOLD, BaseColor.WHITE);
                iTextSharpFont tdFont = new iTextSharpFont(iTextSharpFont.FontFamily.HELVETICA, 9);
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
