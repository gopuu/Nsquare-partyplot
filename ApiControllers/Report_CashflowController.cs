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
    public class Report_CashflowController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_CashflowController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetCashflowGrid")]
        public IActionResult GetCashflowGrid([FromBody] CashflowReportRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetCashflowDetail_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

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
                    throw new Exception("Cashflow data incomplete.");

                DataTable dtLines = ds.Tables[0];
                DataTable dtSummary = ds.Tables[1];

                var lineItems = new List<CashflowLineItem>();
                foreach (DataRow row in dtLines.Rows)
                {
                    lineItems.Add(new CashflowLineItem
                    {
                        SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt64(row["SrNo"]) : 0,
                        TxnDate = row["TxnDate"] != DBNull.Value ? Convert.ToString(row["TxnDate"]) : "-",
                        FlowType = row["FlowType"] != DBNull.Value ? Convert.ToString(row["FlowType"]) : "-",
                        Description = row["Description"] != DBNull.Value ? Convert.ToString(row["Description"]) : "-",
                        Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0
                    });
                }

                var summary = new CashflowSummary();
                if (dtSummary.Rows.Count > 0)
                {
                    DataRow s = dtSummary.Rows[0];
                    summary.TotalInflow = s["TotalInflow"] != DBNull.Value ? Convert.ToDecimal(s["TotalInflow"]) : 0;
                    summary.TotalOutflow = s["TotalOutflow"] != DBNull.Value ? Convert.ToDecimal(s["TotalOutflow"]) : 0;
                    summary.NetCashflow = s["NetCashflow"] != DBNull.Value ? Convert.ToDecimal(s["NetCashflow"]) : 0;
                }

                apiResponse.message = "Cashflow report fetched successfully!";
                apiResponse.data = new CashflowGridResponse { LineItems = lineItems, Summary = summary };
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
        [Route("DownloadCashflowReport")]
        public IActionResult DownloadCashflowReport([FromBody] CashflowReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    cmd = new SqlCommand("Report_GetCashflowDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable summaryTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                // ================= ASSIGN & VALIDATION =================
                #region ASSIGN & VALIDATIONS
                string companyName = string.Empty, companyAddress = string.Empty,
                       companyphoneNo = string.Empty, companygstin = "", companypan = "",
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
                        companyName = dtCompany.Rows[0]["CompanyName"]?.ToString() ?? "";
                        companyAddress = dtCompany.Rows[0]["CompanyAddress"]?.ToString() ?? "";
                        companyphoneNo = dtCompany.Rows[0]["CompanyMobile"]?.ToString() ?? "";
                        logoFile = dtCompany.Rows[0]["CompanyLogo"]?.ToString() ?? "";
                        companygstin = dtCompany.Rows[0]["CompanyGST"]?.ToString();
                        companypan = dtCompany.Rows[0]["CompanyPAN"]?.ToString();
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
                            companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companypan,
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
                            companyName,
                            companyAddress,
                            companyphoneNo,
                            companygstin,
                            companypan,
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
                            "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                    else
                    {
                        writer.PageEvent = new PdfHeaderFooter_StandardReport_A4_Rotate(
                            "", "", "", "", "", "", "",
                            generatedDate,
                            userName,
                            request.WithHeader
                        );
                    }
                }
                #endregion

                document.Open();

                // ── Report title ──
                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                // ── Data table ──
                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                Font boldFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
                BaseColor thColor = new BaseColor(240, 240, 240);
                BaseColor inflowBg = new BaseColor(232, 245, 233);
                BaseColor outflowBg = new BaseColor(255, 235, 238);

                PdfPTable table = new PdfPTable(5);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 8f, 18f, 16f, 38f, 20f });

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    string flowType = row["FlowType"]?.ToString() ?? "";
                    BaseColor rowBg = flowType == "Inflow" ? inflowBg : outflowBg;

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

                // ── Summary box ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow s = summaryTableDB.Rows[0];

                    decimal totalInflow = s["TotalInflow"] != DBNull.Value ? Convert.ToDecimal(s["TotalInflow"]) : 0;
                    decimal totalOutflow = s["TotalOutflow"] != DBNull.Value ? Convert.ToDecimal(s["TotalOutflow"]) : 0;
                    decimal netCashflow = s["NetCashflow"] != DBNull.Value ? Convert.ToDecimal(s["NetCashflow"]) : 0;

                    document.Add(new Paragraph(" ") { SpacingAfter = 10f });

                    PdfPTable summaryTable = new PdfPTable(2);
                    summaryTable.WidthPercentage = 50;
                    summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    summaryTable.SetWidths(new float[] { 60f, 40f });

                    BaseColor summaryHeaderBg = new BaseColor(33, 37, 41);
                    Font summaryHeaderFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                    Font summaryLabelFont = new Font(Font.FontFamily.HELVETICA, 9);
                    Font summaryValueFont = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);

                    // Header
                    PdfPCell hCell = new PdfPCell(new Phrase("CASHFLOW SUMMARY", summaryHeaderFont))
                    {
                        Colspan = 2,
                        BackgroundColor = summaryHeaderBg,
                        Padding = 8,
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    summaryTable.AddCell(hCell);

                    void AddRow(string label, decimal value, BaseColor bg, bool isTotal = false)
                    {
                        Font lf = isTotal
                            ? new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD)
                            : summaryLabelFont;
                        Font vf = isTotal
                            ? new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD,
                                  value >= 0 ? new BaseColor(27, 94, 32) : new BaseColor(183, 28, 28))
                            : summaryValueFont;

                        summaryTable.AddCell(new PdfPCell(new Phrase(label, lf))
                        {
                            BackgroundColor = bg,
                            Padding = 6,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        });
                        summaryTable.AddCell(new PdfPCell(new Phrase(value.ToString("N2"), vf))
                        {
                            BackgroundColor = bg,
                            Padding = 6,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        });
                    }

                    AddRow("Total Inflow", totalInflow, new BaseColor(232, 245, 233));
                    AddRow("Total Outflow", totalOutflow, new BaseColor(255, 235, 238));

                    // Divider
                    summaryTable.AddCell(new PdfPCell(new Phrase(" "))
                    {
                        Colspan = 2,
                        BackgroundColor = new BaseColor(200, 200, 200),
                        FixedHeight = 1f,
                        Border = Rectangle.NO_BORDER
                    });

                    AddRow("Net Cashflow", netCashflow, new BaseColor(240, 240, 240), isTotal: true);

                    document.Add(summaryTable);
                }

                document.Close();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Cashflow_Report_*.pdf");

                string fileName = $"Cashflow_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                using (FileStream fs = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
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
        [Route("DownloadCashflowReport_Excel")]
        public IActionResult DownloadCashflowReport_Excel([FromBody] CashflowReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetCashflowDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;
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
                var worksheet = workbook.Worksheets.Add("Cashflow Report");

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
                    string flowType = dr["FlowType"]?.ToString() ?? "";

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = dr[i]?.ToString() ?? "-";

                        cell.Style.Fill.BackgroundColor = flowType == "Inflow"
                            ? XLColor.FromArgb(232, 245, 233)
                            : XLColor.FromArgb(255, 235, 238);

                        if (dataTableDB.Columns[i].ColumnName == "Amount")
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    row++;
                }

                // ── Summary section ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow s = summaryTableDB.Rows[0];
                    row += 1;

                    // Title
                    worksheet.Cell(row, 1).Value = "CASHFLOW SUMMARY";
                    worksheet.Range(row, 1, row, 5).Merge();
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(33, 37, 41);
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row++;

                    void AddSummaryRow(string label, string value, XLColor bg, bool bold = false)
                    {
                        worksheet.Cell(row, 1).Value = label;
                        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = bg;
                        if (bold) worksheet.Cell(row, 1).Style.Font.Bold = true;

                        worksheet.Cell(row, 2).Value = value;
                        worksheet.Cell(row, 2).Style.Fill.BackgroundColor = bg;
                        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        if (bold) worksheet.Cell(row, 2).Style.Font.Bold = true;
                        row++;
                    }

                    AddSummaryRow("Total Inflow", s["TotalInflow"].ToString(), XLColor.FromArgb(232, 245, 233));
                    AddSummaryRow("Total Outflow", s["TotalOutflow"].ToString(), XLColor.FromArgb(255, 235, 238));
                    AddSummaryRow("Net Cashflow", s["NetCashflow"].ToString(), XLColor.FromArgb(240, 240, 240), bold: true);
                }

                worksheet.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Cashflow_Report_*.xlsx");

                string fileName = $"Cashflow_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Cashflow Report Excel generated successfully.";
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
