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
    public class Report_ProfitLossController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_ProfitLossController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        #region REPORT - PROFIT & LOSS

        [HttpPost]
        [Route("GetProfitLossGrid")]
        public IActionResult GetProfitLossGrid([FromBody] ProfitLossReportRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ds);
                }

                if (ds.Tables.Count < 2)
                    throw new Exception("P&L data incomplete.");

                DataTable dtLines = ds.Tables[0];
                DataTable dtSummary = ds.Tables[1];

                var lineItems = new List<ProfitLossLineItem>();
                foreach (DataRow row in dtLines.Rows)
                {
                    lineItems.Add(new ProfitLossLineItem
                    {
                        SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt64(row["SrNo"]) : 0,
                        Type = row["Type"] != DBNull.Value ? Convert.ToString(row["Type"]) : "-",
                        Description = row["Description"] != DBNull.Value ? Convert.ToString(row["Description"]) : "-",
                        Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0,
                        Nature = row["Nature"] != DBNull.Value ? Convert.ToString(row["Nature"]) : "-"
                    });
                }

                var summary = new ProfitLossSummary();
                if (dtSummary.Rows.Count > 0)
                {
                    DataRow s = dtSummary.Rows[0];
                    summary.TotalSales = s["TotalSales"] != DBNull.Value ? Convert.ToDecimal(s["TotalSales"]) : 0;
                    summary.TotalPurchase = s["TotalPurchase"] != DBNull.Value ? Convert.ToDecimal(s["TotalPurchase"]) : 0;
                    summary.TotalOtherIncome = s["TotalOtherIncome"] != DBNull.Value ? Convert.ToDecimal(s["TotalOtherIncome"]) : 0;
                    summary.TotalExpense = s["TotalExpense"] != DBNull.Value ? Convert.ToDecimal(s["TotalExpense"]) : 0;
                    summary.GrossProfit = s["GrossProfit"] != DBNull.Value ? Convert.ToDecimal(s["GrossProfit"]) : 0;
                    summary.NetProfit = s["NetProfit"] != DBNull.Value ? Convert.ToDecimal(s["NetProfit"]) : 0;
                }

                apiResponse.message = "P&L report fetched successfully!";
                apiResponse.data = new ProfitLossGridResponse { LineItems = lineItems, Summary = summary };
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
        [Route("DownloadProfitLossReport")]
        public IActionResult DownloadProfitLossReport([FromBody] ProfitLossReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);  // Portrait for P&L
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    cmd = new SqlCommand("Report_GetProfitLossDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ── Company info ──
                string companyName = string.Empty, companyAddress = string.Empty,
                       companyphoneNo = string.Empty, logoFile = string.Empty, userName = string.Empty;
                Image logo = null;

                if (request.WithHeader ?? false)
                {
                    if (dtCompany != null && dtCompany.Rows.Count > 0)
                    {
                        companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
                        companyAddress = dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "";
                        companyphoneNo = dtCompany.Rows[0]["CompanyMobile"]?.ToString() ?? "";
                        logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? "";
                        userName = dtCompany.Rows[0]["UserName"]?.ToString() ?? "";

                        if (!string.IsNullOrWhiteSpace(logoFile))
                        {
                            string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img", logoFile);
                            if (System.IO.File.Exists(logoPath))
                                logo = Image.GetInstance(logoPath);
                        }
                    }
                    else
                    {
                        companyName = "[PAGE HEADER] Company Data Not Found";
                    }
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data is incomplete.");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable summaryTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                writer.PageEvent = new PdfPageEvents(userName);
                document.Open();

                // ── Page header ──
                if (request.WithHeader ?? false)
                {
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 20f, 80f });

                    PdfPCell logoCell = logo != null ? new PdfPCell(logo) : new PdfPCell(new Phrase(""));
                    if (logo != null) logo.ScaleToFit(70f, 70f);
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

                // ── Report title ──
                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, BaseColor.BLACK))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 18f
                });

                // ── Line items table ──
                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                Font boldFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
                BaseColor thColor = new BaseColor(0, 121, 182);
                BaseColor creditBg = new BaseColor(232, 245, 233);   // light green
                BaseColor debitBg = new BaseColor(255, 235, 238);   // light red

                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 8f, 18f, 38f, 22f, 14f });

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    string nature = row["Nature"]?.ToString() ?? "";
                    BaseColor rowBg = nature == "Credit" ? creditBg : debitBg;

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string val = row[col]?.ToString() ?? "-";

                        PdfPCell cell = new PdfPCell(new Phrase(val, tdFont))
                        {
                            BackgroundColor = rowBg,
                            Padding = 6,
                            HorizontalAlignment = col.ColumnName == "Amount"
                                ? Element.ALIGN_RIGHT
                                : Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                }

                document.Add(table);

                // ── P&L Summary box ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow s = summaryTableDB.Rows[0];

                    decimal totalSales = s["TotalSales"] != DBNull.Value ? Convert.ToDecimal(s["TotalSales"]) : 0;
                    decimal totalPurchase = s["TotalPurchase"] != DBNull.Value ? Convert.ToDecimal(s["TotalPurchase"]) : 0;
                    decimal totalOtherIncome = s["TotalOtherIncome"] != DBNull.Value ? Convert.ToDecimal(s["TotalOtherIncome"]) : 0;
                    decimal totalExpense = s["TotalExpense"] != DBNull.Value ? Convert.ToDecimal(s["TotalExpense"]) : 0;
                    decimal grossProfit = s["GrossProfit"] != DBNull.Value ? Convert.ToDecimal(s["GrossProfit"]) : 0;
                    decimal netProfit = s["NetProfit"] != DBNull.Value ? Convert.ToDecimal(s["NetProfit"]) : 0;

                    document.Add(new Paragraph(" ") { SpacingAfter = 10f });

                    PdfPTable summaryTable = new PdfPTable(2);
                    summaryTable.WidthPercentage = 55;
                    summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    summaryTable.SetWidths(new float[] { 60f, 40f });

                    BaseColor summaryHeaderBg = new BaseColor(33, 37, 41);
                    Font summaryHeaderFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                    Font summaryLabelFont = new Font(Font.FontFamily.HELVETICA, 9);
                    Font summaryValueFont = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);

                    // Helper to add a summary row
                    void AddSummaryRow(string label, decimal value, BaseColor? bg = null, bool isTotal = false)
                    {
                        Font lf = isTotal ? boldFont : summaryLabelFont;
                        Font vf = isTotal ? new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD,
                                      value >= 0 ? new BaseColor(27, 94, 32) : new BaseColor(183, 28, 28))
                                          : summaryValueFont;

                        PdfPCell lCell = new PdfPCell(new Phrase(label, lf))
                        {
                            BackgroundColor = bg ?? BaseColor.WHITE,
                            Padding = 6,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        PdfPCell vCell = new PdfPCell(new Phrase(value.ToString("N2"), vf))
                        {
                            BackgroundColor = bg ?? BaseColor.WHITE,
                            Padding = 6,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        };
                        summaryTable.AddCell(lCell);
                        summaryTable.AddCell(vCell);
                    }

                    // Header row
                    PdfPCell hCell = new PdfPCell(new Phrase("P & L SUMMARY", summaryHeaderFont))
                    {
                        Colspan = 2,
                        BackgroundColor = summaryHeaderBg,
                        Padding = 8,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    summaryTable.AddCell(hCell);

                    AddSummaryRow("Total Sales", totalSales, new BaseColor(232, 245, 233));
                    AddSummaryRow("Total Other Income", totalOtherIncome, new BaseColor(232, 245, 233));
                    AddSummaryRow("Total Purchase", totalPurchase, new BaseColor(255, 235, 238));
                    AddSummaryRow("Total Expense", totalExpense, new BaseColor(255, 235, 238));

                    // Divider
                    PdfPCell divider = new PdfPCell(new Phrase(" "))
                    {
                        Colspan = 2,
                        BackgroundColor = new BaseColor(200, 200, 200),
                        FixedHeight = 1f,
                        Border = Rectangle.NO_BORDER
                    };
                    summaryTable.AddCell(divider);

                    AddSummaryRow("Gross Profit", grossProfit, new BaseColor(240, 240, 240), isTotal: true);
                    AddSummaryRow("Net Profit", netProfit, new BaseColor(240, 240, 240), isTotal: true);

                    document.Add(summaryTable);
                }

                document.Close();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "ProfitLoss_Report_*.pdf");

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fs);

                response.status = "Success";
                response.message = "Report Generated";
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
        [Route("DownloadProfitLossReport_Excel")]
        public IActionResult DownloadProfitLossReport_Excel([FromBody] ProfitLossReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable summaryTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("P&L Report");

                // ── Headers ──
                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 121, 182);
                    worksheet.Cell(1, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // ── Data rows ──
                int row = 2;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    string nature = dr["Nature"]?.ToString() ?? "";

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = dr[i]?.ToString() ?? "-";

                        if (nature == "Credit")
                            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(232, 245, 233);
                        else
                            cell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 235, 238);

                        if (dataTableDB.Columns[i].ColumnName == "Amount")
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    row++;
                }

                // ── Summary section ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow s = summaryTableDB.Rows[0];
                    row += 1; // blank gap

                    // Summary title
                    worksheet.Cell(row, 1).Value = "P & L SUMMARY";
                    worksheet.Range(row, 1, row, 5).Merge();
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(33, 37, 41);
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row++;

                    void AddSummaryExcelRow(string label, string value, XLColor bg)
                    {
                        worksheet.Cell(row, 1).Value = label;
                        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = bg;
                        worksheet.Cell(row, 2).Value = value;
                        worksheet.Cell(row, 2).Style.Fill.BackgroundColor = bg;
                        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        row++;
                    }

                    XLColor creditBg = XLColor.FromArgb(232, 245, 233);
                    XLColor debitBg = XLColor.FromArgb(255, 235, 238);
                    XLColor neutralBg = XLColor.FromArgb(240, 240, 240);

                    AddSummaryExcelRow("Total Sales", s["TotalSales"].ToString(), creditBg);
                    AddSummaryExcelRow("Total Other Income", s["TotalOtherIncome"].ToString(), creditBg);
                    AddSummaryExcelRow("Total Purchase", s["TotalPurchase"].ToString(), debitBg);
                    AddSummaryExcelRow("Total Expense", s["TotalExpense"].ToString(), debitBg);

                    // Gross & Net
                    worksheet.Cell(row, 1).Value = "Gross Profit";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Fill.BackgroundColor = neutralBg;
                    worksheet.Cell(row, 2).Value = s["GrossProfit"].ToString();
                    worksheet.Cell(row, 2).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Style.Fill.BackgroundColor = neutralBg;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    row++;

                    worksheet.Cell(row, 1).Value = "Net Profit";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Fill.BackgroundColor = neutralBg;
                    worksheet.Cell(row, 2).Value = s["NetProfit"].ToString();
                    worksheet.Cell(row, 2).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Style.Fill.BackgroundColor = neutralBg;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                worksheet.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "ProfitLoss_Report_*.xlsx");

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "P&L Report Excel generated successfully.";
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
