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
    public class Report_Income_SummaryController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_Income_SummaryController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetIncomeSummaryReport_Grid")]
        public IActionResult GetIncomeSummaryReport_Grid([FromBody] IncomeSummaryGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<IncomeSummaryGridModel> rows = new List<IncomeSummaryGridModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetIncomeSummary_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // 🔹 Filters
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    //cmd.Parameters.AddWithValue("@Month", string.IsNullOrWhiteSpace(request.Month) ? (object)DBNull.Value : request.Month);
                    cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);

                    con.Open();

                    using SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }


                /* ─────────────────────────────
                   RESULT SET 1 : GRID DATA
                ───────────────────────────── */
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        rows.Add(new IncomeSummaryGridModel
                        {
                            MonthName = row["MonthName"] != DBNull.Value ? Convert.ToString(row["MonthName"]) ?? "" : "",
                            MonthName2 = row["MonthName2"] != DBNull.Value ? Convert.ToString(row["MonthName2"]) ?? "" : "",
                            IncomeCategoryName = row["IncomeCategoryName"] != DBNull.Value ? Convert.ToString(row["IncomeCategoryName"]) ?? "" : "-",
                            IncomeCategoryId = row["IncomeCategoryId"] != DBNull.Value ? Convert.ToInt64(row["IncomeCategoryId"]) : 0,
                            Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0
                        });
                    }
                }
                /* ─────────────────────────────
                   RESULT SET 2 : TOTAL
                ───────────────────────────── */
                object? totals = null;

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow t = ds.Tables[1].Rows[0];
                    totals = new
                    {
                        TotalAmount = t["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(t["TotalAmount"]) : 0
                    };
                }
                /* ─────────────────────────────
                   RESPONSE
                ───────────────────────────── */
                apiResponse.message = "Income summary report fetched successfully!";
                apiResponse.data = new
                {
                    Rows = rows,
                    Totals = totals
                };
                apiResponse.status = "Success";
                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                apiResponse.message = ex.Message;
                apiResponse.status = "Error";

                return StatusCode(500, apiResponse);
            }
        }
    
        [HttpPost]
        [Route("DownloadIncomeSummaryReport_Pdf")]
        public IActionResult DownloadIncomeSummaryReport_Pdf([FromBody] IncomeSummaryReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                // ============================================================
                // DATABASE
                // ============================================================
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // ── Company Header ──
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // ── Income Summary Report ──
                    cmd = new SqlCommand("Report_GetIncomeSummary_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    //cmd.Parameters.AddWithValue("@Month", string.IsNullOrWhiteSpace(request.Month) ? (object)DBNull.Value : request.Month);
                    // 🔹 Filters
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ============================================================
                // ASSIGN & VALIDATION
                // ============================================================
                #region ASSIGN & VALIDATIONS

                string propertyName = string.Empty;
                string companyName = string.Empty;
                string companyAddress = string.Empty;
                string companyphoneNo = string.Empty;
                string companygstin = "";
                string companyemail = "";
                string logoFile = string.Empty;
                string userName = string.Empty;
                string stateCode = string.Empty;
                string logoFullPath = string.Empty;
                string generatedDate = "";

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
                        companygstin = dtCompany.Rows[0]["CompanyGST"]?.ToString() ?? "";
                        companyemail = dtCompany.Rows[0]["CompanyEmail"]?.ToString() ?? "";
                        stateCode = dtCompany.Rows[0]["StateCode"]?.ToString() ?? "";

                        if (!string.IsNullOrWhiteSpace(logoFile))
                        {
                            string p1 = Path.Combine(
                                _env.WebRootPath,
                                "uploads",
                                "org",
                                domain,
                                "CompanyInvoice_CompanyLogo_Image",
                                logoFile
                            );

                            if (System.IO.File.Exists(p1))
                            {
                                logoFullPath = p1;
                            }
                            else
                            {
                                string p2 = Path.Combine(
                                    _env.WebRootPath,
                                    "assets2",
                                    "img",
                                    logoFile
                                );

                                if (System.IO.File.Exists(p2))
                                {
                                    logoFullPath = p2;
                                }
                            }
                        }
                    }
                    else
                    {
                        companyName = "[PAGE HEADER] Company Data Not Found";
                    }
                }

                if (dsReport.Tables.Count < 4)
                {
                    throw new Exception("Report data is incomplete. Required tables are missing.");
                }

                // ============================================================
                // EXTRACT DATA
                // ============================================================
                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables[3];

                float topMargin = (request.WithHeader ?? false) ? 80f : 36f;

                using var stream = new MemoryStream();

                bool isA4size = true;
                Document document;

                if (dataTableDB.Columns.Count > 7)
                {
                    document = new Document(
                        PageSize.A4.Rotate(),
                        16,
                        16,
                        topMargin,
                        50
                    );

                    isA4size = false;
                }
                else
                {
                    document = new Document(
                        PageSize.A4,
                        16,
                        16,
                        topMargin,
                        50
                    );

                    isA4size = true;
                }

                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                #endregion

                // ============================================================
                // HEADER / FOOTER
                // ============================================================
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
                    if (isA4size)
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4(
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }

                #endregion

                document.Open();

                // ============================================================
                // REPORT TITLE
                // ============================================================
                document.Add(new Paragraph(reportName.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY)) 
                { 
                    Alignment = Element.ALIGN_CENTER, 
                    SpacingAfter = 10f 
                });

                // ============================================================
                // TABLE
                // ============================================================
                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                // RUPEE FONT
                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font AmtFontNormal = new Font(bf, 8, Font.NORMAL);
                Font AmtFontBold = new Font(bf, 8, Font.BOLD);

                // TABLE HEADER
                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                {
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0]?.ToString() ?? "", thFont, thColor));
                }

                table.HeaderRows = 1;

                // ============================================================
                // DATA ROWS
                // ============================================================
                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim();

                        bool isAmountColumn = colName.Equals("Amount", StringComparison.OrdinalIgnoreCase);
                        string value = row[col] == DBNull.Value ? "-" : row[col]?.ToString() ?? "-";

                        if (isAmountColumn && decimal.TryParse(value, out decimal amount))
                        {
                            value = "₹ " + amount.ToString("N2");
                        }

                        int align = colName.ToLower() switch
                        {
                            "month" => Element.ALIGN_CENTER,
                            "income category" => Element.ALIGN_LEFT,
                            "amount" => Element.ALIGN_RIGHT,
                            _ => Element.ALIGN_LEFT
                        };

                        table.AddCell(
                            Utils.CreateBodyCell(
                                value,
                                isAmountColumn ? AmtFontNormal : tdFont,
                                align
                            )
                        );
                    }
                }

                // ============================================================
                // TOTAL ROW
                // ============================================================
                if (totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    decimal totalAmount = totalRow["TotalAmount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(totalRow["TotalAmount"]);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim();

                        string value = "";
                        bool isAmountColumn = false;

                        if (colName.Equals("Amount",StringComparison.OrdinalIgnoreCase))
                        {
                            value = "₹ " + totalAmount.ToString("N2");
                            isAmountColumn = true;
                        }
                        else if (col.Ordinal == 0)
                        {
                            value = "TOTAL";
                        }

                        int align = value == "TOTAL" ? Element.ALIGN_CENTER : Element.ALIGN_RIGHT;

                        table.AddCell(
                            new PdfPCell(
                                new Phrase(
                                    value,
                                    isAmountColumn ? AmtFontBold : new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD)
                                )
                            )
                            {
                                HorizontalAlignment = align,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                BackgroundColor = new BaseColor(240, 240, 240),
                                Padding = 6,
                                BorderColor = new BaseColor(220, 220, 220),
                                BorderWidth = 0.5f
                            }
                        );
                    }
                }

                document.Add(table);
                document.Close();

                // ============================================================
                // SAVE PDF
                // ============================================================
                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                Utils.deleteOldFiles(folder, "IncomeSummary_Report_*.pdf");

                string fileName = $"IncomeSummary_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                // ============================================================
                // RESPONSE
                // ============================================================
                response.status = "Success";
                response.message = "Income Summary Report Generated";

                response.data = new
                {
                    FileUrl = $"{domainUrl}/reports/{fileName}"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                response.status = "Fail";
                response.message = ex.Message;

                return Ok(response);
            }
        }

        [HttpPost]
        [Route("DownloadIncomeSummaryReport_Excel")]
        public IActionResult DownloadIncomeSummaryReport_Excel([FromBody] IncomeSummaryReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet dsReport = new DataSet();

                // DATABASE
                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_GetIncomeSummary_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null ? JsonConvert.SerializeObject(request.Columns) : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    // 🔹 Filters
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    //cmd.Parameters.AddWithValue("@Month", string.IsNullOrWhiteSpace(request.Month) ? (object)DBNull.Value : request.Month);
                    cmd.Parameters.AddWithValue("@CategoryId", request.CategoryId);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // VALIDATION
                if (dsReport.Tables.Count < 4)
                    throw new Exception("Report data is incomplete. Required tables are missing.");

                // TABLES
                string reportName = dsReport.Tables[0].Rows.Count > 0 ? dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "Income Summary Report" : "Income Summary Report";

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables[3];

                // WORKBOOK
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Income Summary");

                int columnCount = dataTableDB.Columns.Count;

                if (columnCount == 0)
                    throw new Exception("No columns available for Income Summary Report.");

                // REPORT TITLE
                worksheet.Range(1, 1, 1, columnCount).Merge();

                var titleCell = worksheet.Cell(1, 1);

                titleCell.Value = reportName.ToUpper();
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.FontSize = 14;
                titleCell.Style.Font.FontColor = XLColor.Gray;
                titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Row(1).Height = 24;

                // HEADER
                int headerRow = 3;
                int col = 1;

                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    var cell = worksheet.Cell(headerRow, col);

                    cell.Value = hRow["HeaderName"]?.ToString() ?? "";
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    col++;
                }

                worksheet.Row(headerRow).Height = 20;

                // DATA
                int row = headerRow + 1;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        DataColumn column = dataTableDB.Columns[i];
                        string columnName = column.ColumnName.Trim();
                        var cell = worksheet.Cell(row, i + 1);

                        if (columnName.Equals("Amount", StringComparison.OrdinalIgnoreCase))
                        {
                            decimal amount = dr[i] == DBNull.Value ? 0 : Convert.ToDecimal(dr[i]);

                            cell.Value = amount;
                            cell.Style.NumberFormat.Format = "₹ #,##0.00";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        else if (columnName.Equals("Month", StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Value = dr[i] == DBNull.Value ? "-" : dr[i]?.ToString() ?? "-";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
                        else if (columnName.Equals("Income Category", StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Value = dr[i] == DBNull.Value ? "-" : dr[i]?.ToString() ?? "-";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        }
                        else
                        {
                            cell.Value = dr[i] == DBNull.Value ? "-" : dr[i]?.ToString() ?? "-";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        }

                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    row++;
                }

                // TOTAL ROW
                if (totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    decimal totalAmount = totalRow["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(totalRow["TotalAmount"]);

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName.Trim();
                        var cell = worksheet.Cell(row, i + 1);

                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                        if (i == 0)
                        {
                            cell.Value = "TOTAL";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }

                        if (columnName.Equals("Amount", StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Value = totalAmount;
                            cell.Style.NumberFormat.Format = "₹ #,##0.00";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                    }
                }

                // FINAL FORMATTING
                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(headerRow);

                if (dataTableDB.Rows.Count > 0)
                    worksheet.Range(headerRow, 1, row - 1, columnCount).SetAutoFilter();

                // SAVE
                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "IncomeSummary_Report_*.xlsx");

                string fileName = $"IncomeSummary_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                // RESPONSE
                response.status = "Success";
                response.message = "Income Summary Report Excel generated successfully.";
                response.data = new { FileUrl = $"{domainUrl}/reports/{fileName}" };

                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                response.status = "Fail";
                response.message = ex.Message;

                return Ok(response);
            }
        }

    }
}
