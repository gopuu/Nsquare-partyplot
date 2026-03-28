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

        [HttpPost]
        [Route("GetProfitLossGrid")]
        public IActionResult GetProfitLossGrid([FromBody] ProfitLossGridRequest request)
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

                    cmd.Parameters.AddWithValue("@YearId",
                        request.YearId == 0 ? (object)DBNull.Value : request.YearId);

                    cmd.Parameters.AddWithValue("@Year",
                        request.Year == 0 ? (object)DBNull.Value : request.Year);

                    cmd.Parameters.AddWithValue("@MonthId_From",
                        request.MonthId_From == 0 ? (object)DBNull.Value : request.MonthId_From);

                    cmd.Parameters.AddWithValue("@MonthId_To",
                        request.MonthId_To == 0 ? (object)DBNull.Value : request.MonthId_To);

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
                apiResponse.data = new ProfitLossGridResponse
                {
                    LineItems = lineItems,
                    Summary = summary
                };
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
        [Route("DownloadProfitLossReport_Pdf")]
        public IActionResult DownloadProfitLossReport_Pdf([FromBody] ProfitLossReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    /* ── Company Header ── */

                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);


                    /* ── P&L REPORT ── */

                    cmd = new SqlCommand("Report_GetProfitLossDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@YearId",
                        request.YearId == 0 ? (object)DBNull.Value : request.YearId);

                    cmd.Parameters.AddWithValue("@Year",
                        request.Year == 0 ? (object)DBNull.Value : request.Year);

                    cmd.Parameters.AddWithValue("@MonthId_From",
                        request.MonthId_From == 0 ? (object)DBNull.Value : request.MonthId_From);

                    cmd.Parameters.AddWithValue("@MonthId_To",
                        request.MonthId_To == 0 ? (object)DBNull.Value : request.MonthId_To);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= ASSIGN & VALIDATION =================
                #region ASSIGN & VALIDATIONS
                string propertyName= string.Empty, companyName = string.Empty, companyAddress = string.Empty,
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

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data is incomplete. Required tables are missing.");

                // ================= EXTRACT DATA =================
                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                float topMargin = (request.WithHeader ?? false) ? 80f : 36f;

                using var stream = new MemoryStream();

                bool isA4size = true; // default to A4
                Document document = null;
                if (dsReport.Tables[2].Columns.Count > 7)
                {
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

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable summaryTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                /* ── Report Title ── */
                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                /* ── Data Table ── */
                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);

                BaseColor thColor = new BaseColor(240, 240, 240);
                BaseColor creditBg = new BaseColor(232, 245, 233);
                BaseColor debitBg = new BaseColor(255, 235, 238);

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
                            BorderColor = BaseColor.LIGHT_GRAY,
                            HorizontalAlignment = col.ColumnName == "Amount"
                                ? Element.ALIGN_RIGHT
                                : Element.ALIGN_LEFT
                        };

                        table.AddCell(cell);
                    }
                }

                document.Add(table);

                /* ── SUMMARY ── */
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow s = summaryTableDB.Rows[0];

                    decimal totalSales = Convert.ToDecimal(s["TotalSales"]);
                    decimal totalPurchase = Convert.ToDecimal(s["TotalPurchase"]);
                    decimal totalOtherIncome = Convert.ToDecimal(s["TotalOtherIncome"]);
                    decimal totalExpense = Convert.ToDecimal(s["TotalExpense"]);
                    decimal grossProfit = Convert.ToDecimal(s["GrossProfit"]);
                    decimal netProfit = Convert.ToDecimal(s["NetProfit"]);

                    document.Add(new Paragraph(" ") { SpacingAfter = 10f });

                    PdfPTable summaryTable = new PdfPTable(2);
                    summaryTable.WidthPercentage = 55;
                    summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;

                    void AddSummary(string label, decimal value)
                    {
                        summaryTable.AddCell(new PdfPCell(new Phrase(label)) { Padding = 6 });
                        summaryTable.AddCell(new PdfPCell(new Phrase(value.ToString("N2")))
                        {
                            Padding = 6,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        });
                    }

                    AddSummary("Total Sales", totalSales);
                    AddSummary("Total Other Income", totalOtherIncome);
                    AddSummary("Total Purchase", totalPurchase);
                    AddSummary("Total Expense", totalExpense);
                    AddSummary("Gross Profit", grossProfit);
                    AddSummary("Net Profit", netProfit);

                    document.Add(summaryTable);
                }

                document.Close();

                /* ── Save file ── */
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

                    SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    /* ── P&L REPORT ── */

                    cmd = new SqlCommand("Report_GetProfitLossDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@YearId",
                        request.YearId == 0 ? (object)DBNull.Value : request.YearId);

                    cmd.Parameters.AddWithValue("@Year",
                        request.Year == 0 ? (object)DBNull.Value : request.Year);

                    cmd.Parameters.AddWithValue("@MonthId_From",
                        request.MonthId_From == 0 ? (object)DBNull.Value : request.MonthId_From);

                    cmd.Parameters.AddWithValue("@MonthId_To",
                        request.MonthId_To == 0 ? (object)DBNull.Value : request.MonthId_To);

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

    }
}
