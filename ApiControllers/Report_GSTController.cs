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
    public class Report_GSTController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_GSTController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
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

                con.Open();
                using SqlDataReader dr = cmd.ExecuteReader();

                /* ── Result Set 1 : Grid ── */
                while (dr.Read())
                {
                    rows.Add(new GSTSalesGridModel
                    {
                        InvoiceNo = Convert.ToInt64(dr["InvoiceNo"]),
                        InvoiceDate = dr["InvoiceDate"]?.ToString(),
                        InvoiceDate_Format = dr["InvoiceDate_Format"]?.ToString(),
                        Name = dr["Name"]?.ToString(),
                        GSTIN = dr["GSTIN"]?.ToString(),
                        EventType = dr["EventType"]?.ToString(),
                        SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                        SGST = Convert.ToDecimal(dr["SGST"]),
                        CGST = Convert.ToDecimal(dr["CGST"]),
                        IGST = Convert.ToDecimal(dr["IGST"]),
                        TotalTax = Convert.ToDecimal(dr["TotalTax"]),
                        TotalInvoiceAmount = Convert.ToDecimal(dr["TotalInvoiceAmount"]),
                        PaymentMode = dr["PaymentMode"]?.ToString(),
                        PaidAmount = Convert.ToDecimal(dr["PaidAmount"]),
                        DueAmount = Convert.ToDecimal(dr["DueAmount"])
                    });
                }

                // NOTE: Report_GetGSTSales_GRID SP currently returns only one result set (grid).
                // If you add a totals result set to the SP later, uncomment the block below:
                //
                if (dr.NextResult() && dr.Read())
                {
                    totals = new GSTSalesTotalsModel
                    {
                        TotalSubTotal = Convert.ToDecimal(dr["TotalSubTotal"]),
                        TotalCGST = Convert.ToDecimal(dr["TotalCGST"]),
                        TotalSGST = Convert.ToDecimal(dr["TotalSGST"]),
                        TotalIGST = Convert.ToDecimal(dr["TotalIGST"]),
                        TotalTax = Convert.ToDecimal(dr["TotalTax"]),
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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4.Rotate(), 36, 36, topMargin, 50); // landscape for more columns
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

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
                    throw new Exception("Report data not found");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

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

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 18f
                };
                document.Add(reportTitle);

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 7);
                BaseColor thColor = new BaseColor(0, 121, 182);

                // Header row
                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(hRow[0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                // Data rows
                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim().ToLower();

                        int align = colName switch
                        {
                            "invoice no" => Element.ALIGN_CENTER,
                            "invoice date" => Element.ALIGN_CENTER,
                            "customer name" => Element.ALIGN_LEFT,
                            "gstin" => Element.ALIGN_LEFT,
                            "event type" => Element.ALIGN_LEFT,
                            "payment mode" => Element.ALIGN_LEFT,
                            _ => Element.ALIGN_RIGHT
                        };

                        table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont, align));
                    }
                }

                // Total row
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT;
                        string value = col.ColumnName switch
                        {
                            "Sub Total" => totalRow["TotalSubTotal"].ToString() ?? "-",
                            "CGST" => totalRow["TotalCGST"].ToString() ?? "-",
                            "SGST" => totalRow["TotalSGST"].ToString() ?? "-",
                            "IGST" => totalRow["TotalIGST"].ToString() ?? "-",
                            "Total GST" => totalRow["TotalTax"].ToString() ?? "-",
                            "Invoice Amount" => totalRow["TotalInvoiceAmount"].ToString() ?? "-",
                            "Paid Amount" => totalRow["TotalPaidAmount"].ToString() ?? "-",
                            "Due Amount" => totalRow["TotalDueAmount"].ToString() ?? "-",
                            _ => col.Ordinal == 0 ? "TOTAL" : ""
                        };

                        if (value == "TOTAL") align = Element.ALIGN_CENTER;

                        table.AddCell(new PdfPCell(new Phrase(value, totalFont))
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
                            "Sub Total" => totalRow["TotalSubTotal"].ToString(),
                            "CGST" => totalRow["TotalCGST"].ToString(),
                            "SGST" => totalRow["TotalSGST"].ToString(),
                            "IGST" => totalRow["TotalIGST"].ToString(),
                            "Total GST" => totalRow["TotalTax"].ToString(),
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

    }
}
