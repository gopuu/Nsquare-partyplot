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
    public class Report_PurchaseController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_PurchaseController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetPurchaseReportGrid")]
        public IActionResult GetPurchaseReportGrid([FromBody] PurchaseReportGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<PurchaseReportGridModel> rows = new List<PurchaseReportGridModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetPurchaseDetail_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SupplierId",
                        request.SupplierId == 0 ? 0 : request.SupplierId);
                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    con.Open();

                    using SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                foreach (DataRow row in dt.Rows)
                {
                    rows.Add(new PurchaseReportGridModel
                    {
                        SrNo = row["SrNo"] != DBNull.Value ? Convert.ToInt64(row["SrNo"]) : 0,
                        PurchaseId = row["PurchaseId"] != DBNull.Value ? Convert.ToInt64(row["PurchaseId"]) : 0,
                        InvoiceDate = row["InvoiceDate"] != DBNull.Value ? Convert.ToString(row["InvoiceDate"]) : "-",
                        InvoiceNo = row["InvoiceNo"] != DBNull.Value ? Convert.ToString(row["InvoiceNo"]) : "-",
                        SupplierName = row["SupplierName"] != DBNull.Value ? Convert.ToString(row["SupplierName"]) : "-",
                        SupplierId = row["SupplierId"] != DBNull.Value ? Convert.ToInt64(row["SupplierId"]) : 0,
                        GrandTotal = row["GrandTotal"] != DBNull.Value ? Convert.ToDecimal(row["GrandTotal"]) : 0,
                        PaidAmount = row["PaidAmount"] != DBNull.Value ? Convert.ToDecimal(row["PaidAmount"]) : 0,
                        Outstanding = row["Outstanding"] != DBNull.Value ? Convert.ToDecimal(row["Outstanding"]) : 0
                    });
                }

                apiResponse.message = "Purchase report fetched successfully!";
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
        [Route("DownloadPurchaseReport")]
        public IActionResult DownloadPurchaseReport([FromBody] PurchaseReportRequest request)
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
                    cmd = new SqlCommand("Report_GetPurchaseDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns",
                        columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplierId",
                        request.SupplierId == 0 ? 0 : request.SupplierId);
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

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));

                // ── Totals row ──
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 9, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = col.ColumnName switch
                        {
                            "Grand Total" => $"{totalRow["TotalGrandTotal"]}",
                            "Paid Amount" => $"{totalRow["TotalPaidAmount"]}",
                            "Outstanding" => $"{totalRow["TotalOutstanding"]}",
                            _ => col.Ordinal == 0 ? "TOTAL" : ""
                        };

                        table.AddCell(new PdfPCell(new Phrase(value, totalFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
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

                Utils.deleteOldFiles(folder, "Purchase_Report_*.pdf");

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadPurchaseReport_Excel")]
        public IActionResult DownloadPurchaseReport_Excel([FromBody] PurchaseReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetPurchaseDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string? columnsJson = request.Columns != null ? JsonConvert.SerializeObject(request.Columns) : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SupplierId", request.SupplierId == 0 ? 0 : request.SupplierId);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Purchase Report");

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
                            "Grand Total" => totalRow["TotalGrandTotal"].ToString(),
                            "Paid Amount" => totalRow["TotalPaidAmount"].ToString(),
                            "Outstanding" => totalRow["TotalOutstanding"].ToString(),
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

                Utils.deleteOldFiles(folder, "Purchase_Report_*.xlsx");

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);
                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Purchase Report Excel generated successfully.";
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

        //Purchase Invoice - Reports
        [HttpPost]
        [Route("Report_GetPurchaseInvoice_GRID")]
        public IActionResult Report_GetPurchaseInvoice_GRID([FromBody] GetPurchaseRequestGRID request)
        {
            apiResponse apiResponse = new apiResponse();
            GetPurchaseListModelGRID model = new GetPurchaseListModelGRID();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString,
                    out string domain,
                    out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetPurchaseInvoice_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PurchaseId",
                        request.PurchaseId > 0 ? request.PurchaseId : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@InvoiceNo",
                        string.IsNullOrEmpty(request.InvoiceNo)
                            ? (object)DBNull.Value
                            : request.InvoiceNo);

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ds);
                }

                if (ds.Tables.Count < 3)
                    throw new Exception("Invoice data incomplete.");

                DataTable headerTable = ds.Tables[0];
                DataTable itemsTable = ds.Tables[1];
                DataTable totalsTable = ds.Tables[2];

                if (headerTable.Rows.Count == 0)
                    throw new Exception("Invoice not found.");

                DataRow h = headerTable.Rows[0];

                // ───────── HEADER ─────────
                model.Id = Convert.ToInt64(h["Id"]);
                model.PurchaseFrom = h["PurchaseFrom"] == DBNull.Value ? 0 : Convert.ToInt64(h["PurchaseFrom"]);
                model.PurchaseFromName = Convert.ToString(h["PurchaseFromName"]);

                model.SupplierId = h["SupplierId"] == DBNull.Value ? 0 : Convert.ToInt64(h["SupplierId"]);
                model.SupplierName = Convert.ToString(h["SupplierName"]);
                model.SupplierCompany = Convert.ToString(h["SupplierCompany"]);
                model.SupplierPhone = Convert.ToString(h["SupplierPhone"]);

                model.InvoiceDate = Convert.ToString(h["InvoiceDate"]);
                model.InvoiceDate_Format = Convert.ToString(h["InvoiceDate_Format"]);
                model.InvoiceNo = Convert.ToString(h["InvoiceNo"]);
                model.PONo = Convert.ToString(h["PONo"]);


                // ───────── ITEMS ─────────
                List<GetPurchaseItemsGRID> items = new();

                foreach (DataRow row in itemsTable.Rows)
                {
                    items.Add(new GetPurchaseItemsGRID
                    {
                        RawMaterial = Convert.ToString(row["RawMaterial"]),
                        Qty = row["Qty"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Qty"]),
                        UnitName = Convert.ToString(row["UnitName"]),
                        Price = row["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Price"]),
                        BaseAmount = row["BaseAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["BaseAmount"]),
                        CGST = row["CGST"] == DBNull.Value ? 0 : Convert.ToDecimal(row["CGST"]),
                        SGST = row["SGST"] == DBNull.Value ? 0 : Convert.ToDecimal(row["SGST"]),
                        IGST = row["IGST"] == DBNull.Value ? 0 : Convert.ToDecimal(row["IGST"]),
                        CGST_Amount = row["CGST_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["CGST_Amount"]),
                        SGST_Amount = row["SGST_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["SGST_Amount"]),
                        IGST_Amount = row["IGST_Amount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["IGST_Amount"]),
                        TotalGST = row["TotalGST"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalGST"]),
                        TotalAmount = row["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalAmount"])
                    });
                }

                model.JSON_PurchaseItems = items;

                // ───────── TOTALS ─────────
                if (totalsTable.Rows.Count > 0)
                {
                    DataRow t = totalsTable.Rows[0];

                    model.SubTotal_TotalDiscount_Amount = t["SubTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(t["SubTotal"]);
                    model.SubTotal_OtherCharges_Amount = t["OtherCharges"] == DBNull.Value ? 0 : Convert.ToDecimal(t["OtherCharges"]);
                    model.SubTotal_OtherTaxes_Amount = t["OtherTaxes"] == DBNull.Value ? 0 : Convert.ToDecimal(t["OtherTaxes"]);
                    model.TotalDiscountInInvoice = t["Discount"] == DBNull.Value ? 0 : Convert.ToDecimal(t["Discount"]);
                    model.GrandTotal_Amount = t["GrandTotal"] == DBNull.Value ? 0 : Convert.ToDecimal(t["GrandTotal"]);
                }


                apiResponse.status = "Success";
                apiResponse.message = "Purchase invoice fetched successfully!";
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
        [Route("DownloadPurchaseInvoiceReport_Pdf")]
        public IActionResult DownloadPurchaseInvoiceReport_Pdf([FromBody] PurchaseInvoiceReportRequest request)
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
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // Purchase Invoice Report
                    cmd = new SqlCommand("Report_GetPurchaseInvoice_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PurchaseId", request.PurchaseId > 0 ? request.PurchaseId : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@InvoiceNo",
                        string.IsNullOrEmpty(request.InvoiceNo) ? (object)DBNull.Value : request.InvoiceNo);

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

                DataRow inv = totalTableDB.Rows[0];

                // ── Report title ──
                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                // ── Supplier / Invoice info ──
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 50f, 50f });

                infoTable.AddCell(Utils.CreateInfoCell("Invoice No", inv["InvoiceNo"]));
                infoTable.AddCell(Utils.CreateInfoCell("Invoice Date", inv["InvoiceDate_Format"]));
                infoTable.AddCell(Utils.CreateInfoCell("Supplier", inv["SupplierName"]));
                infoTable.AddCell(Utils.CreateInfoCell("Company", inv["SupplierCompany"]));
                infoTable.AddCell(Utils.CreateInfoCell("Phone", inv["SupplierPhone"]));
                infoTable.AddCell(Utils.CreateInfoCell("PO No", inv["PONo"]));

                document.Add(infoTable);
                document.Add(new Paragraph("\n"));

                // ── Items table ──
                int columnCount = dataTableDB.Columns.Count;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 6, 30, 10, 10, 10, 10, 10, 10, 10 });

                Font thFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 8);
                BaseColor thColor = new BaseColor(240, 240, 240);

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.ToLower();

                        int align = colName switch
                        {
                            "sr. no." => Element.ALIGN_CENTER,
                            "raw material" => Element.ALIGN_LEFT,
                            "qty" => Element.ALIGN_CENTER,
                            "unit" => Element.ALIGN_CENTER,
                            _ => Element.ALIGN_RIGHT
                        };

                        table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont, align));
                    }
                }

                document.Add(table);

                // ── Totals ──
                document.Add(new Paragraph("\n"));

                PdfPTable totalTable = new PdfPTable(2);
                totalTable.WidthPercentage = 40;
                totalTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalTable.SetWidths(new float[] { 60f, 40f });

                totalTable.AddCell(Utils.CreateTotalLabelCell("Sub Total"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["SubTotal"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Other Charges"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["OtherCharges"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Other Taxes"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["OtherTaxes"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Discount"));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["Discount"]));

                totalTable.AddCell(Utils.CreateTotalLabelCell("Grand Total", true));
                totalTable.AddCell(Utils.CreateTotalValueCell(inv["GrandTotal"], true));

                document.Add(totalTable);

                document.Close();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "PurchaseInvoice_*.pdf");

                string identifier = request.InvoiceNo ?? request.PurchaseId.ToString();
                string fileName = $"PurchaseInvoice_{identifier}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fs);

                response.status = "Success";
                response.message = "Purchase Invoice Generated";
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
        [Route("DownloadPurchaseInvoiceReport_Excel")]
        public IActionResult DownloadPurchaseInvoiceReport_Excel([FromBody] PurchaseInvoiceReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetPurchaseInvoice_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@PurchaseId",
                        request.PurchaseId > 0 ? request.PurchaseId : (object)DBNull.Value);

                    cmd.Parameters.AddWithValue("@InvoiceNo",
                        string.IsNullOrEmpty(request.InvoiceNo) ? (object)DBNull.Value : request.InvoiceNo);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 4)
                    throw new Exception("Purchase invoice data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables[3];

                DataRow inv = totalTableDB.Rows[0];

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Purchase Invoice");

                int row = 1;

                // ── Invoice Info ──
                worksheet.Cell(row, 1).Value = "Invoice No";
                worksheet.Cell(row, 2).Value = inv["InvoiceNo"].ToString();

                worksheet.Cell(row, 4).Value = "Invoice Date";
                worksheet.Cell(row, 5).Value = inv["InvoiceDate_Format"].ToString();
                row++;

                worksheet.Cell(row, 1).Value = "Supplier";
                worksheet.Cell(row, 2).Value = inv["SupplierName"].ToString();

                worksheet.Cell(row, 4).Value = "Company";
                worksheet.Cell(row, 5).Value = inv["SupplierCompany"].ToString();
                row++;

                worksheet.Cell(row, 1).Value = "Phone";
                worksheet.Cell(row, 2).Value = inv["SupplierPhone"].ToString();

                worksheet.Cell(row, 4).Value = "PO No";
                worksheet.Cell(row, 5).Value = inv["PONo"].ToString();

                row += 2;

                // ── Headers ──
                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(row, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                row++;

                // ── Data ──
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";

                    row++;
                }

                // ── Totals ──
                row++;

                worksheet.Cell(row, 4).Value = "Sub Total";
                worksheet.Cell(row, 5).Value = inv["SubTotal"].ToString();
                row++;

                worksheet.Cell(row, 4).Value = "Other Charges";
                worksheet.Cell(row, 5).Value = inv["OtherCharges"].ToString();
                row++;

                worksheet.Cell(row, 4).Value = "Other Taxes";
                worksheet.Cell(row, 5).Value = inv["OtherTaxes"].ToString();
                row++;

                worksheet.Cell(row, 4).Value = "Discount";
                worksheet.Cell(row, 5).Value = inv["Discount"].ToString();
                row++;

                worksheet.Cell(row, 4).Value = "Grand Total";
                worksheet.Cell(row, 5).Value = inv["GrandTotal"].ToString();

                worksheet.Range(row, 4, row, 5).Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();

                // ── Save ──
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "PurchaseInvoice_*.xlsx");

                string? identifier = request.InvoiceNo ?? request.PurchaseId.ToString();
                string fileName = $"PurchaseInvoice_{identifier}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                string filePath = Path.Combine(folder, fileName);
                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Purchase Invoice Excel generated successfully.";
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
        //Purchase Invoice - Reports
    }
}
