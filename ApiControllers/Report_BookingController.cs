using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2016.Excel;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Report_BookingController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_BookingController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetBookingReportGrid")]
        public IActionResult GetBookingReportGrid([FromBody] BookingReportRequestGRID request)
        {
            apiResponse apiResponse = new apiResponse();
            List<BookingReportGridModel> bookings = new List<BookingReportGridModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetBookingDetails_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId == 0 ? 0 : request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId == 0 ? 0 : request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId == 0 ? 0 : request.HallId);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@MonthDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.AddWithValue("@IsInvoiceGenerated", request.IsInvoiceGenerated);

                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    bookings.Add(new BookingReportGridModel
                    {
                        BRN = Convert.ToInt64(row["BRN"]),

                        EventDate = Convert.ToString(row["EventDate"]),
                        EventDate_Format = Convert.ToString(row["EventDate_Format"]),

                        EventTypeName = Convert.ToString(row["EventTypeName"]),
                        EventSubTypeName = Convert.ToString(row["EventSubTypeName"]),
                        EventDisplay = Convert.ToString(row["EventDisplay"]),

                        HallName = Convert.ToString(row["HallName"]),

                        Pax = Convert.ToInt32(row["Pax"]),

                        Package = Convert.ToString(row["Package"]),

                        DealAmount = Convert.ToDecimal(row["DealAmount"]),
                        GrossAmount = Convert.ToDecimal(row["GrossAmount"]),
                        Discount = Convert.ToDecimal(row["Discount"]),
                        NetAmount = Convert.ToDecimal(row["NetAmount"]),
                        TaxableAmount = Convert.ToDecimal(row["TaxableAmount"]),
                        TaxAmount = Convert.ToDecimal(row["TaxAmount"]),
                        InvoiceAmount = Convert.ToDecimal(row["InvoiceAmount"]),

                        Client_Name = Convert.ToString(row["Client_Name"]),
                        Client_Contact = Convert.ToString(row["Client_Contact"]),

                        InvoiceNo = row["InvoiceNo"] != DBNull.Value
                            ? Convert.ToInt64(row["InvoiceNo"])
                            : 0,

                        GSTInvoiceNo = row["GSTInvoiceNo"] != DBNull.Value
                            ? Convert.ToString(row["GSTInvoiceNo"])
                            : string.Empty
                    });
                }

                apiResponse.message = "Booking report fetched successfully!";
                apiResponse.data = bookings;
                apiResponse.status = "Success";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null ?? "");

                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DownloadBookingReport")]
        public IActionResult DownloadBookingReport([FromBody] BookingReportRequest request)
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

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    cmd = new SqlCommand("Report_GetBookingDetails_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;
                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);
                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MonthDuration", request.FilterDuration ?? (object)DBNull.Value);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= ASSIGN & VALIDATION =================
                #region ASSIGN & VALIDATIONS
                string propertyName= string.Empty, companyName = string.Empty, companyAddress = string.Empty,
                       companyphoneNo = string.Empty, companygstin = "", companyemail = "",
                       logoFile = string.Empty, userName = string.Empty,
                       stateCode = string.Empty, logoFullPath = string.Empty, generatedDate="";
                
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
                Document document = null;
                if (dsReport.Tables[2].Columns.Count > 7) {
                    document = new Document(PageSize.A4.Rotate(), 16, 16, topMargin, 50);
                    isA4size = false;
                }
                else
                {
                    document = new Document(PageSize.A4, 16, 16, topMargin, 50);
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
                    else {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
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
                }
                else
                {
                    // no company header, but still want page number / generated-by footer
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4 (
                            "", "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate (
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

                // ================= REPORT TITLE =================
                Font reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY);
                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };

                document.Add(reportTitle);

                // ================= TABLE =================

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD);

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                {
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));
                }

                table.HeaderRows = 1;

                int GetAlignment(string columnName)
                {
                    switch (columnName.Trim().ToLower())
                    {
                        case "brn":
                        case "invoice no":
                        case "event date":
                        case "pax":
                            return Element.ALIGN_CENTER;

                        case "deal amount":
                        case "gross total":
                        case "discount":
                        case "net total":
                        case "taxable amt":
                        case "tax amt":
                        case "invoice amt":
                            return Element.ALIGN_RIGHT;

                        default:
                            return Element.ALIGN_LEFT;
                    }
                }

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        bool isAmountColumn =
                            col.ColumnName.Equals("Deal Amount", StringComparison.OrdinalIgnoreCase) ||
                            col.ColumnName.Equals("Gross Total", StringComparison.OrdinalIgnoreCase) ||
                            col.ColumnName.Equals("Discount", StringComparison.OrdinalIgnoreCase) ||
                            col.ColumnName.Equals("Net Total", StringComparison.OrdinalIgnoreCase) ||
                            col.ColumnName.Equals("Taxable Amt", StringComparison.OrdinalIgnoreCase) ||
                            col.ColumnName.Equals("Tax Amt", StringComparison.OrdinalIgnoreCase) ||
                            col.ColumnName.Equals("Invoice Amt", StringComparison.OrdinalIgnoreCase);

                        string value = row[col]?.ToString() ?? "-";

                        if (isAmountColumn && row[col] != DBNull.Value &&
                            decimal.TryParse(Convert.ToString(row[col]), out decimal amount))
                        {
                            value = "₹ " + amount.ToString("N2");
                        }

                        int align = GetAlignment(col.ColumnName);

                        table.AddCell(
                            Utils.CreateBodyCell(
                                value,
                                isAmountColumn ? AmtFontNormal : tdFont,
                                align
                            )
                        );
                    }
                }

                // SUMMARY TOTALS
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = "";
                        bool isAmountColumn = false;
                        switch (col.ColumnName)
                        {
                            case "Pax":
                                value = Convert.ToDecimal(totalRow["TotalPax"]).ToString("N0");
                                break;

                            case "Deal Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDealAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Gross Total":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalGrossAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Discount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDiscount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Net Total":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalNetAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Taxable Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalTaxableAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Tax Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalTax"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Invoice Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalInvoiceAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            default:
                                value = col.Ordinal == 0 ? "TOTAL" : "";
                                break;
                        }

                        PdfPCell cell = new PdfPCell(new Phrase(value, isAmountColumn ? AmtFontBold : totalFont))
                        {
                            HorizontalAlignment = GetAlignment(col.ColumnName),
                            BackgroundColor = new BaseColor(240, 240, 240),
                            Padding = 6,
                            BorderColor = BaseColor.LIGHT_GRAY
                        };

                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================

                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"Booking_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Booking_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"{domainUrl}/reports/{fileName}";

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
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_GetBookingDetails_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // SAME LOGIC AS PDF
                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);
                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MonthDuration", request.FilterDuration ?? (object)DBNull.Value);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("BookingReport");

                int col = 1;

                foreach (DataRow header in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = header["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                int row = 2;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    }
                    row++;
                }

                // Add TOTAL ROW
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;
                        string value = "";

                        switch (columnName)
                        {
                            case "Pax":
                                value = Convert.ToDecimal(totalRow["TotalPax"]).ToString("N0");
                                break;

                            case "Deal Amount":
                                value = Convert.ToDecimal(totalRow["TotalDealAmount"]).ToString("N2");
                                break;

                            case "Gross Total":
                                value = Convert.ToDecimal(totalRow["TotalGrossAmount"]).ToString("N2");
                                break;

                            case "Discount":
                                value = Convert.ToDecimal(totalRow["TotalDiscount"]).ToString("N2");
                                break;

                            case "Net Total":
                                value = Convert.ToDecimal(totalRow["TotalNetAmount"]).ToString("N2");
                                break;

                            case "Taxable Amt":
                                value = Convert.ToDecimal(totalRow["TotalTaxableAmount"]).ToString("N2");
                                break;

                            case "Tax Amt":
                                value = Convert.ToDecimal(totalRow["TotalTax"]).ToString("N2");
                                break;

                            case "Invoice Amt":
                                value = Convert.ToDecimal(totalRow["TotalInvoiceAmount"]).ToString("N2");
                                break;

                            default:
                                if (i == 0)
                                    value = "TOTAL";
                                break;
                        }

                        var cell = worksheet.Cell(row, i + 1);

                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                        // Right align numeric totals
                        if (columnName == "Pax"
                            || columnName == "Deal Amount"
                            || columnName == "Gross Total"
                            || columnName == "Discount"
                            || columnName == "Net Total"
                            || columnName == "Taxable Amt"
                            || columnName == "Tax Amt"
                            || columnName == "Invoice Amt")
                        {
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                string reportsDir = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(reportsDir))
                    Directory.CreateDirectory(reportsDir);

                Utils.deleteOldFiles(reportsDir, "BookingReport_*.xlsx");

                string fileName = $"BookingReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(reportsDir, fileName);

                workbook.SaveAs(filePath);

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Booking Report Excel generated successfully.";
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

    }
}

