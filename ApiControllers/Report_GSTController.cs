using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2016.Excel;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using ERPAdmin.Services;
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
    public class Report_GSTController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;
        private readonly IInvoicePdfService _invoicePdfService;

        public Report_GSTController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, IInvoicePdfService invoicePdfService) 
            : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            _invoicePdfService = invoicePdfService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetGSTSalesGrid")]
        public IActionResult GetGSTSalesGrid([FromBody] GSTSalesGridRequest request)
        {
            apiResponse response = new apiResponse();
            List<GSTSalesGridModel> rows = new();
            GSTSalesTotalsModel totals = new GSTSalesTotalsModel();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Report_GetGSTSales_GRID", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FromDate",
                    string.IsNullOrWhiteSpace(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                cmd.Parameters.AddWithValue("@ToDate",
                    string.IsNullOrWhiteSpace(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                cmd.Parameters.AddWithValue("@FilterDuration",
                    string.IsNullOrWhiteSpace(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                cmd.Parameters.AddWithValue("@InvoiceNo",
                    request.InvoiceNo.HasValue ? (object)request.InvoiceNo.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@NameEmailContactNo",
                    string.IsNullOrWhiteSpace(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);
                cmd.Parameters.AddWithValue("@IsWithGST",
                    request.IsWithGST.HasValue ? (object)request.IsWithGST.Value : DBNull.Value);

                con.Open();
                using SqlDataReader dr = cmd.ExecuteReader();

                /* ── Result Set 1 : Grid ── */
                while (dr.Read())
                {
                    rows.Add(new GSTSalesGridModel
                    {
                        InvoiceNo = Convert.ToInt64(dr["InvoiceNo"]),
                        GSTInvoiceNo = dr["GSTInvoiceNo"]?.ToString(),
                        DisplayInvoiceNo = dr["DisplayInvoiceNo"]?.ToString(),

                        BookingId = dr["BookingId"] == DBNull.Value
                            ? null
                            : Convert.ToInt64(dr["BookingId"]),

                        InvoiceDate = dr["InvoiceDate"]?.ToString(),
                        InvoiceDate_Format = dr["InvoiceDate_Format"]?.ToString(),

                        Name = dr["Name"]?.ToString(),
                        GSTIN = dr["GSTIN"]?.ToString(),
                        EventType = dr["EventType"]?.ToString(),

                        SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                        Discount = Convert.ToDecimal(dr["Discount"]),
                        NetSale = Convert.ToDecimal(dr["NetSale"]),
                        TaxableAmount = Convert.ToDecimal(dr["TaxableAmount"]),

                        CGST = Convert.ToDecimal(dr["CGST"]),
                        SGST = Convert.ToDecimal(dr["SGST"]),
                        IGST = Convert.ToDecimal(dr["IGST"]),

                        TaxAmount = Convert.ToDecimal(dr["TaxAmount"]),
                        InvoiceAmount = Convert.ToDecimal(dr["InvoiceAmount"]),

                        PaymentMode = dr["PaymentMode"]?.ToString(),

                        PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
                        DueAmount = Convert.ToDecimal(dr["DueAmount"])
                    });
                }

                if (dr.NextResult() && dr.Read())
                {
                    totals = new GSTSalesTotalsModel
                    {
                        TotalSubTotal = Convert.ToDecimal(dr["TotalSubTotal"]),

                        TotalDiscount = Convert.ToDecimal(dr["TotalDiscount"]),

                        TotalNetSale = Convert.ToDecimal(dr["TotalNetSale"]),

                        TotalTaxableAmount = Convert.ToDecimal(dr["TotalTaxableAmount"]),

                        TotalCGST = Convert.ToDecimal(dr["TotalCGST"]),

                        TotalSGST = Convert.ToDecimal(dr["TotalSGST"]),

                        TotalIGST = Convert.ToDecimal(dr["TotalIGST"]),

                        TotalTaxAmount = Convert.ToDecimal(dr["TotalTaxAmount"]),

                        TotalInvoiceAmount = Convert.ToDecimal(dr["TotalInvoiceAmount"]),

                        TotalPaidAmount = Convert.ToDecimal(dr["TotalPaidAmount"]),

                        TotalDueAmount = Convert.ToDecimal(dr["TotalDueAmount"])
                    };
                }
                response.status = "Success";
                response.message = "GST sales grid fetched successfully";
                response.data = new { rows, totals };
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
        [Route("DownloadGSTSalesReport_Pdf")]
        public IActionResult DownloadGSTSalesReport_Pdf([FromBody] GSTSalesReportRequest request)
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
                    cmd = new SqlCommand("Report_GetGSTSales_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar, 20).Value = (object?)request.FromDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar, 20).Value = (object?)request.ToDate ?? DBNull.Value;
                    cmd.Parameters.Add("@FilterDuration", SqlDbType.NVarChar, 50).Value = (object?)request.FilterDuration ?? DBNull.Value;
                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.BigInt).Value = request.InvoiceNo.HasValue ? (object)request.InvoiceNo.Value : DBNull.Value;
                    cmd.Parameters.Add("@NameEmailContactNo", SqlDbType.NVarChar, 50).Value = (object?)request.NameEmailContactNo ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@IsWithGST", request.IsWithGST.HasValue ? (object)request.IsWithGST.Value : DBNull.Value);

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

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                };
                document.Add(reportTitle);

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 7);
                BaseColor thColor = new BaseColor(240, 240, 240);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD);

                // Header row
                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(hRow[0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim();

                        bool isAmountColumn =
                            colName.Equals("SubTotal", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Discount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Net Sale", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Taxable Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("CGST", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("SGST", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("IGST", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Tax Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Invoice Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Paid Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Due Amount", StringComparison.OrdinalIgnoreCase);

                        string value = row[col]?.ToString() ?? "-";

                        if (isAmountColumn &&
                            decimal.TryParse(value, out decimal amount))
                        {
                            value = "₹ " + amount.ToString("N2");
                        }

                        int align = colName.ToLower() switch
                        {
                            "invoice no" => Element.ALIGN_CENTER,
                            "invoice date" => Element.ALIGN_CENTER,
                            "customer name" => Element.ALIGN_LEFT,
                            "gstin" => Element.ALIGN_LEFT,
                            "event type" => Element.ALIGN_LEFT,
                            "payment mode" => Element.ALIGN_LEFT,
                            _ => Element.ALIGN_RIGHT
                        };

                        table.AddCell(
                            Utils.CreateBodyCell(
                                value,
                                isAmountColumn ? AmtFontNormal : tdFont,
                                align
                            ));
                    }
                }

                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    Font totalFont = new Font(
                        Font.FontFamily.HELVETICA,
                        7,
                        Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT;
                        bool isAmountColumn = false;

                        string value = "";

                        switch (col.ColumnName)
                        {
                            case "SubTotal":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalSubTotal"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Discount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDiscount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Net Sale":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalNetSale"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Taxable Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalTaxableAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "CGST":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalCGST"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "SGST":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalSGST"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "IGST":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalIGST"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Tax Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalTaxAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Invoice Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalInvoiceAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Paid Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalPaidAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Due Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDueAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            default:
                                value = col.Ordinal == 0 ? "TOTAL" : "";
                                break;
                        }

                        if (value == "TOTAL")
                            align = Element.ALIGN_CENTER;

                        table.AddCell(new PdfPCell(
                            new Phrase(
                                value,
                                isAmountColumn ? AmtFontBold : totalFont))
                        {
                            HorizontalAlignment = align,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            Padding = 6,
                            BorderColor = new BaseColor(220, 220, 220),
                            BorderWidth = 0.5f
                        });
                    }
                }

                document.Add(table);
                document.Close();
                
                //SAVE
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "GSTSales_Report_*.pdf");

                string fileName = $"GSTSales_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadGSTSalesReport_Excel")]
        public IActionResult DownloadGSTSalesReport_Excel([FromBody] GSTSalesReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetGSTSales_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar, 20).Value = (object?)request.FromDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar, 20).Value = (object?)request.ToDate ?? DBNull.Value;
                    cmd.Parameters.Add("@FilterDuration", SqlDbType.NVarChar, 50).Value = (object?)request.FilterDuration ?? DBNull.Value;
                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.BigInt).Value = request.InvoiceNo.HasValue ? (object)request.InvoiceNo.Value : DBNull.Value;
                    cmd.Parameters.Add("@NameEmailContactNo", SqlDbType.NVarChar, 50).Value = (object?)request.NameEmailContactNo ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@IsWithGST", request.IsWithGST.HasValue ? (object)request.IsWithGST.Value : DBNull.Value);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("GST Sales");

                // Header row
                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // Data rows
                int row = 2;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    row++;
                }

                // Total row
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string? value = columnName switch
                        {
                            "SubTotal" => totalRow["TotalSubTotal"].ToString(),

                            "Discount" => totalRow["TotalDiscount"].ToString(),

                            "Net Sale" => totalRow["TotalNetSale"].ToString(),

                            "Taxable Amount" => totalRow["TotalTaxableAmount"].ToString(),

                            "CGST" => totalRow["TotalCGST"].ToString(),

                            "SGST" => totalRow["TotalSGST"].ToString(),

                            "IGST" => totalRow["TotalIGST"].ToString(),

                            "Tax Amount" => totalRow["TotalTaxAmount"].ToString(),

                            "Invoice Amount" => totalRow["TotalInvoiceAmount"].ToString(),

                            "Paid Amount" => totalRow["TotalPaidAmount"].ToString(),

                            "Due Amount" => totalRow["TotalDueAmount"].ToString(),

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

                Utils.deleteOldFiles(folder, "GSTSales_Report_*.xlsx");

                string fileName = $"GSTSales_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "GST Sales Excel generated successfully.";
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

        // All Invoices DOWNLOAD
        [HttpPost]
        [Route("DownloadGSTSalesDetailReport")]
        public async Task<IActionResult> DownloadGSTSalesDetailReport([FromBody] GSTSalesAllInvoiceReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }
                string fileUrl = await _invoicePdfService.GenerateCustomerInvoiceAllPdf(request, connString, domain, domainUrl);

                response.status = "Success";
                response.message = "GST Sales Detail PDF generated successfully.";
                response.data = new
                {
                    FileUrl = fileUrl
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, "");
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

    }
}
