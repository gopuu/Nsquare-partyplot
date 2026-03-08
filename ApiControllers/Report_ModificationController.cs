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
    public class Report_ModificationController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_ModificationController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        #region REPORT - MODIFICATION REPORT

        [HttpPost]
        [Route("GetModificationGrid")]
        public IActionResult GetModificationGrid([FromBody] ModificationReportGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetModificationDetail_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    cmd.Parameters.AddWithValue("@Module",
                        string.IsNullOrEmpty(request.Module) ? (object)DBNull.Value : request.Module);
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
                    throw new Exception("Modification data incomplete.");

                DataTable dtLines = ds.Tables[0];
                DataTable dtSummary = ds.Tables[1];

                var lineItems = new List<ModificationLineItem>();
                foreach (DataRow row in dtLines.Rows)
                {
                    lineItems.Add(new ModificationLineItem
                    {
                        SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt64(row["SrNo"]) : 0,
                        ModDate = row["ModDate"] != DBNull.Value ? Convert.ToString(row["ModDate"]) : "-",
                        Module = row["Module"] != DBNull.Value ? Convert.ToString(row["Module"]) : "-",
                        RefNo = row["RefNo"] != DBNull.Value ? Convert.ToString(row["RefNo"]) : "-",
                        Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0
                    });
                }

                var summary = new ModificationSummary();
                if (dtSummary.Rows.Count > 0)
                {
                    DataRow s = dtSummary.Rows[0];
                    summary.TotalModifications = s["TotalModifications"] != DBNull.Value ? Convert.ToInt32(s["TotalModifications"]) : 0;
                    summary.SalesCount = s["SalesCount"] != DBNull.Value ? Convert.ToInt32(s["SalesCount"]) : 0;
                    summary.PurchaseCount = s["PurchaseCount"] != DBNull.Value ? Convert.ToInt32(s["PurchaseCount"]) : 0;
                    summary.ExpenseCount = s["ExpenseCount"] != DBNull.Value ? Convert.ToInt32(s["ExpenseCount"]) : 0;
                    summary.IncomeCount = s["IncomeCount"] != DBNull.Value ? Convert.ToInt32(s["IncomeCount"]) : 0;
                    summary.SalesAmount = s["SalesAmount"] != DBNull.Value ? Convert.ToDecimal(s["SalesAmount"]) : 0;
                    summary.PurchaseAmount = s["PurchaseAmount"] != DBNull.Value ? Convert.ToDecimal(s["PurchaseAmount"]) : 0;
                    summary.ExpenseAmount = s["ExpenseAmount"] != DBNull.Value ? Convert.ToDecimal(s["ExpenseAmount"]) : 0;
                    summary.IncomeAmount = s["IncomeAmount"] != DBNull.Value ? Convert.ToDecimal(s["IncomeAmount"]) : 0;
                    summary.TotalAmount = s["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(s["TotalAmount"]) : 0;
                }

                apiResponse.message = "Modification report fetched successfully!";
                apiResponse.data = new ModificationGridResponse { LineItems = lineItems, Summary = summary };
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
        [Route("DownloadModificationReport")]
        public IActionResult DownloadModificationReport([FromBody] ModificationReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

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

                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    cmd = new SqlCommand("Report_GetModificationDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns",
                        columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Module",
                        string.IsNullOrEmpty(request.Module) ? (object)DBNull.Value : request.Module);
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

                // ── Data table ──
                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                Font boldFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);
                BaseColor thColor = new BaseColor(0, 121, 182);

                // Module colour map
                Dictionary<string, BaseColor> moduleBg = new Dictionary<string, BaseColor>
                {
                    { "Sales",    new BaseColor(232, 245, 233) },   // green
                    { "Purchase", new BaseColor(255, 235, 238) },   // red
                    { "Expense",  new BaseColor(255, 248, 225) },   // yellow
                    { "Income",   new BaseColor(227, 242, 253) }    // blue
                };

                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 8f, 18f, 18f, 36f, 20f });

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    string mod = row["Module"]?.ToString() ?? "";
                    BaseColor rowBg = moduleBg.ContainsKey(mod) ? moduleBg[mod] : BaseColor.WHITE;

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string val = row[col]?.ToString() ?? "-";

                        PdfPCell cell = new PdfPCell(new Phrase(val, tdFont))
                        {
                            BackgroundColor = rowBg,
                            Padding = 6,
                            HorizontalAlignment = col.ColumnName == "Amount Impact"
                                ? Element.ALIGN_RIGHT
                                : Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                }

                // ── Totals row ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = summaryTableDB.Rows[0];

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = col.ColumnName switch
                        {
                            "Amount Impact" => $"{totalRow["TotalAmount"]}",
                            _ => col.Ordinal == 0 ? "TOTAL" : ""
                        };

                        table.AddCell(new PdfPCell(new Phrase(value, boldFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            Padding = 6
                        });
                    }
                }

                document.Add(table);

                // ── Per-module summary box ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow s = summaryTableDB.Rows[0];

                    document.Add(new Paragraph(" ") { SpacingAfter = 10f });

                    PdfPTable summaryTable = new PdfPTable(3);
                    summaryTable.WidthPercentage = 65;
                    summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    summaryTable.SetWidths(new float[] { 35f, 25f, 40f });

                    BaseColor summaryHeaderBg = new BaseColor(33, 37, 41);
                    Font summaryHeaderFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                    Font summaryLabelFont = new Font(Font.FontFamily.HELVETICA, 9);
                    Font summaryValueFont = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);

                    // Header row
                    foreach (string hdr in new[] { "Module", "Count", "Amount Impact" })
                    {
                        summaryTable.AddCell(new PdfPCell(new Phrase(hdr, summaryHeaderFont))
                        {
                            BackgroundColor = summaryHeaderBg,
                            Padding = 7,
                            HorizontalAlignment = Element.ALIGN_CENTER
                        });
                    }

                    void AddModuleRow(string module, string countKey, string amtKey, BaseColor bg)
                    {
                        summaryTable.AddCell(new PdfPCell(new Phrase(module, summaryLabelFont))
                        { BackgroundColor = bg, Padding = 6 });
                        summaryTable.AddCell(new PdfPCell(new Phrase(s[countKey].ToString(), summaryValueFont))
                        { BackgroundColor = bg, Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                        summaryTable.AddCell(new PdfPCell(new Phrase(
                                Convert.ToDecimal(s[amtKey]).ToString("N2"), summaryValueFont))
                        { BackgroundColor = bg, Padding = 6, HorizontalAlignment = Element.ALIGN_RIGHT });
                    }

                    AddModuleRow("Sales", "SalesCount", "SalesAmount", new BaseColor(232, 245, 233));
                    AddModuleRow("Purchase", "PurchaseCount", "PurchaseAmount", new BaseColor(255, 235, 238));
                    AddModuleRow("Expense", "ExpenseCount", "ExpenseAmount", new BaseColor(255, 248, 225));
                    AddModuleRow("Income", "IncomeCount", "IncomeAmount", new BaseColor(227, 242, 253));

                    // Divider
                    summaryTable.AddCell(new PdfPCell(new Phrase(" "))
                    {
                        Colspan = 3,
                        BackgroundColor = new BaseColor(200, 200, 200),
                        FixedHeight = 1f,
                        Border = Rectangle.NO_BORDER
                    });

                    // Total row
                    summaryTable.AddCell(new PdfPCell(new Phrase("TOTAL", boldFont))
                    { BackgroundColor = new BaseColor(240, 240, 240), Padding = 6 });
                    summaryTable.AddCell(new PdfPCell(new Phrase(s["TotalModifications"].ToString(), boldFont))
                    { BackgroundColor = new BaseColor(240, 240, 240), Padding = 6, HorizontalAlignment = Element.ALIGN_CENTER });
                    summaryTable.AddCell(new PdfPCell(new Phrase(
                            Convert.ToDecimal(s["TotalAmount"]).ToString("N2"), boldFont))
                    { BackgroundColor = new BaseColor(240, 240, 240), Padding = 6, HorizontalAlignment = Element.ALIGN_RIGHT });

                    document.Add(summaryTable);
                }

                document.Close();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Modification_Report_*.pdf");

                string fileName = $"Modification_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadModificationReport_Excel")]
        public IActionResult DownloadModificationReport_Excel([FromBody] ModificationReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetModificationDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns",
                        columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Module",
                        string.IsNullOrEmpty(request.Module) ? (object)DBNull.Value : request.Module);
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
                var worksheet = workbook.Worksheets.Add("Modification Report");

                // Module colour map
                var moduleColors = new Dictionary<string, XLColor>
        {
            { "Sales",    XLColor.FromArgb(232, 245, 233) },
            { "Purchase", XLColor.FromArgb(255, 235, 238) },
            { "Expense",  XLColor.FromArgb(255, 248, 225) },
            { "Income",   XLColor.FromArgb(227, 242, 253) }
        };

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
                    string module = dr["Module"]?.ToString() ?? "";
                    XLColor rowColor = moduleColors.ContainsKey(module)
                        ? moduleColors[module]
                        : XLColor.White;

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = dr[i]?.ToString() ?? "-";
                        cell.Style.Fill.BackgroundColor = rowColor;

                        if (dataTableDB.Columns[i].ColumnName == "Amount Impact")
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    row++;
                }

                // ── Totals row ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = summaryTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;
                        string value = columnName switch
                        {
                            "Amount Impact" => totalRow["TotalAmount"].ToString(),
                            _ => i == 0 ? "TOTAL" : ""
                        };

                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                    row += 2;

                    // ── Per-module summary ──
                    worksheet.Cell(row, 1).Value = "MODULE SUMMARY";
                    worksheet.Range(row, 1, row, 3).Merge();
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(33, 37, 41);
                    worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    row++;

                    // Sub-header
                    foreach (var (hdr, c) in new[] { ("Module", 1), ("Count", 2), ("Amount Impact", 3) })
                    {
                        worksheet.Cell(row, c).Value = hdr;
                        worksheet.Cell(row, c).Style.Font.Bold = true;
                        worksheet.Cell(row, c).Style.Fill.BackgroundColor = XLColor.FromArgb(33, 37, 41);
                        worksheet.Cell(row, c).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                    row++;

                    void AddModuleSummaryRow(string module, string countKey, string amtKey, XLColor bg)
                    {
                        worksheet.Cell(row, 1).Value = module;
                        worksheet.Cell(row, 1).Style.Fill.BackgroundColor = bg;

                        worksheet.Cell(row, 2).Value = totalRow[countKey].ToString();
                        worksheet.Cell(row, 2).Style.Fill.BackgroundColor = bg;
                        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Cell(row, 3).Value = Convert.ToDecimal(totalRow[amtKey]).ToString("N2");
                        worksheet.Cell(row, 3).Style.Fill.BackgroundColor = bg;
                        worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        row++;
                    }

                    AddModuleSummaryRow("Sales", "SalesCount", "SalesAmount", XLColor.FromArgb(232, 245, 233));
                    AddModuleSummaryRow("Purchase", "PurchaseCount", "PurchaseAmount", XLColor.FromArgb(255, 235, 238));
                    AddModuleSummaryRow("Expense", "ExpenseCount", "ExpenseAmount", XLColor.FromArgb(255, 248, 225));
                    AddModuleSummaryRow("Income", "IncomeCount", "IncomeAmount", XLColor.FromArgb(227, 242, 253));

                    // Grand total
                    worksheet.Cell(row, 1).Value = "TOTAL";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;

                    worksheet.Cell(row, 2).Value = totalRow["TotalModifications"].ToString();
                    worksheet.Cell(row, 2).Style.Font.Bold = true;
                    worksheet.Cell(row, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    worksheet.Cell(row, 3).Value = Convert.ToDecimal(totalRow["TotalAmount"]).ToString("N2");
                    worksheet.Cell(row, 3).Style.Font.Bold = true;
                    worksheet.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
                    worksheet.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                worksheet.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Modification_Report_*.xlsx");

                string fileName = $"Modification_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "Modification Report Excel generated successfully.";
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
