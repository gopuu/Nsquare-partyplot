//using DocumentFormat.OpenXml.Office2010.Excel;
//using ERPAdmin.ApiControllers;
//using System.Dynamic;
//using System.Web.Helpers;
using ClosedXML.Excel;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
//using DocumentFormat.OpenXml.Office.Word;
//using DocumentFormat.OpenXml.Office2016.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
//using ERPAdmin.Services;
//using Humanizer;
//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;
//using Microsoft.AspNetCore.Http;
//using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharpFont = iTextSharp.text.Font;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergencyContactMasterController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;
        public EmergencyContactMasterController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _env = env;
        }

        [HttpPost]
        [Route("GetEmergencyContact_Master")]
        public IActionResult GetEmergencyContact_Master([FromBody] GetEmergencyContactRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetEmergencyContactResponse> list = new();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("GetEmergencyContact_Master", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", request?.Id ?? 0);
                cmd.Parameters.AddWithValue("@CategoryId", request?.CategoryId ?? 0);
                cmd.Parameters.AddWithValue("@SubCategoryId", request?.SubCategoryId ?? 0);
                cmd.Parameters.AddWithValue("@IsActive", request?.IsActive ?? (object)DBNull.Value);

                con.Open();

                using SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new GetEmergencyContactResponse
                    {
                        Id = Convert.ToInt64(dr["Id"]),
                        SubCategoryId = Convert.ToInt64(dr["SubCategoryId"]),
                        SubCategoryName = Convert.ToString(dr["SubCategoryName"]),
                        CategoryId = Convert.ToInt64(dr["CategoryId"]),
                        CategoryName = Convert.ToString(dr["CategoryName"]),
                        ContactType = Convert.ToString(dr["ContactType"]),
                        ContactName = Convert.ToString(dr["ContactName"]),
                        MobileNo = Convert.ToString(dr["MobileNo"]),
                        MobileNo2 = dr["MobileNo2"] == DBNull.Value
                            ? null
                            : Convert.ToString(dr["MobileNo2"]),
                        Location = dr["Location"] == DBNull.Value
                            ? null
                            : Convert.ToString(dr["Location"]),
                        Remarks = dr["Remarks"] == DBNull.Value
                            ? null
                            : Convert.ToString(dr["Remarks"]),
                        IsActive = Convert.ToBoolean(dr["IsActive"])
                    });
                }

                apiResponse.status = "Success";
                apiResponse.message = "Emergency contacts fetched successfully!";
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

        [HttpPost]
        [Route("SaveEmergencyContact_Master")]
        public IActionResult SaveEmergencyContact_Master([FromBody] SaveEmergencyContactRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long id;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("SaveEmergencyContact_Master", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Id", model.Id <= 0 ? 0 : model.Id);
                cmd.Parameters.AddWithValue("@SubCategoryId", model.SubCategoryId);
                cmd.Parameters.AddWithValue("@ContactType", model.ContactType);
                cmd.Parameters.AddWithValue("@ContactName", model.ContactName);
                cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                cmd.Parameters.AddWithValue("@MobileNo2",
                    model.MobileNo2 ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Location",
                    model.Location ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Remarks",
                    model.Remarks ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", model.IsActive);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                id = Convert.ToInt64(cmd.ExecuteScalar());
                con.Close();

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0
                    ? "Emergency contact updated successfully!"
                    : "Emergency contact added successfully!";
                apiResponse.data = new { Id = id };

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
        [Route("DeleteEmergencyContact_Master")]
        public IActionResult DeleteEmergencyContact_Master([FromBody] DeleteEmergencyContactRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("DeleteEmergencyContact_Master", con);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", model.Id);
                cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                con.Open();
                cmd.ExecuteNonQuery();

                apiResponse.status = "Success";
                apiResponse.message = "Emergency contact deleted successfully!";
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
        [Route("Report_EmergencyContact")]
        public IActionResult Report_EmergencyContact([FromBody] EmergencyContactReportRequest request)
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

                    // Company Header
                    using (SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                        new SqlDataAdapter(cmd).Fill(dtCompany);
                    }

                    // Report Data
                    using (SqlCommand cmd = new SqlCommand("Report_GetEmergencyContact", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = request.Columns;
                        cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id;
                        cmd.Parameters.Add("@CategoryId", SqlDbType.BigInt).Value = request.CategoryId;
                        cmd.Parameters.Add("@SubCategoryId", SqlDbType.BigInt).Value = request.SubCategoryId;
                        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value =
                            request.IsActive == null ? DBNull.Value : request.IsActive;

                        new SqlDataAdapter(cmd).Fill(dsReport);
                    }
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

                // ================= REPORT TITLE =================
                iTextSharpFont reportTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY);
                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 12f
                };

                document.Add(reportTitle);

                // ================= TABLE =================

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                iTextSharpFont thFont = new iTextSharpFont(iTextSharpFont.FontFamily.HELVETICA, 9, iTextSharpFont.BOLD);
                iTextSharpFont tdFont = new iTextSharpFont(iTextSharpFont.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(240, 240, 240);

                // Header
                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                {
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));
                }

                table.HeaderRows = 1;

                // Data
                if (dataTableDB.Rows.Count == 0)
                {
                    PdfPCell noDataCell = new PdfPCell(new Phrase("No Data Found"))
                    {
                        Colspan = columnCount,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 12f
                    };

                    table.AddCell(noDataCell);
                }
                else
                {
                    foreach (DataRow row in dataTableDB.Rows)
                    {
                        for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        {
                            table.AddCell(Utils.CreateBodyCell(row[i]?.ToString() ?? "-", tdFont));
                        }
                    }
                }

                document.Add(table);
                document.Close();

                // Save file
                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "EmergencyContactReport_*.pdf");

                string fileName = $"EmergencyContactReport_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Emergency Contact Report Generated";
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
        [Route("Report_EmergencyContact_Excel")]
        public IActionResult Report_EmergencyContact_Excel([FromBody] EmergencyContactReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                if (string.IsNullOrWhiteSpace(request.Columns))
                {
                    request.Columns = "[\"CategoryName\",\"SubCategoryName\",\"ContactType\",\"ContactName\",\"MobileNo\",\"MobileNo2\",\"Location\",\"Remarks\",\"IsActive\"]";
                }

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("Report_GetEmergencyContact", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = request.Columns;
                        cmd.Parameters.Add("@Id", SqlDbType.BigInt).Value = request.Id;
                        cmd.Parameters.Add("@CategoryId", SqlDbType.BigInt).Value = request.CategoryId;
                        cmd.Parameters.Add("@SubCategoryId", SqlDbType.BigInt).Value = request.SubCategoryId;
                        cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value =
                            request.IsActive == null ? DBNull.Value : request.IsActive;

                        new SqlDataAdapter(cmd).Fill(dsReport);
                    }
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("EmergencyContacts");

                int colCount = headerTableDB.Rows.Count;

                // Headers
                for (int i = 0; i < colCount; i++)
                {
                    worksheet.Cell(1, i + 1).Value = headerTableDB.Rows[i][0].ToString();
                    worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Data
                int row = 2;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < colCount; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                string reportsDir = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(reportsDir))
                    Directory.CreateDirectory(reportsDir);

                Utils.deleteOldFiles(reportsDir, "EmergencyContactReport_*.xlsx");

                string fileName = $"EmergencyContactReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                string filePath = Path.Combine(reportsDir, fileName);

                workbook.SaveAs(filePath);

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Emergency Contact Report Excel generated successfully.";
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
