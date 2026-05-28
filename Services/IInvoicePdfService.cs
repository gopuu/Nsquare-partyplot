using ERPAdmin.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;

using ERPAdmin.Helper;
using ERPAdmin.PDF;
using ERPAdmin.Services;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
//using Org.BouncyCastle.Asn1.Ocsp;
//using SixLabors.Fonts;
using System.Data;
using System.Data.SqlClient;
using iTextChunk = iTextSharp.text.Chunk;
using iTextDocument = iTextSharp.text.Document;
using iTextFont = iTextSharp.text.Font;
using iTextPageSize = iTextSharp.text.PageSize;
using iTextParagraph = iTextSharp.text.Paragraph;
using iTextPhrase = iTextSharp.text.Phrase;

namespace ERPAdmin.Services
{
    public interface IInvoicePdfService
    {
        Task<string> GenerateCustomerInvoicePdf(
            CustomerInvoiceReportRequest request,
            string connString,
            string domain,
            string domainUrl
        );
    }

    public class InvoicePdfService : IInvoicePdfService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public InvoicePdfService(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        public async Task<string> GenerateCustomerInvoicePdf(CustomerInvoiceReportRequest request, string connString, string domain,string domainUrl)
        {
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
                cmd = new SqlCommand("Report_GetCustomerInvoiceById", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BookingId", request.BookingId);
                cmd.Parameters.AddWithValue("@InvoiceNo", request.InvoiceNo);
                cmd.Parameters.AddWithValue("@GSTInvoiceNo", request.GSTInvoiceNo);
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
                    InvoiceNo = row["InvoiceNo"].ToString() ?? null,
                    //GSTInvoiceNo = row["GSTInvoiceNo"].ToString() ?? "",
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
                    PAN = row["PAN"]?.ToString() ?? "",
                    IsInterState = row["IsInterState"] == DBNull.Value
                        ? (bool?)null
                        : Convert.ToBoolean(row["IsInterState"])
                };

                // 🔹 Totals
                totals = new InvoiceTotals
                {
                    SubTotal = row.Field<decimal>("SubTotal"),
                    Taxable = row.Field<decimal>("Taxable"),
                    TotalDiscount = row.Field<decimal>("TotalDiscount"),
                    TotalCGST = row.Field<decimal>("TotalCGST"),
                    TotalSGST = row.Field<decimal>("TotalSGST"),
                    TotalIGST = row.Field<decimal>("TotalIGST"),
                    InvoiceTotal = row.Field<decimal>("InvoiceTotal"),
                    RoundOff = row.Field<decimal>("RoundOff"),
                    TotalAdvance = row.Field<decimal>("TotalAdvance"),
                    Balance = row.Field<decimal>("Balance"),
                    AmountInWords = row["AmountInWords"]?.ToString() ?? ""
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
            var doc = new iTextSharp.text.Document(iTextPageSize.A4, 28, 28, 36, 50);
            var writer = PdfWriter.GetInstance(doc, stream);
            writer.CloseStream = false;

            // 👉 Add this line
            writer.PageEvent = new PageBorder();

            doc.Open();

            // ── Fonts ──────────────────────────────────
            var fNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextFont.NORMAL);
            var fBold = FontFactory.GetFont(FontFactory.HELVETICA, 9, iTextFont.BOLD);
            var fSmall7 = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextFont.NORMAL);
            var fSmall7Bold = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextFont.BOLD);
            var fSmall = FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextFont.NORMAL);
            var fSmallB = FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextFont.BOLD);
            var fTitle = FontFactory.GetFont(FontFactory.HELVETICA, 14, iTextFont.BOLD);
            var fHeader = FontFactory.GetFont(FontFactory.HELVETICA, 11, iTextFont.BOLD);
            var fWhite = FontFactory.GetFont(FontFactory.HELVETICA, 8, iTextFont.BOLD, BaseColor.WHITE);
            //var fGrayHdr = FontFactory.GetFont(FontFactory.HELVETICA, 7, iTextFont.BOLD, BaseColor.WHITE);

            string fontPathB = Path.Combine(_env.WebRootPath, "fonts", "arialbd.ttf");
            BaseFont bfB = BaseFont.CreateFont(fontPathB, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            iTextFont fBoldRupee = new iTextFont(bfB, 8, iTextFont.NORMAL);
            iTextFont fBoldRupee7 = new iTextFont(bfB, 7, iTextFont.NORMAL);

            string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
            BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            iTextFont fRupee7 = new iTextFont(bf, 7, iTextFont.NORMAL);

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

            AddMeta(titleText, header.ProformaInvoiceNo > 0 ? header.ProformaInvoiceNo.ToString() : header.InvoiceNo);
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
            billCell.AddElement(new iTextSharp.text.Paragraph("Bill To:", fSmall));

            // Company Name (Bold & Bigger)
            billCell.AddElement(new iTextSharp.text.Paragraph(billTo.Name.ToUpper(), fBold));
            billCell.AddElement(new iTextSharp.text.Paragraph(billTo.Website, fSmall7));
            billCell.AddElement(new iTextSharp.text.Paragraph(billTo.Address, fSmall));
            billCell.AddElement(new iTextSharp.text.Paragraph("Phone: " + billTo.Phone, fSmall));
            billCell.AddElement(new iTextSharp.text.Paragraph("GSTIN: " + billTo.GSTIN, fSmall));
            billCell.AddElement(new iTextSharp.text.Paragraph("PAN: " + billTo.PAN, fSmall));
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
            PdfPCell TD(string text, int align = Element.ALIGN_CENTER, BaseColor bg = null, bool isRupeeFormat = false)
            {
                return new PdfPCell(new Phrase(text, isRupeeFormat ? fRupee7 : fSmall7))
                {
                    HorizontalAlignment = align,
                    BackgroundColor = bg ?? BaseColor.WHITE,
                    BorderColor = colBorder,
                    BorderWidth = 0.4f,
                    Padding = 2
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
                itemTable.AddCell(TD("₹ " + item.Rate.ToString("0.##"), Element.ALIGN_RIGHT, rowBg, true));
                itemTable.AddCell(TD(item.Qty.ToString("0.##"), Element.ALIGN_CENTER, rowBg));
                itemTable.AddCell(TD("₹ " + item.SubTotal.ToString("0.##"), Element.ALIGN_RIGHT, rowBg, true));
                itemTable.AddCell(TD("₹ " + item.TaxableValue.ToString("0.##"), Element.ALIGN_RIGHT, rowBg, true));

                //var gst = company.GST?.Trim();
                // Use interstate flag directly
                bool isInterState = billTo.IsInterState ?? false;

                // CGST %
                var cgstPercent = !isInterState
                    ? $"{item.CGST_Percent:0.##}%"
                    : "-";

                // CGST Amount
                var cgstAmt = !isInterState
                    ? (item.CGST_Amount == 0 ? "-" : "₹ " + item.CGST_Amount.ToString("#,##0.##"))
                    : "-";

                // SGST %
                var sgstPercent = !isInterState
                    ? $"{item.SGST_Percent:0.##}%"
                    : "-";

                // SGST Amount
                var sgstAmt = !isInterState
                    ? (item.SGST_Amount == 0 ? "-" : "₹ " + item.SGST_Amount.ToString("#,##0.##"))
                    : "-";

                // IGST %
                var igstPercent = isInterState
                    ? $"{item.IGST_Percent:0.##}%"
                    : "-";

                // IGST Amount
                var igstAmt = isInterState
                    ? (item.IGST_Amount == 0 ? "-" : "₹ " + item.IGST_Amount.ToString("#,##0.##"))
                    : "-";

                // Add to table
                itemTable.AddCell(TD(cgstPercent, Element.ALIGN_CENTER, rowBg));
                itemTable.AddCell(TD(cgstAmt, Element.ALIGN_RIGHT, rowBg, true));
                itemTable.AddCell(TD(sgstPercent, Element.ALIGN_CENTER, rowBg));
                itemTable.AddCell(TD(sgstAmt, Element.ALIGN_RIGHT, rowBg, true));
                itemTable.AddCell(TD(igstPercent, Element.ALIGN_CENTER, rowBg));
                itemTable.AddCell(TD(igstAmt, Element.ALIGN_CENTER, rowBg, true));
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

            var wordsPara = new iTextParagraph();
            wordsPara.Add(new Chunk("Invoice Value in Words :\n", fSmall7));
            wordsPara.Add(new Chunk(totals.AmountInWords, fSmallB));
            leftInfo.AddCell(new PdfPCell(wordsPara)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_BOTTOM,
                Padding = 4
            });

            // ── Right: totals table ──
            PdfPTable totalsTable = new PdfPTable(2);
            totalsTable.WidthPercentage = 100;
            totalsTable.SetWidths(new float[] { 66.7f, 33.3f });

            void AddTotal(string label, string value, bool bold, iTextFont lf, iTextFont vf)
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
            AddTotal("Total CGST", "₹ " + totals.TotalCGST.ToString("N2"), false, fBoldRupee, fBoldRupee);
            AddTotal("Total SGST", "₹ " + totals.TotalSGST.ToString("N2"), false, fBoldRupee, fBoldRupee);
            AddTotal("Total IGST", "₹ " + totals.TotalIGST.ToString("N2"), false, fBoldRupee, fBoldRupee);
            AddTotal("Invoice Total", "₹ " + totals.InvoiceTotal.ToString("N2"), false, fBoldRupee, fBoldRupee);
            AddTotal("Round off", "₹ " + totals.RoundOff.ToString("N0"), false, fBoldRupee, fBoldRupee);
            AddTotal("Total Advance (Paid)", "₹ " + totals.TotalAdvance.ToString("N0"), false, fBoldRupee, fBoldRupee);
            //AddTotal("Balance Amount", "₹ " + totals.Balance.ToString("N0"), true, fBoldRupee, fBoldRupee);
            decimal balance = totals.Balance;
            string label = "Balance Amount";
            string value = "₹ " + Math.Abs(balance).ToString("N0");
            if (balance < 0)
            {
                label = "Overpaid (Refund Required)";
            }
            else if (balance == 0)
            {
                label = "Settled";
            }
            AddTotal(label, value, true, fBoldRupee, fBoldRupee);



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
            doc.Add(new iTextParagraph(company.Address, fSmall)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 4
            });
            doc.Add(new iTextParagraph($"Tel: {company.Phone}  Email: {company.Email}", fSmall)
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
            else
            {
                Utils.deleteOldFiles(folder, $"Invoice_*.pdf");
                fileName = $"Invoice_{header.InvoiceNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            }

            string filePath = Path.Combine(folder, fileName);

            stream.Position = 0;
            using (var fs = new FileStream(filePath, FileMode.Create))
                stream.CopyTo(fs);

            return $"{domainUrl}/reports/{fileName}";
        }
    }
}
