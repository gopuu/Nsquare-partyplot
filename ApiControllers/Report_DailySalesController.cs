using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2016.Excel;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using ExcelDataReader.Log;
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
    public class Report_DailySalesController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_DailySalesController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }
        // DATA
        [HttpPost]
        [Route("GetDailySalesGrid")]
        public IActionResult GetDailySalesGrid([FromBody] DailySalesGridRequest request)
        {
            apiResponse response = new apiResponse();

            List<DailySalesGridModel> rows = new();
            DailySalesTotalsModel totals = new DailySalesTotalsModel();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Report_GetDailySales_GRID", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FromDate",
                    string.IsNullOrWhiteSpace(request.FromDate) ? (object)DBNull.Value : request.FromDate);

                cmd.Parameters.AddWithValue("@ToDate",
                    string.IsNullOrWhiteSpace(request.ToDate) ? (object)DBNull.Value : request.ToDate);

                cmd.Parameters.AddWithValue("@FilterDuration",
                    string.IsNullOrWhiteSpace(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                con.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                /* =========================
                   RESULT SET 1 : GRID
                ========================= */
                while (dr.Read())
                {
                    rows.Add(new DailySalesGridModel
                    {
                        SrNo = Convert.ToInt64(dr["SrNo"]),

                        InvoiceDate = Convert.ToString(dr["InvoiceDate"]),
                        InvoiceDate_Format = Convert.ToString(dr["InvoiceDate_Format"]),

                        Qty = Convert.ToInt32(dr["Qty"]),

                        SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                        Discount = Convert.ToDecimal(dr["Discount"]),
                        NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                        TaxableAmount = Convert.ToDecimal(dr["TaxableAmount"]),
                        TaxAmount = Convert.ToDecimal(dr["TaxAmount"]),

                        InvoiceAmount = Convert.ToDecimal(dr["InvoiceAmount"]),

                        PaymentAmount = Convert.ToDecimal(dr["PaymentAmount"]),
                        PaymentMode = Convert.ToString(dr["PaymentMode"]),

                        DuePayment = Convert.ToDecimal(dr["DuePayment"])
                    });
                }

                /* =========================
                   RESULT SET 2 : TOTALS
                ========================= */
                if (dr.NextResult() && dr.Read())
                {
                    totals = new DailySalesTotalsModel
                    {
                        TotalQty = Convert.ToInt32(dr["TotalQty"]),

                        TotalSubTotalAmount = Convert.ToDecimal(dr["TotalSubTotalAmount"]),
                        TotalDiscount = Convert.ToDecimal(dr["TotalDiscount"]),
                        TotalNetAmount = Convert.ToDecimal(dr["TotalNetAmount"]),
                        TotalTaxableAmount = Convert.ToDecimal(dr["TotalTaxableAmount"]),
                        TotalTaxAmount = Convert.ToDecimal(dr["TotalTaxAmount"]),

                        TotalInvoiceAmount = Convert.ToDecimal(dr["TotalInvoiceAmount"]),

                        TotalPaymentAmount = Convert.ToDecimal(dr["TotalPaymentAmount"]),

                        TotalDuePayment = Convert.ToDecimal(dr["TotalDuePayment"])
                    };
                }

                response.status = "Success";
                response.message = "Daily sales grid fetched successfully";
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

        //DETAILS DATA
        [HttpPost]
        [Route("GetDailySalesDateDetails")]
        public IActionResult GetDailySalesDateDetails([FromBody] DailySalesDateDetailsRequest request)
        {
            apiResponse response = new apiResponse();

            List<DailySalesDateDetailsModel> Rows = new();
            DailySalesDateDetailsTotalsModel Totals = new();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using SqlConnection con = new SqlConnection(connString);
                using SqlCommand cmd = new SqlCommand("Report_GetDailySales_DateDetails", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@InvoiceDate", SqlDbType.NVarChar, 20).Value = request.InvoiceDate ?? (object)DBNull.Value;

                con.Open();

                using SqlDataReader dr = cmd.ExecuteReader();

                /* =========================
                   RESULT SET 1 : DETAILS
                ========================= */
                while (dr.Read())
                {
                    Rows.Add(new DailySalesDateDetailsModel
                    {
                        SrNo = Convert.ToInt64(dr["SrNo"]),

                        InvoiceNo = Convert.ToString(dr["InvoiceNo"]),

                        InvoiceDate = Convert.ToString(dr["InvoiceDate"]),
                        InvoiceDate_Format = Convert.ToString(dr["InvoiceDate_Format"]),

                        BookingId = Convert.ToInt64(dr["BookingId"]),

                        ClientName = Convert.ToString(dr["ClientName"]),

                        SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                        Discount = Convert.ToDecimal(dr["Discount"]),

                        NetAmount = Convert.ToDecimal(dr["NetAmount"]),
                        TaxableAmount = Convert.ToDecimal(dr["TaxableAmount"]),

                        TaxAmount = Convert.ToDecimal(dr["TaxAmount"]),

                        InvoiceAmount = Convert.ToDecimal(dr["InvoiceAmount"]),

                        PaymentAmount = Convert.ToDecimal(dr["PaymentAmount"]),
                        DuePayment = Convert.ToDecimal(dr["DuePayment"]),

                        PaymentModes = Convert.ToString(dr["PaymentModes"])
                    });
                }

                /* =========================
                   RESULT SET 2 : TOTALS
                ========================= */
                if (dr.NextResult() && dr.Read())
                {
                    Totals = new DailySalesDateDetailsTotalsModel
                    {
                        TotalInvoices = Convert.ToInt32(dr["TotalInvoices"]),

                        TotalSubTotalAmount = Convert.ToDecimal(dr["TotalSubTotalAmount"]),
                        TotalDiscount = Convert.ToDecimal(dr["TotalDiscount"]),

                        TotalNetAmount = Convert.ToDecimal(dr["TotalNetAmount"]),
                        TotalTaxableAmount = Convert.ToDecimal(dr["TotalTaxableAmount"]),

                        TotalTaxAmount = Convert.ToDecimal(dr["TotalTaxAmount"]),

                        TotalInvoiceAmount = Convert.ToDecimal(dr["TotalInvoiceAmount"]),

                        TotalPaymentAmount = Convert.ToDecimal(dr["TotalPaymentAmount"]),
                        TotalDuePayment = Convert.ToDecimal(dr["TotalDuePayment"])
                    };
                }

                response.status = "Success";
                response.message = "Invoice details fetched.";
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

        //DATA
        [HttpPost]
        [Route("DownloadDailySalesReport_Pdf")]
        public IActionResult DownloadDailySalesReport_Pdf([FromBody] DailySalesReportRequest request)
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
                    cmd = new SqlCommand("Report_GetDailySales_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar, 20).Value = (object?)request.FromDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar, 20).Value = (object?)request.ToDate ?? DBNull.Value;
                    cmd.Parameters.Add("@FilterDuration", SqlDbType.NVarChar, 50).Value = (object?)request.FilterDuration ?? DBNull.Value;

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                #region ASSIGN & VALIDATION
                // ================= ASSIGN & VALIDATION =================
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

                // ── Attach StandardReport header-footer ──
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

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD);

                BaseColor thColor = new BaseColor(240, 240, 240);

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
                            colName.Equals("Net Amt", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Taxable Amt", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Tax Amt", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Invoice Amt", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Payment Amt", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Due Payment", StringComparison.OrdinalIgnoreCase);

                        string value = row[col]?.ToString() ?? "-";

                        if (isAmountColumn &&
                            decimal.TryParse(value, out decimal amount))
                        {
                            value = "₹ " + amount.ToString("N2");
                        }

                        int align = Element.ALIGN_LEFT;

                        if (
                            colName.Equals("Sr. No.", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Date", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Qty", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            align = Element.ALIGN_CENTER;
                        }
                        else if (isAmountColumn)
                        {
                            align = Element.ALIGN_RIGHT;
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

                // ===== TOTAL ROW =====
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim();

                        bool isAmountColumn = false;
                        string value = "";
                        int align = Element.ALIGN_LEFT;

                        switch (colName)
                        {
                            case "Qty":
                                value = Convert.ToDecimal(totalRow["TotalQty"]).ToString("N0");
                                align = Element.ALIGN_CENTER;
                                break;

                            case "SubTotal":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalSubTotalAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Discount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDiscount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Net Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalNetAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Taxable Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalTaxableAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Tax Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalTaxAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Invoice Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalInvoiceAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Payment Amt":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalPaymentAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Due Payment":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDuePayment"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            default:
                                value = col.Ordinal == 0 ? "TOTAL" : "";
                                if (value == "TOTAL")
                                    align = Element.ALIGN_CENTER;
                                break;
                        }

                        if (isAmountColumn)
                            align = Element.ALIGN_RIGHT;

                        table.AddCell(
                            new PdfPCell(
                                new Phrase(
                                    value,
                                    isAmountColumn ? AmtFontBold : totalFont
                                )
                            )
                            {
                                HorizontalAlignment = align,
                                BackgroundColor = new BaseColor(240, 240, 240),
                                Padding = 6,
                                BorderColor = new BaseColor(220, 220, 220),
                                BorderWidth = 0.5f
                            }
                        );
                    }
                }

                document.Add(table);
                document.Close();

                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "DailySales_Report_*.pdf");

                string fileName = $"DailySales_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadDailySalesReport_Excel")]
        public IActionResult DownloadDailySalesReport_Excel([FromBody] DailySalesReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetDailySales_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar, 20).Value = (object?)request.FromDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar, 20).Value = (object?)request.ToDate ?? DBNull.Value;
                    cmd.Parameters.Add("@FilterDuration", SqlDbType.NVarChar, 50).Value = (object?)request.FilterDuration ?? DBNull.Value;

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Daily Sales");

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

                // TOTAL ROW
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string value = "";

                        switch (columnName)
                        {
                            case "Qty":
                                value = Convert.ToDecimal(totalRow["TotalQty"]).ToString("N0");
                                break;

                            case "SubTotal":
                                value = Convert.ToDecimal(totalRow["TotalSubTotalAmount"]).ToString("N2");
                                break;

                            case "Discount":
                                value = Convert.ToDecimal(totalRow["TotalDiscount"]).ToString("N2");
                                break;

                            case "Net Amt":
                                value = Convert.ToDecimal(totalRow["TotalNetAmount"]).ToString("N2");
                                break;

                            case "Taxable Amt":
                                value = Convert.ToDecimal(totalRow["TotalTaxableAmount"]).ToString("N2");
                                break;

                            case "Tax Amt":
                                value = Convert.ToDecimal(totalRow["TotalTaxAmount"]).ToString("N2");
                                break;

                            case "Invoice Amt":
                                value = Convert.ToDecimal(totalRow["TotalInvoiceAmount"]).ToString("N2");
                                break;

                            case "Payment Amt":
                                value = Convert.ToDecimal(totalRow["TotalPaymentAmount"]).ToString("N2");
                                break;

                            case "Due Payment":
                                value = Convert.ToDecimal(totalRow["TotalDuePayment"]).ToString("N2");
                                break;

                            default:
                                value = i == 0 ? "TOTAL" : "";
                                break;
                        }

                        var cell = worksheet.Cell(row, i + 1);

                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                        if (
                            columnName == "Qty" ||
                            columnName == "Sr. No." ||
                            columnName == "Date"
                        )
                        {
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        }
                        else if (
                            columnName == "SubTotal" ||
                            columnName == "Discount" ||
                            columnName == "Net Amt" ||
                            columnName == "Taxable Amt" ||
                            columnName == "Tax Amt" ||
                            columnName == "Invoice Amt" ||
                            columnName == "Payment Amt" ||
                            columnName == "Due Payment"
                        )
                        {
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "DailySales_Report_*.xlsx");

                string fileName = $"DailySales_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Daily Sales Excel generated successfully.";
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

        //DETAILS DATA
        [HttpPost]
        [Route("DownloadDailySalesDateDetailsReport_Pdf")]
        public IActionResult DownloadDailySalesDateDetailsReport_Pdf([FromBody] DailySalesDateDetailsRequest request)
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

                    cmd = new SqlCommand("Report_GetDailySales_DateDetails_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@InvoiceDate", SqlDbType.Date).Value = (object?)request.InvoiceDate ?? DBNull.Value;

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                #region ASSIGN & VALIDATION
                // ================= ASSIGN & VALIDATION =================
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

                // ── Attach StandardReport header-footer ──
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

                Paragraph title = new Paragraph(reportName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 18
                };
                document.Add(title);

                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                int fontSize = 8;
                if (headerTableDB.Rows.Count > 10) {
                    fontSize = 7;
                }

                Font thFont = new Font(Font.FontFamily.HELVETICA, fontSize, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, fontSize);
                BaseColor thColor = new BaseColor(240, 240, 240);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD);

                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Utils.CreateHeaderCell(hRow[0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT;

                        string colName = col.ColumnName.Trim();

                        bool isAmountColumn =
                            colName.Equals("SubTotal", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Discount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Net Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Taxable Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Tax Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Invoice Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Payment Amount", StringComparison.OrdinalIgnoreCase) ||
                            colName.Equals("Due Payment", StringComparison.OrdinalIgnoreCase);

                        string value = row[col]?.ToString() ?? "-";

                        if (isAmountColumn &&
                            decimal.TryParse(value, out decimal amount))
                        {
                            value = "₹ " + amount.ToString("N2");
                        }

                        string colNameLower = colName.ToLower();

                        if (colNameLower == "sr. no." ||
                            colNameLower == "invoice no" ||
                            colNameLower == "invoice date" ||
                            colNameLower == "booking id")
                        {
                            align = Element.ALIGN_CENTER;
                        }
                        else if (colNameLower == "client name" ||
                                 colNameLower == "payment modes")
                        {
                            align = Element.ALIGN_LEFT;
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
                //TOTALS
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    Font totalFont = new Font(
                        Font.FontFamily.HELVETICA,
                        fontSize,
                        Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        int align = Element.ALIGN_RIGHT;

                        string colName = col.ColumnName.Trim();

                        bool isAmountColumn = false;
                        string value = "";

                        switch (colName)
                        {
                            case "SubTotal":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalSubTotalAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Discount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDiscount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Net Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalNetAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Taxable Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalTaxableAmount"]).ToString("N2");
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

                            case "Payment Amount":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalPaymentAmount"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            case "Due Payment":
                                value = "₹ " + Convert.ToDecimal(totalRow["TotalDuePayment"]).ToString("N2");
                                isAmountColumn = true;
                                break;

                            default:
                                value = col.Ordinal == 0 ? "TOTAL" : "";
                                break;
                        }

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

                Utils.deleteOldFiles(folder, "DailySalesDetails_Report_*.pdf");

                string fileName = $"DailySalesDetails_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadDailySalesDateDetailsReport_Excel")]
        public IActionResult DownloadDailySalesDateDetailsReport_Excel([FromBody] DailySalesDateDetailsRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetDailySales_DateDetails_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;
                    cmd.Parameters.Add("@InvoiceDate", SqlDbType.Date).Value = (object?)request.InvoiceDate ?? DBNull.Value;

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data missing");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Daily Sales Details");

                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    ws.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    ws.Cell(1, col).Style.Font.Bold = true;
                    ws.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    ws.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                int row = 2;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        ws.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    row++;
                }

                //TOTALS
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string colName = dataTableDB.Columns[i].ColumnName;

                        string value = colName switch
                        {
                            "Qty" => totalRow["TotalInvoices"].ToString(),
                            "SubTotal" => totalRow["TotalSubTotalAmount"].ToString(),
                            "Discount" => totalRow["TotalDiscount"].ToString(),
                            "Net Amount" => totalRow["TotalNetAmount"].ToString(),

                            "Taxable Amount" => totalRow["TotalTaxableAmount"].ToString(),
                            "Tax Amount" => totalRow["TotalTaxAmount"].ToString(),
                            "Invoice Amount" => totalRow["TotalInvoiceAmount"].ToString(),
                            "Payment Amount" => totalRow["TotalPaymentAmount"].ToString(),

                            "Due Payment" => totalRow["TotalDuePayment"].ToString(),
                            _ => i == 0 ? "TOTAL" : ""
                        };

                        var cell = ws.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    row++;
                }

                ws.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "DailySalesDetails_Report_*.xlsx");

                string fileName = $"DailySalesDetails_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
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
