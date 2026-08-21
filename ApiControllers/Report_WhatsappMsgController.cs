using ClosedXML.Excel;
//using DocumentFormat.OpenXml.Office.Word;
//using DocumentFormat.OpenXml.Office2016.Excel;
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
    public class Report_WhatsappMsgController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_WhatsappMsgController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetWhatsAppMessageLogReportGrid")]
        public IActionResult GetWhatsAppMessageLogReportGrid([FromBody] WhatsAppMessageLogReportGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<WhatsAppMessageLogReportGridModel> rows = new List<WhatsAppMessageLogReportGridModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("WhatsApp_MessageLog_GetReport_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(request.Status) ? (object)DBNull.Value : request.Status);
                    cmd.Parameters.AddWithValue("@CompanyId", request.CompanyId == 0 ? 0 : request.CompanyId);
                    cmd.Parameters.AddWithValue("@MobileNo", string.IsNullOrEmpty(request.MobileNo) ? (object)DBNull.Value : request.MobileNo);
                    cmd.Parameters.AddWithValue("@EventId", request.EventId == 0 ? 0 : request.EventId);
                    cmd.Parameters.AddWithValue("@Search", string.IsNullOrEmpty(request.Search) ? (object)DBNull.Value : request.Search);

                    con.Open();

                    using SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    rows.Add(new WhatsAppMessageLogReportGridModel
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
                        CompanyId = row["CompanyId"] != DBNull.Value ? Convert.ToInt64(row["CompanyId"]) : 0,
                        MobileNo = row["MobileNo"] != DBNull.Value ? Convert.ToString(row["MobileNo"]) : "-",
                        TemplateName = row["TemplateName"] != DBNull.Value ? Convert.ToString(row["TemplateName"]) : "-",
                        MsgTemplate = row["MsgTemplate"] != DBNull.Value ? Convert.ToString(row["MsgTemplate"]) : "-",
                        VariablesJson = row["VariablesJson"] != DBNull.Value ? Convert.ToString(row["VariablesJson"]) : "-",
                        MsgSample = row["MsgSample"] != DBNull.Value ? Convert.ToString(row["MsgSample"]) : "-",
                        Status = row["Status"] != DBNull.Value ? Convert.ToString(row["Status"]) : "-",
                        ProviderMessageId = row["ProviderMessageId"] != DBNull.Value ? Convert.ToString(row["ProviderMessageId"]) : "-",
                        ProviderResponse = row["ProviderResponse"] != DBNull.Value ? Convert.ToString(row["ProviderResponse"]) : "-",
                        ErrorMessage = row["ErrorMessage"] != DBNull.Value ? Convert.ToString(row["ErrorMessage"]) : "-",
                        ReferenceId = row["ReferenceId"] != DBNull.Value ? Convert.ToString(row["ReferenceId"]) : "-",
                        MessageType = row["MessageType"] != DBNull.Value ? Convert.ToString(row["MessageType"]) : "-",
                        IsPromotional = row["IsPromotional"] != DBNull.Value && Convert.ToBoolean(row["IsPromotional"]),
                        DeductedAmount = row["DeductedAmount"] != DBNull.Value ? Convert.ToDecimal(row["DeductedAmount"]) : 0,
                        RefundAmount = row["RefundAmount"] != DBNull.Value ? Convert.ToDecimal(row["RefundAmount"]) : 0,

                        SentAt = row["SentAt"] != DBNull.Value ? Convert.ToString(row["SentAt"]) : "-",
                        FailedAt = row["FailedAt"] != DBNull.Value ? Convert.ToString(row["FailedAt"]) : "-",
                        RefundedAt = row["RefundedAt"] != DBNull.Value ? Convert.ToString(row["RefundedAt"]) : "-",
                        ActionByName = row["ActionByName"] != DBNull.Value ? Convert.ToString(row["ActionByName"]) : "-",
                        TempName_Title = row["TempName_Title"] != DBNull.Value ? Convert.ToString(row["TempName_Title"]) : "-",
                    });
                }

                apiResponse.message = "WhatsApp message log report fetched successfully!";
                apiResponse.data = rows;
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
        [Route("DownloadWhatsappMsgReport_Pdf")]
        public IActionResult DownloadWhatsappMsgReport_Pdf([FromBody] WhatsAppMessageLogReportRequest request)
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

                    // Company
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // Report
                    cmd = new SqlCommand("WhatsApp_MessageLog_GetReport_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(request.Status) ? (object)DBNull.Value : request.Status);
                    cmd.Parameters.AddWithValue("@CompanyId", request.CompanyId == 0 ? 0 : request.CompanyId);
                    cmd.Parameters.AddWithValue("@MobileNo", string.IsNullOrEmpty(request.MobileNo) ? (object)DBNull.Value : request.MobileNo);
                    cmd.Parameters.AddWithValue("@EventId", request.EventId == 0 ? 0 : request.EventId);
                    cmd.Parameters.AddWithValue("@Search", string.IsNullOrEmpty(request.Search) ? (object)DBNull.Value : request.Search);

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
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                // ── Report title ──
                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                // ── Data table ──
                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD);

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim();

                        bool isAmountColumn =
                            colName.Equals("Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Refund", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Net Amount", StringComparison.OrdinalIgnoreCase);

                        string value = row[col]?.ToString() ?? "-";

                        if (isAmountColumn &&
                            decimal.TryParse(value, out decimal amount))
                        {
                            value = "₹ " + amount.ToString("N2");
                        }

                        int align = isAmountColumn ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT;

                        if (colName.Equals("Sr. No.", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Date", StringComparison.OrdinalIgnoreCase))
                        {
                            align = Element.ALIGN_CENTER;
                        }

                        table.AddCell(
                            Utils.CreateBodyCell(
                                value,
                                isAmountColumn ? AmtFontNormal : tdFont,
                                align
                            )
                        );
                    }
                }

                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim();

                        bool isAmountColumn = false;
                        string value = "";

                        switch (colName)
                        {
                            case "Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDeductedAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Refund":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalRefundAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Net Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["NetAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            default:
                                value = col.Ordinal == 0 ? "TOTAL" : "";
                                break;
                        }

                        int align = isAmountColumn
                            ? Element.ALIGN_RIGHT
                            : Element.ALIGN_LEFT;

                        if (value == "TOTAL")
                        {
                            align = Element.ALIGN_CENTER;
                        }

                        table.AddCell(new PdfPCell(
                            new Phrase(
                                value,
                                isAmountColumn ? AmtFontBold : totalFont))
                        {
                            HorizontalAlignment = align,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            BorderColor = BaseColor.LIGHT_GRAY,
                            Padding = 6
                        });
                    }
                }

                document.Add(table);
                document.Close();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "WhatsappMsg_Report_*.pdf");

                string fileName = $"WhatsappMsg_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);

                response.status = "Success";
                response.message = "WhatsappMsg Report Generated";
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
        [Route("DownloadWhatsappMsgReport_Excel")]
        public IActionResult DownloadWhatsappMsgReport_Excel([FromBody] WhatsAppMessageLogReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("WhatsApp_MessageLog_GetReport_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string? columnsJson = request.Columns != null ? JsonConvert.SerializeObject(request.Columns) : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(request.Status) ? (object)DBNull.Value : request.Status);
                    cmd.Parameters.AddWithValue("@CompanyId", request.CompanyId == 0 ? 0 : request.CompanyId);
                    cmd.Parameters.AddWithValue("@MobileNo", string.IsNullOrEmpty(request.MobileNo) ? (object)DBNull.Value : request.MobileNo);
                    cmd.Parameters.AddWithValue("@EventId", request.EventId == 0 ? 0 : request.EventId);
                    cmd.Parameters.AddWithValue("@Search", string.IsNullOrEmpty(request.Search) ? (object)DBNull.Value : request.Search);


                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("WhatsappMsg Report");

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
                            "Amount" => totalRow["TotalDeductedAmount"].ToString(),
                            "Refund" => totalRow["TotalRefundAmount"].ToString(),
                            "Net Amount" => totalRow["NetAmount"].ToString(),
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

                Utils.deleteOldFiles(folder, "WhatsappMsg_Report_*.xlsx");

                string fileName = $"WhatsappMsg_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);
                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "WhatsappMsg Report Excel generated successfully.";
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

        [HttpPost("GetWhatsappTemplateList")]
        public IActionResult GetWhatsappTemplateList(WhatsappTemplateModel_Request model)
        {
            apiResponse apiResponse = new apiResponse();
            List<WhatsappTemplateModel> list = new List<WhatsappTemplateModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Whatsapp_Template_List", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EventId", model.EventId);
                    con.Open();
                    dt.Load(cmd.ExecuteReader());
                }

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new WhatsappTemplateModel
                    {
                        EventId = row["EventId"] != DBNull.Value ? Convert.ToInt64(row["EventId"]) : 0,
                        TempName_Title = row["TempName_Title"] != DBNull.Value ? Convert.ToString(row["TempName_Title"]) : ""
                    });
                }

                apiResponse.data = list;
                apiResponse.status = "Success";
                apiResponse.message = "WhatsApp templates fetched!";

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

    }
}
