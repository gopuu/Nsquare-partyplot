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
    public class Report_FoodWastageController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_FoodWastageController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        #region Summary Reports

        [HttpPost]
        [Route("Report_GetFoodWastage_GRID")]
        public IActionResult Report_GetFoodWastage_GRID([FromBody] Report_FoodWastageGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<Report_FoodWastageGridModel> rows = new();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString,
                    out string domain,
                    out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetFoodWastage_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);

                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);

                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    rows.Add(new Report_FoodWastageGridModel
                    {
                        SrNo = Convert.ToInt64(row["SrNo"]),
                        BookingId = Convert.ToInt64(row["BookingId"]),
                        EventDate = Convert.ToString(row["EventDate"]),
                        EventDate_Format = Convert.ToString(row["EventDate_Format"]),
                        ShiftCode = Convert.ToString(row["ShiftCode"]),
                        ClientName = Convert.ToString(row["ClientName"]),
                        WastageKg = Convert.ToDecimal(row["WastageKg"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Food wastage summary fetched successfully";
                apiResponse.data = rows;

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

        [HttpPost]
        [Route("DownloadFoodWastageReport_Pdf")]
        public IActionResult DownloadFoodWastageReport_Pdf([FromBody] Report_FoodWastageGridRequest_PdfExcel request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
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
                    cmd = new SqlCommand("Report_GetFoodWastage_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

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

                // ── Table ──
                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(hRow["HeaderName"].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                // ─────────────────────────────────────────────
                // GRID ROWS
                // ─────────────────────────────────────────────
                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = row[col] == DBNull.Value
                            ? "-"
                            : row[col].ToString();

                        int alignment = col.ColumnName switch
                        {
                            "Sr. No." => Element.ALIGN_CENTER,
                            "Event Date" => Element.ALIGN_CENTER,
                            "Shift" => Element.ALIGN_CENTER,
                            "Client Name" => Element.ALIGN_LEFT,
                            "Total Wastage (Kg)" => Element.ALIGN_RIGHT,
                            _ => Element.ALIGN_LEFT
                        };

                        // Format wastage value
                        if (col.ColumnName == "Total Wastage (Kg)")
                        {
                            decimal wastage = row[col] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(row[col]);

                            value = $"{wastage:N2} Kg";
                        }

                        PdfPCell cell = new PdfPCell(new Phrase(value, tdFont))
                        {
                            HorizontalAlignment = alignment,
                            VerticalAlignment = Element.ALIGN_MIDDLE,

                            PaddingTop = 5f,
                            PaddingBottom = 5f,
                            PaddingLeft = 6f,
                            PaddingRight = 6f,

                            BorderColor = new BaseColor(220, 220, 220),
                            BorderWidth = 0.5f
                        };

                        table.AddCell(cell);
                    }
                }

                // ─────────────────────────────────────────────
                // TOTAL ROW
                // ─────────────────────────────────────────────
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    Font totalFont = new Font(
                        Font.FontFamily.HELVETICA,
                        8,
                        Font.BOLD
                    );

                    decimal totalWastage = totalRow["TotalWastageKg"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(totalRow["TotalWastageKg"]);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = "";
                        int alignment = Element.ALIGN_LEFT;

                        switch (col.ColumnName)
                        {
                            case "Sr. No.":
                                value = "TOTAL";
                                alignment = Element.ALIGN_CENTER;
                                break;

                            case "Total Wastage (Kg)":
                                value = $"{totalWastage:N2} Kg";
                                alignment = Element.ALIGN_RIGHT;
                                break;
                        }

                        PdfPCell cell = new PdfPCell(new Phrase(value, totalFont))
                        {
                            HorizontalAlignment = alignment,
                            VerticalAlignment = Element.ALIGN_MIDDLE,

                            BackgroundColor = new BaseColor(240, 240, 240),

                            PaddingTop = 6f,
                            PaddingBottom = 6f,
                            PaddingLeft = 6f,
                            PaddingRight = 6f,

                            BorderColor = new BaseColor(210, 210, 210),
                            BorderWidth = 0.5f
                        };

                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "FoodWastage_Report_*.pdf");

                string fileName = $"FoodWastage_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);

                response.status = "Success";
                response.message = "Food Wastage Report Generated";
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
        [Route("DownloadFoodWastageReport_Excel")]
        public IActionResult DownloadFoodWastageReport_Excel([FromBody] Report_FoodWastageGridRequest_PdfExcel request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_GetFoodWastage_PdfExcel", con);
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
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Food Wastage Report");

                int col = 1;

                // Headers
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                int row = 2;

                // Data
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                // Totals
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string value = columnName switch
                        {
                            "Wastage (Kg)" => totalRow["TotalWastageKg"].ToString(),
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

                Utils.deleteOldFiles(folder, "FoodWastage_Report_*.xlsx");

                string fileName = $"FoodWastage_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Food Wastage Excel generated successfully";
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

        #region Detail Reports

        [HttpPost]
        [Route("Report_GetFoodWastageDetail_GRID")]
        public IActionResult Report_GetFoodWastageDetail_GRID([FromBody] Report_FoodWastageDetailsRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString,
                    out string domain,
                    out IActionResult? errorResult))
                    return errorResult!;

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetFoodWastageDetail_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ds);
                }

                if (ds.Tables.Count < 2)
                    throw new Exception("Food wastage detail not found.");

                DataTable headerTable = ds.Tables[0];
                DataTable itemsTable = ds.Tables[1];

                if (headerTable.Rows.Count == 0)
                    throw new Exception("Booking details not found.");

                DataRow h = headerTable.Rows[0];

                Report_FoodWastageDetailsModel model = new Report_FoodWastageDetailsModel
                {
                    BookingId = h["BookingId"] != DBNull.Value ? Convert.ToInt64(h["BookingId"]) : 0,
                    EventDate = h["EventDate"]?.ToString(),
                    EventDate_Format = h["EventDate_Format"]?.ToString(),
                    ShiftCode = h["ShiftCode"]?.ToString(),
                    ClientName = h["ClientName"]?.ToString(),
                    Items = new List<Report_FoodWastageDetailsModel_Items>()
                };

                foreach (DataRow row in itemsTable.Rows)
                {
                    model.Items.Add(new Report_FoodWastageDetailsModel_Items
                    {
                        SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt64(row["SrNo"]) : 0,
                        MenuItemName = row["MenuItemName"]?.ToString(),
                        MenuItemId = row["MenuItemId"] != DBNull.Value ? Convert.ToInt64(row["MenuItemId"]) : null,
                        WastageKg = row["WastageKg"] != DBNull.Value ? Convert.ToDecimal(row["WastageKg"]) : 0
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Food wastage details fetched successfully!";
                apiResponse.data = model;

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

        [HttpPost]
        [Route("DownloadFoodWastageDetailReport_Pdf")]
        public IActionResult DownloadFoodWastageDetailReport_Pdf([FromBody] Report_FoodWastageDetailsRequest_PdfExcel request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
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
                    cmd = new SqlCommand("Report_GetFoodWastageDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);

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
                DataTable totalTableDB = dsReport.Tables[3];
                DataTable basicInfoTable = dsReport.Tables[4];

                // ── Report title ──
                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                // ── Booking basic info ──
                if (basicInfoTable.Rows.Count > 0)
                {
                    DataRow r = basicInfoTable.Rows[0];

                    PdfPTable infoTable = new PdfPTable(2);
                    infoTable.WidthPercentage = 100;

                    infoTable.AddCell(Utils.CreateInfoCell("Booking ID", "#" + r["BookingId"]));
                    infoTable.AddCell(Utils.CreateInfoCell("Event Date", r["EventDate"]));

                    infoTable.AddCell(Utils.CreateInfoCell("Client", r["ClientName"]));
                    infoTable.AddCell(Utils.CreateInfoCell("Shift", r["ShiftCode"]));

                    document.Add(infoTable);
                    document.Add(new Paragraph("\n"));
                }

                // ── Data table ──
                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.GRAY);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                foreach (DataRow h in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(h["HeaderName"].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = row[col] == DBNull.Value
                            ? "-"
                            : row[col].ToString();

                        int alignment = col.ColumnName switch
                        {
                            "Sr. No." => Element.ALIGN_CENTER,
                            "Menu Item" => Element.ALIGN_LEFT,
                            "Wastage (Kg)" => Element.ALIGN_RIGHT,
                            _ => Element.ALIGN_LEFT
                        };

                        // Format Wastage
                        if (col.ColumnName == "Wastage (Kg)")
                        {
                            decimal wastage = row[col] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(row[col]);

                            value = $"{wastage:N2} Kg";
                        }

                        PdfPCell cell = new PdfPCell(new Phrase(value, tdFont))
                        {
                            HorizontalAlignment = alignment,
                            VerticalAlignment = Element.ALIGN_MIDDLE,

                            PaddingTop = 5f,
                            PaddingBottom = 5f,
                            PaddingLeft = 6f,
                            PaddingRight = 6f,

                            BorderColor = new BaseColor(220, 220, 220),
                            BorderWidth = 0.5f
                        };

                        table.AddCell(cell);
                    }
                }

                // ── Totals ──
                if (totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = col.ColumnName switch
                        {
                            "Wastage (Kg)" => $"{totalRow["TotalWastageKg"]} Kg",
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

                Utils.deleteOldFiles(folder, "FoodWastageDetail_Report_*.pdf");

                string fileName = $"FoodWastageDetail_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);

                response.status = "Success";
                response.message = "Food Wastage Detail Report Generated";
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
        [Route("DownloadFoodWastageDetailReport_Excel")]
        public IActionResult DownloadFoodWastageDetailReport_Excel([FromBody] Report_FoodWastageDetailsRequest_PdfExcel request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_GetFoodWastageDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 4)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables[3];
                DataTable basicInfoTable = dsReport.Tables[4];

                string reportName = dsReport.Tables[0].Rows.Count > 0
                    ? dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "Food Wastage Detail"
                    : "Food Wastage Detail";

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Food Wastage Detail");

                int columnCount = dataTableDB.Columns.Count;


                // ============================================================
                // REPORT TITLE
                // ============================================================

                worksheet.Range(1, 1, 1, columnCount).Merge();

                var titleCell = worksheet.Cell(1, 1);
                titleCell.Value = reportName.ToUpper();
                titleCell.Style.Font.Bold = true;
                titleCell.Style.Font.FontSize = 14;
                titleCell.Style.Font.FontColor = XLColor.Gray;
                titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Row(1).Height = 24;


                // ============================================================
                // BOOKING BASIC INFO
                // ============================================================

                int currentRow = 3;

                if (basicInfoTable.Rows.Count > 0)
                {
                    DataRow r = basicInfoTable.Rows[0];

                    // Row 3
                    worksheet.Cell(currentRow, 1).Value = "Booking ID";
                    worksheet.Cell(currentRow, 2).Value = "#" + r["BookingId"]?.ToString();

                    worksheet.Cell(currentRow, 3).Value = "Event Date";
                    worksheet.Cell(currentRow, 4).Value = r["EventDate"]?.ToString();

                    // Row 4
                    worksheet.Cell(currentRow + 1, 1).Value = "Client";
                    worksheet.Cell(currentRow + 1, 2).Value = r["ClientName"]?.ToString();

                    worksheet.Cell(currentRow + 1, 3).Value = "Shift";
                    worksheet.Cell(currentRow + 1, 4).Value = r["ShiftCode"]?.ToString();


                    // Label formatting
                    foreach (var cell in new[]
                    {
                        worksheet.Cell(currentRow, 1),
                        worksheet.Cell(currentRow, 3),
                        worksheet.Cell(currentRow + 1, 1),
                        worksheet.Cell(currentRow + 1, 3)
                    })
                    {
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    }


                    // Border around basic info
                    var infoRange = worksheet.Range(
                        currentRow,
                        1,
                        currentRow + 1,
                        4
                    );

                    infoRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    infoRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    currentRow += 3; // blank row after basic info
                }


                // ============================================================
                // TABLE HEADER
                // ============================================================
                int headerRow = currentRow;
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


                // ============================================================
                // DATA
                // ============================================================
                int row = headerRow + 1;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        DataColumn column = dataTableDB.Columns[i];
                        var cell = worksheet.Cell(row, i + 1);

                        if (column.ColumnName == "Wastage (Kg)")
                        {
                            decimal wastage = dr[i] == DBNull.Value
                                ? 0
                                : Convert.ToDecimal(dr[i]);

                            cell.Value = wastage;
                            cell.Style.NumberFormat.Format = "0.00 \"Kg\"";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        else
                        {
                            cell.Value = dr[i] == DBNull.Value
                                ? "-"
                                : dr[i].ToString();

                            cell.Style.Alignment.Horizontal = column.ColumnName switch
                            {
                                "Sr. No." => XLAlignmentHorizontalValues.Center,
                                "Menu Item" => XLAlignmentHorizontalValues.Left,
                                _ => XLAlignmentHorizontalValues.Left
                            };
                        }

                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    row++;
                }


                // ============================================================
                // TOTAL
                // ============================================================
                if (totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

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
                        else if (columnName == "Wastage (Kg)")
                        {
                            decimal totalWastage =
                                totalRow["TotalWastageKg"] == DBNull.Value
                                    ? 0
                                    : Convert.ToDecimal(totalRow["TotalWastageKg"]);

                            cell.Value = totalWastage;
                            cell.Style.NumberFormat.Format = "0.00 \"Kg\"";
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        else
                        {
                            cell.Value = "";
                        }
                    }
                }


                // ============================================================
                // FINAL FORMATTING
                // ============================================================

                worksheet.Columns().AdjustToContents();

                worksheet.SheetView.FreezeRows(headerRow);

                // Optional minimum widths
                worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 10);
                worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 25);
                worksheet.Column(3).Width = Math.Max(worksheet.Column(3).Width, 15);


                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "FoodWastageDetail_Report_*.xlsx");

                string fileName = $"FoodWastageDetail_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Food Wastage Detail Excel generated successfully";
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
