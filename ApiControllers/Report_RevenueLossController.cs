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
    public class Report_RevenueLossController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_RevenueLossController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetRevenueLossGrid")]
        public IActionResult GetRevenueLossGrid([FromBody] RevenueLossGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<RevenueLossGridModel> rows = new List<RevenueLossGridModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetRevenueLoss_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId ?? 0);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId ?? 0);
                    cmd.Parameters.AddWithValue("@LeadSourceId", request.LeadSourceId ?? 0);

                    con.Open();

                    using SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        rows.Add(new RevenueLossGridModel
                        {
                            SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt64(row["SrNo"]) : 0,
                            EnquiryDealId = row["EnquiryDealId"] != DBNull.Value ? Convert.ToInt64(row["EnquiryDealId"]) : 0,
                            EventDate = row["EventDate"] != DBNull.Value ? Convert.ToString(row["EventDate"]) : "-",
                            GuestName = row["GuestName"] != DBNull.Value ? Convert.ToString(row["GuestName"]) : "-",
                            EventType = row["EventType"] != DBNull.Value ? Convert.ToString(row["EventType"]) : "-",
                            EventSubType = row["EventSubType"] != DBNull.Value ? Convert.ToString(row["EventSubType"]) : "-",
                            LeadSource = row["LeadSource"] != DBNull.Value ? Convert.ToString(row["LeadSource"]) : "-",
                            EventDisplay = row["EventDisplay"] != DBNull.Value ? Convert.ToString(row["EventDisplay"]) : "-",
                            EnquiryDateTime = row["EnquiryDateTime"] != DBNull.Value ? Convert.ToString(row["EnquiryDateTime"]) : "-",
                            LossAmount = row["LossAmount"] != DBNull.Value ? Convert.ToDecimal(row["LossAmount"]) : 0
                        });
                    }
                }

                object totals = null;
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow t = ds.Tables[1].Rows[0];
                    totals = new
                    {
                        TotalLeadLost = t["TotalLeadLost"] != DBNull.Value ? Convert.ToInt64(t["TotalLeadLost"]) : 0,
                        TotalLossAmount = t["TotalLossAmount"] != DBNull.Value ? Convert.ToDecimal(t["TotalLossAmount"]) : 0
                    };
                }

                apiResponse.message = "Revenue loss report fetched successfully!";
                apiResponse.message = "Revenue loss report fetched successfully!";
                apiResponse.data = new
                {
                    Rows = rows,
                    Totals = totals
                };
                apiResponse.status = "Success";
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
        [Route("DownloadRevenueLossReport")]
        public IActionResult DownloadRevenueLossReport([FromBody] RevenueLossReportRequest request)
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

                    // Company
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // Report
                    cmd = new SqlCommand("Report_GetRevenueLoss_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId ?? 0);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId ?? 0);
                    cmd.Parameters.AddWithValue("@LeadSourceId", request.LeadSourceId ?? 0);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ── Company info ──
                string companyName = string.Empty, companyAddress = string.Empty,
                       companyphoneNo = string.Empty, logoFile = string.Empty, userName = string.Empty;
                Image logo = null;

                if (request.WithHeader ?? false)
                {
                    bool isCompanyAvailable = dtCompany != null && dtCompany.Rows.Count > 0;

                    if (isCompanyAvailable)
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
                    throw new Exception("Report data is incomplete. Required tables are missing.");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

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
                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows) {
                    foreach (DataColumn col in dataTableDB.Columns) {
                        ///table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));

                        string colName = col.ColumnName.Trim().ToLower();
                        int align = colName switch
                        {
                            "sr. no." => Element.ALIGN_CENTER,
                            "date" => Element.ALIGN_CENTER,
                            "guest name" => Element.ALIGN_LEFT,
                            "event" => Element.ALIGN_LEFT,
                            "sub-event" => Element.ALIGN_LEFT,
                            "lead source" => Element.ALIGN_LEFT,
                            "enquiry datetime" => Element.ALIGN_LEFT,
                            _ => Element.ALIGN_RIGHT
                        };

                        table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont, align));
                    }
                }

                // ── Totals row ──
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = col.ColumnName switch
                        {
                            "Loss Amount" => $"{totalRow["TotalLossAmount"]}",
                            _ => col.Ordinal == 0 ? "TOTAL" : ""
                        };

                        table.AddCell(new PdfPCell(new Phrase(value, totalFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            Padding = 6,
                            BorderColor = new BaseColor(220, 220, 220), // light gray
                            BorderWidth = 0.5f
                        });
                    }
                }

                document.Add(table);
                document.Close();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "RevenueLoss_Report_*.pdf");

                string fileName = $"RevenueLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);

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
        [Route("DownloadRevenueLossReport_Excel")]
        public IActionResult DownloadRevenueLossReport_Excel([FromBody] RevenueLossReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetRevenueLoss_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId ?? 0);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId ?? 0);
                    cmd.Parameters.AddWithValue("@LeadSourceId", request.LeadSourceId ?? 0);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Revenue Loss");

                // ── Headers ──
                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // ── Data ──
                int row = 2;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    row++;
                }

                // ── Totals row ──
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string value = columnName switch
                        {
                            "Loss Amount" => totalRow["TotalLossAmount"].ToString(),
                            _ => i == 0 ? "TOTAL" : ""
                        };

                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                }

                worksheet.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "RevenueLoss_Report_*.xlsx");

                string fileName = $"RevenueLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);
                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Revenue Loss Excel generated successfully.";
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
