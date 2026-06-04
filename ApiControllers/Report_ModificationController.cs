using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
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
using System.Data;
using System.Data.SqlClient;

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

                    cmd.Parameters.AddWithValue("@ModuleId", request.ModuleId > 0 ? request.ModuleId : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

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
                        ModuleName = row["ModuleName"]?.ToString() ?? "-",
                        RowId = row["RowId"] != DBNull.Value ? Convert.ToInt64(row["RowId"]) : 0,
                        ReferenceNo = row["ReferenceNo"]?.ToString() ?? "-",
                        OldAmount = row["OldAmount"] != DBNull.Value ? Convert.ToDecimal(row["OldAmount"]) : 0,
                        NewAmount = row["NewAmount"] != DBNull.Value ? Convert.ToDecimal(row["NewAmount"]) : 0,
                        ImpactAmount = row["ImpactAmount"] != DBNull.Value ? Convert.ToDecimal(row["ImpactAmount"]) : 0,
                        ModifiedDateTime = row["ModifiedDateTime"]?.ToString() ?? "-",
                        ModifiedDateTime_Format = row["ModifiedDateTime_Format"]?.ToString() ?? "-",
                        ModifiedBy = row["ModifiedBy"]?.ToString() ?? "-"
                    });
                }

                var summary = new ModificationSummary();
                if (dtSummary.Rows.Count > 0)
                {
                    DataRow s = dtSummary.Rows[0];
                    summary.TotalModifications = s["TotalModifications"] != DBNull.Value ? Convert.ToInt32(s["TotalModifications"]) : 0;
                    summary.TotalOldAmount = s["TotalOldAmount"] != DBNull.Value ? Convert.ToDecimal(s["TotalOldAmount"]) : 0;
                    summary.TotalNewAmount = s["TotalNewAmount"] != DBNull.Value ? Convert.ToDecimal(s["TotalNewAmount"]) : 0;
                    summary.TotalImpactAmount = s["TotalImpactAmount"] != DBNull.Value ? Convert.ToDecimal(s["TotalImpactAmount"]) : 0;
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
        public IActionResult DownloadModificationReport([FromBody] ModificationReportRequest_PdfExcel request)
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

                    cmd = new SqlCommand("Report_GetModificationDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 120;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModuleId", request.ModuleId.HasValue && request.ModuleId.Value > 0
                            ? request.ModuleId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

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
                string reportName = "";
                if (dsReport.Tables.Count > 0 && dsReport.Tables[0].Rows.Count > 0)
                {
                    reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";
                }

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

                // ── Report title ──
                document.Add(new Paragraph(reportName.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                // ── Data table ──
                iTextSharp.text.Font thFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
                iTextSharp.text.Font tdFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8);
                iTextSharp.text.Font boldFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.BOLD);
                BaseColor thColor = new BaseColor(240, 240, 240);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD);

                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                if (columnCount == 9)
                {
                    table.SetWidths(new float[] { 6f,15f,10f,18f,12f,12f,12f,20f,15f });
                }
                else
                {
                    table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());
                }

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    BaseColor rowBg = BaseColor.WHITE;
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        //string val = row[col]?.ToString() ?? "-";
                        string value = "";
                        switch (col.ColumnName)
                        {
                            case "Old Amount":
                                value = "₹ " + Convert.ToDecimal(row["Old Amount"]).ToString("N2");
                                break;

                            case "New Amount":
                                value = "₹ " + Convert.ToDecimal(row["New Amount"]).ToString("N2");
                                break;

                            case "Impact Amount":
                                value = "₹ " + Convert.ToDecimal(row["Impact Amount"]).ToString("N2");
                                break;

                            default: 
                                value = row[col]?.ToString() ?? "-";
                                break;
                        }

                        iTextSharp.text.Font cellFont =
                        (
                            col.ColumnName == "Old Amount" ||
                            col.ColumnName == "New Amount" ||
                            col.ColumnName == "Impact Amount"
                        )
                        ? AmtFontNormal
                        : tdFont;

                        PdfPCell cell = new PdfPCell(new Phrase(value, cellFont))
                        {
                            BackgroundColor = rowBg,
                            Padding = 6,
                            BorderColor = BaseColor.LIGHT_GRAY,
                            HorizontalAlignment =
                            (
                                col.ColumnName == "Old Amount" ||
                                col.ColumnName == "New Amount" ||
                                col.ColumnName == "Impact Amount"
                            )
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
                        string value = "";

                        switch (col.ColumnName)
                        {
                            case "Sr. No.":
                                value = "TOTAL";
                                break;

                            case "Old Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalOldAmount"]).ToString("N2");
                                break;

                            case "New Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalNewAmount"]).ToString("N2");
                                break;

                            case "Impact Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalImpactAmount"]).ToString("N2");
                                break;
                        }

                        iTextSharp.text.Font cellFont =
                        (
                            col.ColumnName == "Old Amount" ||
                            col.ColumnName == "New Amount" ||
                            col.ColumnName == "Impact Amount"
                        )
                        ? AmtFontBold
                        : boldFont;

                        table.AddCell(new PdfPCell(new Phrase(value, cellFont))
                        {
                            BackgroundColor = new BaseColor(240, 240, 240),
                            BorderColor = BaseColor.LIGHT_GRAY,
                            Padding = 6,
                            HorizontalAlignment =
                            (
                                col.ColumnName == "Old Amount" ||
                                col.ColumnName == "New Amount" ||
                                col.ColumnName == "Impact Amount"
                            )
                            ? Element.ALIGN_RIGHT
                            : Element.ALIGN_LEFT
                        });
                    }
                }

                document.Add(table);

                // ── Per-module summary box ──
                if (summaryTableDB != null && summaryTableDB.Rows.Count > 0)
                {
                    DataRow s = summaryTableDB.Rows[0];

                    document.Add(new Paragraph(" ") { SpacingAfter = 10f });

                    PdfPTable summaryTable = new PdfPTable(2);
                    summaryTable.WidthPercentage = 45;
                    summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                    summaryTable.SetWidths(new float[] { 60f, 40f });

                    void AddSummaryRow(string label, string value, bool isAmount = false)
                    {
                        summaryTable.AddCell(new PdfPCell(new Phrase(label, tdFont))
                        {
                            Padding = 6
                        });

                        summaryTable.AddCell(new PdfPCell(
                            new Phrase(value, isAmount ? AmtFontBold : boldFont))
                        {
                            Padding = 6,
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        });
                    }

                    AddSummaryRow(
                        "Total Modifications",
                        s["TotalModifications"].ToString());

                    AddSummaryRow(
                        "Total Old Amount",
                        "₹ " + Convert.ToDecimal(s["TotalOldAmount"]).ToString("N2"),
                        true);

                    AddSummaryRow(
                        "Total New Amount",
                        "₹ " + Convert.ToDecimal(s["TotalNewAmount"]).ToString("N2"),
                        true);

                    AddSummaryRow(
                        "Total Impact Amount",
                        "₹ " + Convert.ToDecimal(s["TotalImpactAmount"]).ToString("N2"),
                        true);
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
        public IActionResult DownloadModificationReport_Excel([FromBody] ModificationReportRequest_PdfExcel request)
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

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ModuleId", request.ModuleId > 0 ? request.ModuleId : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable summaryTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Modification Report");

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
                XLColor rowColor = XLColor.White;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = dr[i]?.ToString() ?? "-";
                        cell.Style.Fill.BackgroundColor = rowColor;

                        if (
                            dataTableDB.Columns[i].ColumnName == "Old Amount" ||
                            dataTableDB.Columns[i].ColumnName == "New Amount" ||
                            dataTableDB.Columns[i].ColumnName == "Impact Amount"
                        )
                        {
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
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

                        string value = "";
                        switch (columnName)
                        {
                            case "Sr. No.":
                                value = "TOTAL";
                                break;

                            case "Old Amount":
                                value = Convert.ToDecimal(totalRow["TotalOldAmount"]).ToString("N2");
                                break;

                            case "New Amount":
                                value = Convert.ToDecimal(totalRow["TotalNewAmount"]).ToString("N2");
                                break;

                            case "Impact Amount":
                                value = Convert.ToDecimal(totalRow["TotalImpactAmount"]).ToString("N2");
                                break;
                        }

                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        cell.Style.Alignment.Horizontal =
                            (
                                columnName == "Old Amount" ||
                                columnName == "New Amount" ||
                                columnName == "Impact Amount"
                            )
                            ? XLAlignmentHorizontalValues.Right
                            : XLAlignmentHorizontalValues.Left;
                    }
                    row += 2;

                    worksheet.Cell(row, 1).Value = "SUMMARY";
                    worksheet.Range(row, 1, row, 2).Merge();
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(33, 37, 41);
                    worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                    row++;

                    void AddSummary(string label, string value)
                    {
                        worksheet.Cell(row, 1).Value = label;
                        worksheet.Cell(row, 1).Style.Font.Bold = true;

                        worksheet.Cell(row, 2).Value = value;
                        worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        row++;
                    }

                    AddSummary("Total Modifications", totalRow["TotalModifications"].ToString());
                    AddSummary("Total Old Amount", Convert.ToDecimal(totalRow["TotalOldAmount"]).ToString("N2"));
                    AddSummary("Total New Amount", Convert.ToDecimal(totalRow["TotalNewAmount"]).ToString("N2"));
                    AddSummary("Total Impact Amount", Convert.ToDecimal(totalRow["TotalImpactAmount"]).ToString("N2"));

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

        [HttpGet]
        [Route("GetModuleMasterForModificationReport")]
        public IActionResult GetModuleMasterForModificationReport()
        {
            apiResponse apiResponse = new apiResponse();
            List<GetModuleMasterForModificationReportResponse> list =
                new List<GetModuleMasterForModificationReportResponse>();

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

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetModuleMaster_ForModificationReport", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new GetModuleMasterForModificationReportResponse
                    {
                        Id = dr["Id"] == DBNull.Value
                            ? 0
                            : Convert.ToInt64(dr["Id"]),

                        Name = dr["Name"] == DBNull.Value
                            ? string.Empty
                            : Convert.ToString(dr["Name"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Modules fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;

                return StatusCode(500, apiResponse);
            }
        }

    }
}
