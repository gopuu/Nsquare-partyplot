using ClosedXML.Excel;
//using DocumentFormat.OpenXml.Office.Word;
//using DocumentFormat.OpenXml.Office2010.Excel;
//using DocumentFormat.OpenXml.Office2016.Excel;
//using DocumentFormat.OpenXml.Presentation;
//using DocumentFormat.OpenXml.Spreadsheet;
//using DocumentFormat.OpenXml.Wordprocessing;
using ERPAdmin.Helper;
using ERPAdmin.Models;
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

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;
        private readonly IInvoicePdfService _invoicePdfService;

        public ReportController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, IInvoicePdfService invoicePdfService) 
            : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            _invoicePdfService = invoicePdfService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        #region Booking Calendar - reports

        // ─────────────────────────────────────────────
        //  POST  /api/Invoice/DownloadGSTInvoice
        // ─────────────────────────────────────────────
        [HttpPost]
        [Route("DownloadCustomerInvoiceReport")]
        public async Task<IActionResult> DownloadCustomerInvoiceReport([FromBody] CustomerInvoiceReportRequest request)
        {
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                string fileUrl = await _invoicePdfService.GenerateCustomerInvoicePdf(request, connString, domain, domainUrl);

                return Ok(new
                {
                    status = "Success",
                    message = "Invoice Generated",
                    data = new { FileUrl = fileUrl }
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
                iTextDocument document = null;
                //if (dsReport.Tables[2].Columns.Count > 7)
                //{
                //    document = new iTextDocument(PageSize.A4.Rotate(), 16, 16, topMargin, 50);
                //    isA4size = false;
                //}
                //else
                //{
                document = new iTextDocument(iTextPageSize.A4, 16, 16, topMargin, 50);
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
                iTextFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                iTextFont boldFont_gray = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.GRAY);
                iTextFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                iTextFont normalFont_gray = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.GRAY);

                iTextFont boldFont9 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                iTextFont boldFont9_gray = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.GRAY);
                iTextFont normalFont9 = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                iTextFont normalFont9_gray = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY);

                iTextFont boldFont7 = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
                iTextFont boldFont7_gray = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.GRAY);
                iTextFont normalFont7 = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                iTextFont normalFont7_gray = FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.GRAY);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextFont AmtFontNormal = new iTextFont(bf, 9, iTextFont.NORMAL);
                iTextFont AmtFontBold = new iTextFont(bf, 9, iTextFont.NORMAL);

                string fontPath_ArialUnicode = Path.Combine(_env.WebRootPath, "fonts", "arialuni.ttf"); // Segoe UI Symbol
                BaseFont bfSymbol = BaseFont.CreateFont(fontPath_ArialUnicode, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextFont checkboxFont = new iTextFont(bfSymbol, 12, iTextFont.NORMAL);

                #endregion

                #region PAGE 1
                iTextParagraph titleForm = new iTextParagraph("Event Form", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, BaseColor.GRAY))
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
                    document.Add(new iTextParagraph("\n"));

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

                plateTitle.AddCell(new PdfPCell(new Phrase("PLATES SUMMARY", boldFont9_gray))
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

                /* ================= FOOTER - feedback & sign ================= */
                #region FOOTER - SIGN & GUEST FEEDBACK

                /* ================= MAIN WRAPPER ================= */
                PdfPTable wrapperTbl = new PdfPTable(1);
                wrapperTbl.WidthPercentage = 100;
                wrapperTbl.SpacingBefore = 10f;

                /* =========================================================
                   GUEST FEEDBACK BOX
                ========================================================= */
                PdfPTable feedbackTbl = new PdfPTable(3);
                feedbackTbl.WidthPercentage = 100;
                feedbackTbl.SetWidths(new float[] { 30, 10, 70 });

                PdfPCell feedbackOuter = new PdfPCell();
                feedbackOuter.Border = Rectangle.BOX;
                feedbackOuter.BorderColor = BaseColor.LIGHT_GRAY;
                feedbackOuter.Padding = 12;
                feedbackOuter.PaddingTop = 0;
                feedbackOuter.PaddingLeft = 0;
                feedbackOuter.PaddingRight = 0;

                /* ================= TITLE ================= */
                PdfPCell titleCell = new PdfPCell(new Phrase("GUEST FEEDBACK", boldFont_gray))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY,
                    Colspan = 3
                };
                feedbackTbl.AddCell(titleCell);

                /* ================= COMMENT HEADER ================= */
                feedbackTbl.AddCell(new PdfPCell(new Phrase(""))
                {
                    Border = Rectangle.NO_BORDER
                });

                feedbackTbl.AddCell(new PdfPCell(new Phrase(""))
                {
                    Border = Rectangle.NO_BORDER
                });

                feedbackTbl.AddCell(new PdfPCell(new Phrase("Comment", normalFont9_gray))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingTop = 5
                });

                /* ================= HELPER ================= */
                void AddFeedbackRow(string label)
                {
                    // LEFT LABEL
                    feedbackTbl.AddCell(new PdfPCell(new Phrase(label, normalFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10,
                        PaddingBottom = 7,
                        PaddingLeft = 30
                    });

                    // RATING
                    feedbackTbl.AddCell(new PdfPCell(new Phrase(".... /10", boldFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        PaddingTop = 10,
                        PaddingBottom = 7
                    });

                    // COMMENT LINE
                    feedbackTbl.AddCell(new PdfPCell(new Phrase(".....................................................................................................................", 
                        normalFont))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 10,
                        PaddingBottom = 7
                    });
                }

                /* ================= FEEDBACK ROWS ================= */
                AddFeedbackRow("Food Quality");
                AddFeedbackRow("Service");
                AddFeedbackRow("Ambience");
                AddFeedbackRow("Management");
                AddFeedbackRow("Overall Experience");

                /* ================= SIGNATURE SPACE ================= */
                feedbackTbl.AddCell(new PdfPCell(new Phrase(""))
                {
                    Border = Rectangle.NO_BORDER,
                    Colspan = 3,
                    FixedHeight = 5
                });

                PdfPCell signCell = new PdfPCell(new Phrase("Guest Signature  ...................................................................................", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingTop = 10,
                    Colspan = 3
                };
                feedbackTbl.AddCell(signCell);


                /* ================= ADD TABLE INTO OUTER CELL ================= */
                feedbackOuter.AddElement(feedbackTbl);
                wrapperTbl.AddCell(feedbackOuter);

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
                        HorizontalAlignment = Element.ALIGN_MIDDLE,
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
                    iTextParagraph optionsPara = new iTextParagraph();
                    optionsPara.SetLeading(0, 1.2f); // line spacing

                    foreach (var item in grp)
                    {
                        optionsPara.Add(new Chunk("☐ ", checkboxFont));
                        optionsPara.Add(new Chunk(item["OptionText"] + "    ", normalFont9));
                    }

                    optionsPara.SpacingAfter = 5f; // 🔥 this is your padding-bottom
                    optionsCell.AddElement(optionsPara); 
                    
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
                PdfPTable outerTbl = new PdfPTable(3);
                outerTbl.WidthPercentage = 100;
                outerTbl.SetWidths(new float[] { 33, 33, 33 });
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

                            phrase.Add(new Chunk("  ", normalFont9));          // spacing
                            phrase.Add(new Chunk("☐ ", checkboxFont));         // ✅ checkbox font
                            phrase.Add(new Chunk("Issue: ____________________", normalFont9)); // normal text

                            inner.AddCell(new PdfPCell(phrase)
                            {
                                Border = Rectangle.NO_BORDER,
                                PaddingBottom = 5
                            });
                        }
                        else
                        {
                            Phrase phrase = new Phrase();

                            phrase.Add(new Chunk("  ", normalFont9));          // spacing
                            phrase.Add(new Chunk("☐ ", checkboxFont));         // ✅ checkbox font
                            phrase.Add(new Chunk(text, normalFont9));          // normal text

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
                // 🔹 TITLE TABLE
                PdfPTable foodTitleTbl = new PdfPTable(3);
                foodTitleTbl.WidthPercentage = 100;
                foodTitleTbl.SpacingBefore = 5f;

                // Left Empty Cell
                foodTitleTbl.AddCell(new PdfPCell(new Phrase(""))
                {
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Padding = 5
                });

                // Center Title
                foodTitleTbl.AddCell(new PdfPCell(new Phrase(
                    dtFoodWasteTitle.Rows[0]["FoodWasteTitle"].ToString().ToUpper(),
                    boldFont7))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Padding = 5
                });

                // Right Side Location + DateTime
                foodTitleTbl.AddCell(new PdfPCell(new Phrase(
                    //dtFoodWasteTitle.Rows[0]["LocationDateTime"].ToString(),
                    "Location: \nsTime:",
                    normalFont7))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Border = Rectangle.BOX,
                    BorderWidth = 0.2f,
                    BorderColor = BaseColor.LIGHT_GRAY,
                    BackgroundColor = new BaseColor(240, 240, 240),
                    Padding = 5
                });

                // Width Ratio
                foodTitleTbl.SetWidths(new float[] { 25f, 50f, 25f });

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
                FH("Item Name");
                FH("Weight (KG/Litre)");

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
                DataTable dtBooking = new DataTable();

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
                    new SqlDataAdapter(cmd).Fill(dtBooking);
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
                iTextDocument document = null;
                //if (dsReport.Tables[2].Columns.Count > 7)
                //{
                //    document = new iTextDocument(iTextPageSize.A4.Rotate(), 16, 16, topMargin, 50);
                //    isA4size = false;
                //}
                //else
                //{
                    document = new iTextDocument(iTextPageSize.A4, 16, 16, topMargin, 50);
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

                if (dtBooking.Rows.Count == 0)
                    return NotFound("No booking data found.");

                DataRow row = dtBooking.Rows[0];
                document.Open();

                #region VARIABLES
                // Helper (optional but clean)
                string Get(string col) => row[col]?.ToString();

                // CLIENT
                string bookingId = Get("BookingId");
                string clientName = Get("Client_Name");
                string clientAddress = Get("Client_Address");
                string clientEmail = Get("Client_Email");
                string clientContact = Get("Client_Contact");
                string clientGSTIN = Get("Client_GSTIN");
                string clientState = Get("Client_State");

                // BOOKING
                string pax = Get("Booking_Pax");
                string hallRent = Get("Booking_HallRent");
                string leadReference = Get("LeadReference");
                string menuType = Get("Booking_MenuType");
                string bookingType = Get("Booking_Type");

                // PAYMENT
                string isGST = Get("PaymentSummary_IsGSTAccepted");
                string decorationCharges = Get("PaymentSummary_DecorationCharges");
                string roomCharges = Get("PaymentSummary_RoomCharges");
                string soundSystemCharges = Get("PaymentSummary_SoundSystemCharges");
                string projectorCharges = Get("PaymentSummary_ProjectorCharges");

                string taxableAmount = Get("PaymentSummary_TaxableAmount");
                string cgst = Get("PaymentSummary_CGST");
                string sgst = Get("PaymentSummary_SGST");
                string igst = Get("PaymentSummary_IGST");
                string total = Get("PaymentSummary_Total");
                string discount = Get("PaymentSummary_BookingDiscount_Total");
                string payableAmount = Get("PaymentSummary_PayableAmount");

                string paidAmount = Get("PaidAmount");
                string outstandingAmount = Get("OutstandingAmount");

                // EVENT
                string eventName = Get("EventName");
                string hallName = Get("HallName");
                string eventDate = Get("EventDate");
                string slot = Get("Slot");
                string timeSlot = Get("TimeSlot");

                string bookedOn = Get("BookedOn");
                string bookedBy = Get("BookedBy");

                string packageJson = Get("FoodPackageJSON");
                List<PackageItem_FunctionProspect> packageItems = new List<PackageItem_FunctionProspect>();
                if (!string.IsNullOrWhiteSpace(packageJson))
                {
                    packageItems = JsonConvert.DeserializeObject<List<PackageItem_FunctionProspect>>(packageJson);
                }

                // NOTES
                string otherCharges_BookingNoteForCustomer = Get("OtherCharges_BookingNoteForCustomer");
                string otherCharges_InternalOfficeNote = Get("OtherCharges_InternalOfficeNote");
                string tnc = Get("tnc");
                #endregion

                #region FONT DECLARATIONS
                iTextFont normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9); 
                iTextFont boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                iTextFont boldFont_lightgray = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, BaseColor.GRAY);

                BaseColor summaryBg = new BaseColor(82, 86, 88);
                iTextFont whiteBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);

                iTextFont labelFontLeft = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.BLACK);
                iTextFont labelFontBoldLeft = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.BLACK);

                iTextFont summaryLabelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);
                iTextFont summaryValueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, BaseColor.WHITE);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextFont fontRupeeNormal = new iTextFont(bf, 9, iTextFont.NORMAL);

                string fontPath_Bold = Path.Combine(_env.WebRootPath, "fonts", "arialbd.ttf");
                BaseFont bf_Bold = BaseFont.CreateFont(fontPath_Bold, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextFont fontRupeeBold = new iTextFont(bf_Bold, 9, iTextFont.NORMAL);

                #endregion

                // ===== TITLE LINES =====
                //PdfPTable lineTop = new PdfPTable(1) { WidthPercentage = 100 };
                //lineTop.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, FixedHeight = 1, BackgroundColor = BaseColor.BLACK });
                //document.Add(lineTop);

                iTextParagraph title = new iTextParagraph("Function Prospectus", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, BaseColor.GRAY))
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
                iTextFont labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                iTextFont labelFontnormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                iTextFont valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
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

                AddLeftRow("Booking Ref.", '#' + bookingId);
                AddLeftRow("Client Name", clientName);
                AddLeftRow("Contact No.", clientContact);
                AddLeftRow("Address", clientAddress);
                AddLeftRow("Email", clientEmail);
                AddLeftRow("GSTIN", clientGSTIN);
                AddLeftRow("State", clientState);

                PdfPTable rightTable = new PdfPTable(1) { WidthPercentage = 100 };
                void AddRightRow(string label, string value)
                {
                    Phrase p = new Phrase();
                    p.Add(new Chunk(label + " : ", labelFont));
                    p.Add(new Chunk(value, valueFont));
                    rightTable.AddCell(new PdfPCell(p) { Border = Rectangle.NO_BORDER, PaddingBottom = 6, PaddingLeft = 4 });
                }

                AddRightRow("PAX", pax);
                AddRightRow("Hall", hallName);
                AddRightRow("Menu Type", menuType);
                AddRightRow("Booking Type", bookingType);

                AddRightRow("Event", eventName);
                AddRightRow("Event Date", eventDate);
                AddRightRow("Slot", slot);
                AddRightRow("Timing", timeSlot);

                //AddRightRow("Is GST Accepted?", isGST);
                //AddRightRow("Hall Rent", hallRent);
                //AddRightRow("Decoration Charges", decorationCharges);
                //AddRightRow("Room Charges", roomCharges);
                //AddRightRow("Sound System Charges", soundSystemCharges);
                //AddRightRow("Projector Charges", projectorCharges);

                detailsTable.AddCell(new PdfPCell(leftTable) { Border = Rectangle.NO_BORDER, PaddingTop = 10 });
                detailsTable.AddCell(new PdfPCell(rightTable) { Border = Rectangle.NO_BORDER, PaddingTop = 10, HorizontalAlignment = Element.ALIGN_RIGHT });
                document.Add(detailsTable);
                document.Add(new iTextParagraph("\n"));

                // ================= FOOD PACKAGE (FULL WIDTH) =================
                PdfPTable packageTable = new PdfPTable(1) { WidthPercentage = 100 };
                int i = 1;
                foreach (var pkg in packageItems)
                {
                    // 🔹 INNER TABLE (per package)
                    PdfPTable pkgTable = new PdfPTable(1) { WidthPercentage = 100 };

                    // 🔹 Title
                    string title2 = packageItems.Count > 1
                        ? $"FOOD PACKAGE {i}: {pkg.FoodPackageName} (₹{Convert.ToDecimal(pkg.PerPlate):N0}/Plate)"
                        : $"FOOD PACKAGE: {pkg.FoodPackageName} (₹{Convert.ToDecimal(pkg.PerPlate):N0}/Plate)";

                    pkgTable.AddCell(new PdfPCell(new Phrase(title2, fontRupeeBold))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingTop = 6,
                        PaddingBottom = 2
                    });

                    // 🔹 Pax + Total
                    string? paxVal = i == 1 ? pax : pkg.Pax;

                    var phrase = new Phrase();
                    phrase.Add(new Chunk($"PAX: {paxVal} | Total: ", normalFont));
                    phrase.Add(new Chunk($"₹{Convert.ToDecimal(pkg.TotalCharges):N0}", fontRupeeNormal));

                    pkgTable.AddCell(new PdfPCell(phrase)
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 6
                    });

                    // 🔹 MENU HEADER
                    pkgTable.AddCell(new PdfPCell(new Phrase("Menu", boldFont_lightgray))
                    {
                        Border = Rectangle.NO_BORDER,
                        PaddingBottom = 3
                    });

                    // 🔹 MENU ITEMS
                    var menuGrouped = pkg.MenuItemsList?
                        .GroupBy(x => x.MenuHeader_Name)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.MenuItem_Name).ToList()
                        );

                    if (menuGrouped != null)
                    {
                        foreach (var header in menuGrouped)
                        {
                            BaseColor bgColor = new BaseColor(245, 245, 245);

                            var paragraph = new iTextParagraph();
                            paragraph.Add(new Phrase(header.Key + "\n", boldFont));

                            var items = header.Value
                                .Select((item, index) => $"({index + 1}) {item}")
                                .ToList();

                            paragraph.Add(new Phrase(string.Join("   ", items), normalFont));

                            pkgTable.AddCell(new PdfPCell(paragraph)
                            {
                                Border = Rectangle.BOX,
                                BorderColor = BaseColor.WHITE,
                                BorderWidth = 2f,
                                BackgroundColor = bgColor,
                                Padding = 6
                            });
                        }
                    }

                    // 🔹 ADD-ONS
                    var addonGrouped = pkg.AddOnsList?
                        .GroupBy(x => x.AddOn_Header_Name)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(x => x.AddOn_Item_Name).ToList()
                        );

                    if (addonGrouped != null && addonGrouped.Count > 0)
                    {
                        pkgTable.AddCell(new PdfPCell(new Phrase("Add-ons", boldFont_lightgray))
                        {
                            Border = Rectangle.NO_BORDER,
                            PaddingTop = 6,
                            PaddingBottom = 4
                        });

                        foreach (var header in addonGrouped)
                        {
                            BaseColor bgColor = new BaseColor(245, 245, 245);

                            var paragraph = new iTextParagraph();
                            paragraph.Add(new Phrase(header.Key + "\n", boldFont));

                            var items = header.Value
                                .Select((item, index) => $"({index + 1}) {item}")
                                .ToList();

                            paragraph.Add(new Phrase(string.Join("   ", items), normalFont));

                            pkgTable.AddCell(new PdfPCell(paragraph)
                            {
                                Border = Rectangle.BOX,
                                BorderColor = BaseColor.WHITE,
                                BorderWidth = 2f,
                                BackgroundColor = bgColor,
                                Padding = 6
                            });
                        }
                    }

                    // 🔹 WRAP PACKAGE WITH BORDER
                    PdfPCell outerCell = new PdfPCell(pkgTable)
                    {
                        Border = Rectangle.BOX,
                        BorderColor = BaseColor.LIGHT_GRAY, // ✅ light gray border
                        BorderWidth = 1f,
                        Padding = 8,
                        PaddingBottom = 10
                    };

                    packageTable.AddCell(outerCell);

                    // 🔹 SPACE BETWEEN PACKAGES
                    packageTable.AddCell(new PdfPCell(new Phrase(" "))
                    {
                        Border = Rectangle.NO_BORDER,
                        FixedHeight = 6
                    });

                    i++;
                }

                // ✅ ADD PACKAGE FULL WIDTH
                document.Add(packageTable);
                document.Add(new iTextParagraph("\n"));

                // ================= PAYMENT TABLE =================
                PdfPTable paymentTable = new PdfPTable(2) { WidthPercentage = 100 };
                paymentTable.SetWidths(new float[] { 0.65f, 0.35f });
                // Header
                paymentTable.AddCell(new PdfPCell(new Phrase("PAYMENT SUMMARY", whiteBold))
                {
                    Colspan = 2,
                    BackgroundColor = summaryBg,
                    Border = Rectangle.BOX,
                    BorderColor = BaseColor.LIGHT_GRAY,
                    Padding = 6
                });
                void AddAmountRow(string label, string value)
                {
                    paymentTable.AddCell(new PdfPCell(new Phrase(label, normalFont))
                    {
                        Border = Rectangle.BOX,
                        BorderColor = BaseColor.LIGHT_GRAY,
                        Padding = 4
                    });

                    paymentTable.AddCell(new PdfPCell(new Phrase($"₹ {Convert.ToDecimal(value):N0}", fontRupeeNormal))
                    {
                        Border = Rectangle.BOX,
                        BorderColor = BaseColor.LIGHT_GRAY,
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        Padding = 4
                    });
                }

                AddAmountRow("Deal Amount", taxableAmount);
                AddAmountRow("Booking Discount", discount);
                AddAmountRow("Payable Amount", payableAmount);
                AddAmountRow("Total Paid", paidAmount);
                AddAmountRow("Outstanding Amount", outstandingAmount);

                if (request.IsWithPrice)
                {
                    document.Add(paymentTable);
                }
                document.Add(new iTextParagraph("\n"));

                // ================= BOOKING INFO =================
                PdfPTable infoTable = new PdfPTable(1) { WidthPercentage = 100 };
                //infoTable.AddCell(new PdfPCell(new Phrase("BOOKING DETAILS", boldFont))
                //{
                //    Border = Rectangle.NO_BORDER,
                //    PaddingBottom = 6
                //});
                infoTable.AddCell(new PdfPCell(new Phrase($"Booked By : {bookedBy}", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 3
                });
                infoTable.AddCell(new PdfPCell(new Phrase($"Booked On : {bookedOn}", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 3
                });
                infoTable.AddCell(new PdfPCell(new Phrase($"Reference : {leadReference}", normalFont))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 6
                });

                document.Add(infoTable);
                document.Add(new iTextParagraph("\n"));

                // ================= TERMS & CONDITIONS =================
                PdfPTable tncTable = new PdfPTable(1) { WidthPercentage = 100 };
                tncTable.AddCell(new PdfPCell(new Phrase("TERMS & CONDITIONS", boldFont))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingTop = 6,
                    PaddingBottom = 6
                });
                // actual terms text
                tncTable.AddCell(new PdfPCell(new Phrase(tnc ?? "", labelFont))
                {
                    Border = Rectangle.NO_BORDER,
                    PaddingBottom = 10,
                    MinimumHeight = 60f
                });
                document.Add(tncTable);

                // ===== SIGNATURES =====
                PdfPTable signTable = new PdfPTable(3) { WidthPercentage = 100};
                signTable.SetWidths(new float[] { 0.33f, 0.33f, 0.33f });
                foreach (var sign in new[] { "Sales Manager's Sign.", "Banquet Manager's Sign.", "Guest's Sign" })
                {
                    PdfPCell cell = new PdfPCell() { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER };
                    cell.AddElement(new Phrase("__________________________", labelFont));
                    cell.AddElement(new Phrase(sign, labelFont));
                    signTable.AddCell(cell);
                }
                document.Add(signTable);

                // 2nd Page - NOTES
                //document.NewPage();
                document.Add(new iTextParagraph("\n"));

                title = new iTextParagraph("Booking Notes", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 3,
                    SpacingAfter = 8
                };
                document.Add(title);

                lineBottom = new PdfPTable(1) { WidthPercentage = 100 };
                lineBottom.AddCell(new PdfPCell() { Border = Rectangle.NO_BORDER, FixedHeight = 1, BackgroundColor = BaseColor.LIGHT_GRAY });
                document.Add(lineBottom);

                //BOOKING NOTES
                string notesJson = row["NotesJSON"]?.ToString() ?? "[]";
                List<BookingNotePdf> notes = JsonConvert.DeserializeObject<List<BookingNotePdf>>(notesJson) ?? new List<BookingNotePdf>();

                bool hasPrintedNotes = false;

                iTextParagraph notesPara = new iTextParagraph();
                notesPara.SpacingBefore = 10f;

                if (notes != null && notes.Any())
                {
                    foreach (var category in notes.GroupBy(x => x.CategoryName ?? "Others"))
                    {
                        bool categoryHasData = false;
                        iTextParagraph categoryPara = new iTextParagraph();

                        categoryPara.Add(new Chunk((category.Key ?? "Others") + "\n", labelFontBoldLeft));

                        foreach (var item in category)
                        {
                            string value = "";

                            // Selected Options
                            if (item.Options != null && item.Options.Any())
                            {
                                value = string.Join(", ",
                                    item.Options
                                        .Where(x => !string.IsNullOrWhiteSpace(x.OptionName))
                                        .Select(x => x.OptionName));
                            }

                            // Value
                            if (string.IsNullOrWhiteSpace(value) &&
                                !string.IsNullOrWhiteSpace(item.Value))
                            {
                                value = item.Value;
                            }

                            // Remarks
                            if (string.IsNullOrWhiteSpace(value) &&
                                !string.IsNullOrWhiteSpace(item.Remarks))
                            {
                                value = item.Remarks;
                            }

                            if (string.IsNullOrWhiteSpace(value))
                                continue;

                            categoryHasData = true;
                            hasPrintedNotes = true;

                            categoryPara.Add(new Chunk("• ", labelFontBoldLeft));
                            categoryPara.Add(new Chunk((item.SubCategoryName ?? "-") + ": ", labelFontBoldLeft));
                            categoryPara.Add(new Chunk(value, labelFontLeft));

                            if (!string.IsNullOrWhiteSpace(item.Remarks) &&
                                !string.Equals(item.Remarks, value, StringComparison.Ordinal))
                            {
                                categoryPara.Add(new Chunk(" (" + item.Remarks + ")", labelFontLeft));
                            }

                            categoryPara.Add(Chunk.NEWLINE);
                        }

                        if (categoryHasData)
                        {
                            categoryPara.Add(Chunk.NEWLINE);
                            notesPara.Add(categoryPara);
                        }
                    }
                }

                if (!hasPrintedNotes)
                {
                    notesPara.Add(new Chunk("No booking notes available.", labelFontLeft));
                }

                document.Add(notesPara);

                if (!string.IsNullOrWhiteSpace(otherCharges_BookingNoteForCustomer))
                {
                    document.Add(new Paragraph("Booking Note For Customer :", boldFont));
                    document.Add(new Paragraph(otherCharges_BookingNoteForCustomer, normalFont));
                    document.Add(new Paragraph(" ")); // Optional spacing
                }

                if (!string.IsNullOrWhiteSpace(otherCharges_InternalOfficeNote))
                {
                    document.Add(new Paragraph("Internal Office Note :", boldFont));
                    document.Add(new Paragraph(otherCharges_InternalOfficeNote, normalFont));
                    document.Add(new Paragraph(" ")); // Optional spacing
                }

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

        #region INVENTORY DASHBOARD

        [HttpPost]
        [Route("Report_Inventory_StockOutSummary_RawMaterialWise_Pdf")]
        public IActionResult Report_Inventory_StockOutSummary_RawMaterialWise_Pdf([FromBody] Report_SummaryStockOutRequest_PdfExcel request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // Company
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // Report
                    cmd = new SqlCommand("Report_Inventory_StockOutSummary_RawMaterialWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@MonthDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    //cmd.Parameters.AddWithValue("@RawMaterialId",
                    //    string.IsNullOrEmpty(request.RawMaterialId) ? (object)DBNull.Value : request.RawMaterialId);
                    //cmd.Parameters.AddWithValue("@Columns",
                    //    string.IsNullOrEmpty(request.Columns) ? (object)DBNull.Value : request.Columns);

                    new SqlDataAdapter(cmd).Fill(dsReport);
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
                        propertyName = dtCompany.Rows[0]["PropertyName"]?.ToString() ?? "";
                        companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
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

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data is incomplete. Required tables are missing.");

                // ================= EXTRACT DATA =================
                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                float topMargin = (request.WithHeader ?? false) ? 80f : 36f;

                using var stream = new MemoryStream();

                bool isA4size = true; // default to A4
                iTextDocument document = null;
                if (dsReport.Tables[2].Columns.Count > 7)
                {
                    document = new iTextDocument(iTextPageSize.A4.Rotate(), 16, 16, topMargin, 50);
                    isA4size = false;
                }
                else
                {
                    document = new iTextDocument(iTextPageSize.A4, 16, 16, topMargin, 50);
                    isA4size = true;
                }

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
                            propertyName,
                            companyName,
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

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                // ── Report title ──
                document.Add(new iTextParagraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                // ── Table ──
                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                iTextFont thFont = new iTextFont(iTextFont.FontFamily.HELVETICA, 8, iTextFont.BOLD, BaseColor.BLACK);
                iTextFont tdFont = new iTextFont(iTextFont.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(hRow["HeaderName"].ToString(), thFont, thColor));

                table.HeaderRows = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = row[col]?.ToString() ?? "-";
                        if (col.ColumnName == "Total Stock Out Qty")
                        {
                            value = $"{value}";
                        }
                        table.AddCell(Utils.CreateBodyCell(value, tdFont));
                    }
                }

                document.Add(table);
                document.Close();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "StockOutSummary_Report_*.pdf");

                string fileName = $"StockOutSummary_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);

                response.status = "Success";
                response.message = "Summary Report Generated";
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
        [Route("Report_Inventory_StockOutSummary_RawMaterialWise_Excel")]
        public IActionResult Report_Inventory_StockOutSummary_RawMaterialWise_Excel([FromBody] Report_SummaryStockOutRequest_PdfExcel request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_Inventory_StockOutSummary_RawMaterialWise", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);

                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);

                    cmd.Parameters.AddWithValue("@MonthDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Stock Out Summary");

                int col = 1;

                // ================= HEADERS =================
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    var cell = worksheet.Cell(1, col);
                    cell.Value = hRow["HeaderName"].ToString();
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                int row = 2;

                // ================= DATA =================
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(row, i + 1);

                        string columnName = dataTableDB.Columns[i].ColumnName;
                        string value = dr[i]?.ToString() ?? "-";

                        // 👉 Right align numeric columns
                        if (columnName == "Stock Out Count" || columnName == "Total Stock Out Qty")
                        {
                            cell.Value = string.IsNullOrEmpty(value) ? "0" : value;
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        else
                        {
                            cell.Value = value;
                        }
                    }
                    row++;
                }

                // ================= AUTO FIT =================
                worksheet.Columns().AdjustToContents();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "StockOutSummary_Report_*.xlsx");

                string fileName = $"StockOutSummary_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Stock Out Summary Excel generated successfully";
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
    public override void OnEndPage(PdfWriter writer, iTextDocument document)
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

    iTextFont footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
    BaseColor lineColor = new BaseColor(180, 180, 180);

    public override void OnEndPage(PdfWriter writer, iTextDocument document)
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
