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
    public class Report_EnquiryDealsController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_EnquiryDealsController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetEnquiryDealsReportGrid")]
        public IActionResult GetEnquiryDealsReportGrid([FromBody] EnquiryDealsReportRequestGRID request)
        {
            apiResponse apiResponse = new apiResponse();
            List<EnquiryDealsReportGridModel> enquiries = new List<EnquiryDealsReportGridModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetEnquiryDeals_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@EnquiryFromDate", string.IsNullOrEmpty(request.EnquiryFromDate) ? (object)DBNull.Value : request.EnquiryFromDate);
                    cmd.Parameters.AddWithValue("@EnquiryToDate", string.IsNullOrEmpty(request.EnquiryToDate) ? (object)DBNull.Value : request.EnquiryToDate);
                    cmd.Parameters.AddWithValue("@EnquiryMonthDuration", string.IsNullOrEmpty(request.EnquiryMonthDuration) ? (object)DBNull.Value : request.EnquiryMonthDuration);

                    cmd.Parameters.AddWithValue("@EventFromDate", string.IsNullOrEmpty(request.EventFromDate) ? (object)DBNull.Value : request.EventFromDate);
                    cmd.Parameters.AddWithValue("@EventToDate", string.IsNullOrEmpty(request.EventToDate) ? (object)DBNull.Value : request.EventToDate);

                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);

                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);

                    cmd.Parameters.AddWithValue("@FromPax", request.FromPax);
                    cmd.Parameters.AddWithValue("@ToPax", request.ToPax);

                    cmd.Parameters.AddWithValue("@LeadSourceId", request.LeadSourceId);
                    cmd.Parameters.AddWithValue("@ProcessId", request.ProcessId);

                    cmd.Parameters.AddWithValue("@FromAmount", request.FromAmount);
                    cmd.Parameters.AddWithValue("@ToAmount", request.ToAmount);

                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    enquiries.Add(new EnquiryDealsReportGridModel
                    {
                        EnquiryId = Convert.ToInt64(row["EnquiryId"]),

                        EnquiryDate = Convert.ToString(row["EnquiryDate"]),
                        EventDate = Convert.ToString(row["EventDate"]),

                        ClientName = Convert.ToString(row["ClientName"]),

                        EventType_SubType = Convert.ToString(row["EventType_SubType"]),

                        Hall = Convert.ToString(row["Hall"]),

                        Pax = row["Pax"] != DBNull.Value
                            ? Convert.ToInt32(row["Pax"])
                            : 0,

                        LeadSource = Convert.ToString(row["LeadSource"]),

                        Amount = row["Amount"] != DBNull.Value
                            ? Convert.ToDecimal(row["Amount"])
                            : 0,

                        FollowUps = row["FollowUps"] != DBNull.Value
                            ? Convert.ToInt32(row["FollowUps"])
                            : 0,

                        Status = Convert.ToString(row["Status"])
                    });
                }

                apiResponse.message = "Enquiry report fetched successfully!";
                apiResponse.data = enquiries;
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
        [Route("DownloadEnquiryDealsReport")]
        public IActionResult DownloadEnquiryDealsReport([FromBody] EnquiryDealsReportRequest request)
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

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    cmd = new SqlCommand("Report_GetEnquiryDeals_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EnquiryFromDate", string.IsNullOrEmpty(request.EnquiryFromDate) ? (object)DBNull.Value : request.EnquiryFromDate);
                    cmd.Parameters.AddWithValue("@EnquiryToDate", string.IsNullOrEmpty(request.EnquiryToDate) ? (object)DBNull.Value : request.EnquiryToDate);
                    cmd.Parameters.AddWithValue("@EnquiryMonthDuration", string.IsNullOrEmpty(request.EnquiryMonthDuration) ? (object)DBNull.Value : request.EnquiryMonthDuration);

                    cmd.Parameters.AddWithValue("@EventFromDate", string.IsNullOrEmpty(request.EventFromDate) ? (object)DBNull.Value : request.EventFromDate);
                    cmd.Parameters.AddWithValue("@EventToDate", string.IsNullOrEmpty(request.EventToDate) ? (object)DBNull.Value : request.EventToDate);

                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);

                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);

                    cmd.Parameters.AddWithValue("@FromPax", request.FromPax);
                    cmd.Parameters.AddWithValue("@ToPax", request.ToPax);

                    cmd.Parameters.AddWithValue("@LeadSourceId", request.LeadSourceId);
                    cmd.Parameters.AddWithValue("@ProcessId", request.ProcessId);

                    cmd.Parameters.AddWithValue("@FromAmount", request.FromAmount);
                    cmd.Parameters.AddWithValue("@ToAmount", request.ToAmount);

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
                        case "enq. no.":
                        case "enq. date":
                        case "event date":
                        case "pax":
                        case "follow-ups":
                            return Element.ALIGN_CENTER;

                        case "amount":
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
                            col.ColumnName.Equals("Amount", StringComparison.OrdinalIgnoreCase);

                        string value = row[col]?.ToString() ?? "-";

                        if (isAmountColumn &&
                            row[col] != DBNull.Value &&
                            decimal.TryParse(Convert.ToString(row[col]), out decimal amount))
                        {
                            value = "₹ " + amount.ToString("N2");
                        }

                        table.AddCell(
                            Utils.CreateBodyCell(
                                value,
                                isAmountColumn ? AmtFontNormal : tdFont,
                                GetAlignment(col.ColumnName)
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
                            case "Enq. No.":
                                value = "TOTAL : " + Convert.ToInt32(totalRow["TotalEnquiries"]);
                                break;

                            case "Pax":
                                value = Convert.ToDecimal(totalRow["TotalPax"]).ToString("N0");
                                break;

                            case "Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Follow-ups":
                                value = Convert.ToInt32(totalRow["TotalFollowUps"]).ToString();
                                break;

                            default:
                                value = "";
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

                string fileName = $"Enquiry_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Enquiry_Report_*.pdf");

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
        [Route("DownloadEnquiryDealsReport_Excel")]
        public IActionResult DownloadEnquiryDealsReport_Excel([FromBody] EnquiryDealsReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_GetEnquiryDeals_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // SAME LOGIC AS PDF
                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EnquiryFromDate", string.IsNullOrEmpty(request.EnquiryFromDate) ? (object)DBNull.Value : request.EnquiryFromDate);
                    cmd.Parameters.AddWithValue("@EnquiryToDate", string.IsNullOrEmpty(request.EnquiryToDate) ? (object)DBNull.Value : request.EnquiryToDate);
                    cmd.Parameters.AddWithValue("@EnquiryMonthDuration", string.IsNullOrEmpty(request.EnquiryMonthDuration) ? (object)DBNull.Value : request.EnquiryMonthDuration);

                    cmd.Parameters.AddWithValue("@EventFromDate", string.IsNullOrEmpty(request.EventFromDate) ? (object)DBNull.Value : request.EventFromDate);
                    cmd.Parameters.AddWithValue("@EventToDate", string.IsNullOrEmpty(request.EventToDate) ? (object)DBNull.Value : request.EventToDate);

                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);

                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);

                    cmd.Parameters.AddWithValue("@FromPax", request.FromPax);
                    cmd.Parameters.AddWithValue("@ToPax", request.ToPax);

                    cmd.Parameters.AddWithValue("@LeadSourceId", request.LeadSourceId);
                    cmd.Parameters.AddWithValue("@ProcessId", request.ProcessId);

                    cmd.Parameters.AddWithValue("@FromAmount", request.FromAmount);
                    cmd.Parameters.AddWithValue("@ToAmount", request.ToAmount);


                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("EnquiryReport");

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
                            case "Enq. No.":
                                value = "TOTAL : " + Convert.ToInt32(totalRow["TotalEnquiries"]);
                                break;

                            case "Pax":
                                value = Convert.ToDecimal(totalRow["TotalPax"]).ToString("N0");
                                break;

                            case "Amount":
                                value = Convert.ToDecimal(totalRow["TotalAmount"]).ToString("N2");
                                break;

                            case "Follow-ups":
                                value = Convert.ToInt32(totalRow["TotalFollowUps"]).ToString();
                                break;

                            default:
                                value = "";
                                break;
                        }

                        var cell = worksheet.Cell(row, i + 1);

                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                        // Right align numeric values
                        if (columnName == "Amount")
                        {
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        // Center align count values
                        else if (columnName == "Enq. No."
                              || columnName == "Pax"
                              || columnName == "Follow-ups")
                        {
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                string reportsDir = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(reportsDir))
                    Directory.CreateDirectory(reportsDir);

                Utils.deleteOldFiles(reportsDir, "EnquiryReport_*.xlsx");

                string fileName = $"EnquiryReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(reportsDir, fileName);

                workbook.SaveAs(filePath);

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Enquiry Report Excel generated successfully.";
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
