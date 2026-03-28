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
    public class Report_MonthlySalesController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_MonthlySalesController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetMonthlySalesGrid")]
        public IActionResult GetMonthlySalesGrid([FromBody] MonthlySalesGridRequest request)
        {
            apiResponse response = new apiResponse();

            List<MonthlySalesGridModel> rows = new();
            MonthlySalesTotalsModel totals = new MonthlySalesTotalsModel();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Report_GetMonthlySales_GRID", con);

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

                using SqlDataReader dr = cmd.ExecuteReader();

                /* =========================
                   RESULT SET 1 : GRID
                ========================= */

                while (dr.Read())
                {
                    rows.Add(new MonthlySalesGridModel
                    {
                        SrNo = Convert.ToInt64(dr["SrNo"]),
                        MonthId = Convert.ToInt64(dr["MonthId"]),
                        MonthName = dr["MonthName"]?.ToString(),

                        InvoiceGeneratedCount = Convert.ToInt32(dr["InvoiceGeneratedCount"]),
                        GrossSales = Convert.ToDecimal(dr["GrossSales"]),
                        Taxes = Convert.ToDecimal(dr["Taxes"]),
                        NetSales = Convert.ToDecimal(dr["NetSales"]),

                        PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
                        PaymentMode = dr["PaymentMode"]?.ToString()
                    });
                }

                /* =========================
                   RESULT SET 2 : TOTALS
                ========================= */

                if (dr.NextResult() && dr.Read())
                {
                    totals = new MonthlySalesTotalsModel
                    {
                        TotalInvoices = Convert.ToInt32(dr["TotalInvoices"]),
                        GrossSales = Convert.ToDecimal(dr["TotalGrossSales"]),
                        Taxes = Convert.ToDecimal(dr["TotalTaxes"]),
                        NetSales = Convert.ToDecimal(dr["TotalNetSales"]),
                        TotalPayments = Convert.ToDecimal(dr["TotalPayments"])
                    };
                }

                response.status = "Success";
                response.message = "Monthly sales grid fetched successfully";
                response.data = new
                {
                    rows,
                    totals
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null ?? "");

                response.status = "Error";
                response.message = ex.Message;

                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Route("GetMonthlySalesMonthDetails")]
        public IActionResult GetMonthlySalesMonthDetails([FromBody] MonthlySalesMonthDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            List<MonthlySalesMonthDetailsModel> Rows = new();
            MonthlySalesMonthDetailsTotalsModel Totals = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Report_GetMonthlySales_MonthDetails_GRID", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@MonthId", SqlDbType.Int)
                    .Value = request.MonthId;

                con.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                /* =========================
                   RESULT SET 1 : DETAILS
                ========================= */

                while (dr.Read())
                {
                    Rows.Add(new MonthlySalesMonthDetailsModel
                    {
                        SrNo = Convert.ToInt64(dr["SrNo"]),
                        InvoiceNo = dr["InvoiceNo"]?.ToString(),
                        InvoiceDate = dr["InvoiceDate"]?.ToString(),
                        BookingId = Convert.ToInt64(dr["BookingId"]),

                        ClientName = dr["ClientName"]?.ToString(),

                        GrossAmount = Convert.ToDecimal(dr["GrossAmount"]),
                        Taxes = Convert.ToDecimal(dr["Taxes"]),
                        NetAmount = Convert.ToDecimal(dr["NetAmount"]),

                        PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
                        DueAmount = Convert.ToDecimal(dr["DueAmount"]),

                        PaymentModes = dr["PaymentModes"]?.ToString()
                    });
                }

                /* =========================
                   RESULT SET 2 : TOTALS
                ========================= */

                if (dr.NextResult() && dr.Read())
                {
                    Totals = new MonthlySalesMonthDetailsTotalsModel
                    {
                        TotalInvoices = Convert.ToInt32(dr["TotalInvoices"]),
                        GrossAmount = Convert.ToDecimal(dr["GrossAmount"]),
                        Taxes = Convert.ToDecimal(dr["Taxes"]),
                        NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                        PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
                        DueAmount = Convert.ToDecimal(dr["DueAmount"])
                    };
                }

                response.status = "Success";
                response.message = "Monthly invoice details fetched.";
                response.data = new
                {
                    Rows,
                    Totals
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null ?? "");

                response.status = "Error";
                response.message = ex.Message;

                return StatusCode(500, response);
            }
        }

        [HttpPost]
        [Route("DownloadMonthlySalesReport_Pdf")]
        public IActionResult DownloadMonthlySalesReport_Pdf([FromBody] MonthlySalesReportRequest request)
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
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;

                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // Report Data
                    cmd = new SqlCommand("Report_GetMonthlySales_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@YearId", SqlDbType.Int).Value = (object?)request.YearId ?? DBNull.Value;
                    cmd.Parameters.Add("@Year", SqlDbType.Int).Value = (object?)request.Year ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthId_From", SqlDbType.Int).Value = (object?)request.MonthId_From ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthId_To", SqlDbType.Int).Value = (object?)request.MonthId_To ?? DBNull.Value;

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
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };

                document.Add(reportTitle);

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);

                BaseColor thColor = new BaseColor(240, 240, 240);

                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(hRow[0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT; // default

                        string colName = col.ColumnName.Trim().ToLower();

                        if (colName == "sr. no." ||
                            colName == "month" ||
                            colName == "invoice generated")                        
                        {
                            align = Element.ALIGN_CENTER;
                        }
                        else if (colName == "payment mode")                        
                        {
                            align = Element.ALIGN_LEFT;
                        }

                        //table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));

                        table.AddCell(
                            Utils.CreateBodyCell(
                                row[col]?.ToString() ?? "-",
                                tdFont,
                                align
                            )
                        );

                    }
                }

                // TOTAL ROW
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT; // default
                        string value = col.ColumnName switch
                        {
                            "TotalInvoices" => totalRow["TotalInvoices"].ToString(),
                            "Gross Sales" => totalRow["TotalGrossSales"].ToString(),
                            "Taxes" => totalRow["TotalTaxes"].ToString(),
                            "Net Sales" => totalRow["TotalNetSales"].ToString(),
                            "Paid Amount" => totalRow["TotalPayments"].ToString(),
                            _ => col.Ordinal == 0 ? "TOTAL" : ""
                        };

                        if (col.ColumnName == "TotalInvoices" || value == "TOTAL")
                        {
                            align = Element.ALIGN_CENTER;
                        }

                        table.AddCell(new PdfPCell(new Phrase(value, totalFont))
                        {
                            HorizontalAlignment = align,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            Padding = 6,
                            BorderColor = new BaseColor(220, 220, 220), // light gray
                            BorderWidth = 0.5f
                        });
                    }
                }

                document.Add(table);
                document.Close();

                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "MonthlySales_Report_*.pdf");

                string fileName = $"MonthlySales_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fileStream);

                response.status = "Success";
                response.message = "Monthly Sales Report Generated";
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
        [Route("DownloadMonthlySalesReport_Excel")]
        public IActionResult DownloadMonthlySalesReport_Excel([FromBody] MonthlySalesReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetMonthlySales_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@YearId", SqlDbType.Int).Value = (object?)request.YearId ?? DBNull.Value;
                    cmd.Parameters.Add("@Year", SqlDbType.Int).Value = (object?)request.Year ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthId_From", SqlDbType.Int).Value = (object?)request.MonthId_From ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthId_To", SqlDbType.Int).Value = (object?)request.MonthId_To ?? DBNull.Value;

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Monthly Sales");

                int col = 1;

                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                int row = 2;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string value = columnName switch
                        {
                            "Invoice Generated Count" => totalRow["TotalInvoices"].ToString(),
                            "Gross Sales" => totalRow["TotalGrossSales"].ToString(),
                            "Taxes" => totalRow["TotalTaxes"].ToString(),
                            "Net Sales" => totalRow["TotalNetSales"].ToString(),
                            "Paid Amount" => totalRow["TotalPayments"].ToString(),
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

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "MonthlySales_Report_*.xlsx");

                string fileName = $"MonthlySales_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Monthly Sales Excel generated successfully.";
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
        [Route("DownloadMonthlySalesMonthDetailsReport_Pdf")]
        public IActionResult DownloadMonthlySalesMonthDetailsReport_Pdf([FromBody] MonthlySalesMonthDetailsRequest request)
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
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    cmd = new SqlCommand("Report_GetMonthlySales_MonthDetails_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthId", SqlDbType.Int).Value = request.MonthId;

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
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                Paragraph title = new Paragraph(reportName, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };

                document.Add(title);

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                int fontSize = 8;
                if (headerTableDB.Rows.Count > 10)
                {
                    fontSize = 7;
                }

                Font thFont = new Font(Font.FontFamily.HELVETICA, fontSize, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, fontSize);

                BaseColor thColor = new BaseColor(240, 240, 240);

                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(hRow[0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT;

                        string colName = col.ColumnName.Trim().ToLower();

                        if (colName == "sr. no." ||
                            colName == "invoice no" ||
                            colName == "invoice date" ||
                            colName == "booking id" ||
                            colName == "client name" ||
                            colName == "payment modes")
                        {
                            align = Element.ALIGN_CENTER;
                        }

                        table.AddCell(
                            Utils.CreateBodyCell(
                                row[col]?.ToString() ?? "-",
                                tdFont,
                                align
                            )
                        );
                    }
                }

                // TOTAL ROW
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    Font totalFont = new Font(Font.FontFamily.HELVETICA, fontSize, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT;

                        string? value = col.ColumnName switch
                        {
                            "Gross Amount" => totalRow["GrossAmount"].ToString(),
                            "Taxes" => totalRow["Taxes"].ToString(),
                            "Paid Amount" => totalRow["PaidAmount"].ToString(),
                            "Net Amount" => totalRow["NetAmount"].ToString(),
                            "Due Amount" => totalRow["DueAmount"].ToString(),
                            _ => col.Ordinal == 0 ? "TOTAL" : ""
                        };

                        if (value == "TOTAL")
                            align = Element.ALIGN_CENTER;

                        table.AddCell(new PdfPCell(new Phrase(value, totalFont))
                        {
                            HorizontalAlignment = align,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            Padding = 6,
                            BorderColor = new BaseColor(220, 220, 220), // light gray
                            BorderWidth = 0.5f
                        });
                    }
                }

                document.Add(table);
                document.Close();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "MonthlySalesDetails_Report_*.pdf");

                string fileName = $"MonthlySalesDetails_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using FileStream fs = new FileStream(filePath, FileMode.Create);
                stream.WriteTo(fs);

                response.status = "Success";
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
        [Route("DownloadMonthlySalesMonthDetailsReport_Excel")]
        public IActionResult DownloadMonthlySalesMonthDetailsReport_Excel([FromBody] MonthlySalesMonthDetailsRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetMonthlySales_MonthDetails_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthId", SqlDbType.Int).Value = request.MonthId;

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data missing");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Monthly Sales Details");

                int col = 1;

                /* ───────── Headers ───────── */

                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                int row = 2;

                /* ───────── Data Rows ───────── */

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                /* ───────── TOTAL ROW ───────── */

                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string value = columnName switch
                        {
                            "Gross Amount" => totalRow["GrossAmount"].ToString(),
                            "Taxes" => totalRow["Taxes"].ToString(),
                            "Net Amount" => totalRow["NetAmount"].ToString(),
                            "Due Amount" => totalRow["DueAmount"].ToString(),
                            _ => i == 0 ? "TOTAL" : ""
                        };

                        var cell = worksheet.Cell(row, i + 1);

                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                }

                worksheet.Columns().AdjustToContents();

                /* ───────── Save File ───────── */

                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "MonthlySalesDetails_Report_*.xlsx");

                string fileName = $"MonthlySalesDetails_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
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
