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
    public class InventoryController : MasterController
    {
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;
        private readonly string domainUrl;

        public InventoryController(IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, IWebHostEnvironment env) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            //_configuration = configuration;
            //string masterConnString = _configuration.GetConnectionString("ERPAdminDB_Master") ?? string.Empty;
            //_connClass = new ConnClass(httpContextAccessor, masterConnString);
        }

        [HttpPost]
        [Route("GetStockList")]
        public IActionResult GetStockList([FromBody] GetStockListRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetStockListModel> stocks = new List<GetStockListModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetStockList", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ItemCategoryId", request.ItemCategoryId == 0 ? 0 : request.ItemCategoryId);
                    cmd.Parameters.AddWithValue("@RawMaterialId", request.RawMaterialId == 0 ? 0 : request.RawMaterialId);
                    cmd.Parameters.AddWithValue("@UnitId", request.UnitId == 0 ? 0 : request.UnitId);

                    con.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        GetStockListModel m = new GetStockListModel();

                        m.RawMaterialId = Convert.ToInt64(row["RawMaterialId"]);
                        m.RawMaterialName = Convert.ToString(row["RawMaterialName"]);
                        m.ItemCategoryId = Convert.ToInt64(row["ItemCategoryId"]);
                        m.ItemCategoryName = Convert.ToString(row["ItemCategoryName"]);
                        m.UnitId = Convert.ToInt64(row["UnitId"]);
                        m.UnitName = Convert.ToString(row["UnitName"]);
                        m.CurrentQty = Convert.ToDecimal(row["CurrentQty"]);

                        stocks.Add(m);
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Stock list fetched successfully!";
                apiResponse.data = stocks;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("SaveStockInOut")]
        public IActionResult SaveStockInOut([FromBody] SaveStockInOutRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                // Convert StockInOutList to JSON
                string jsonStockInOutList = JsonConvert.SerializeObject(model.StockInOutList);

                using (SqlConnection conn = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveStockInOut", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@JSON_StockInOutList", SqlDbType.NVarChar).Value =
                        (object)jsonStockInOutList ?? DBNull.Value;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = model.ActionBy;

                    conn.Open();
                    // Read SQL return message (Success/Error)
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string status = reader["Status"].ToString();
                            string message = reader["Message"].ToString();

                            apiResponse.status = status;
                            apiResponse.message = message;
                            apiResponse.data = null;

                            if (status == "Error")
                                return BadRequest(apiResponse);   // return 400 for validation fail
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "StockOut saved successfully!";
                apiResponse.data = null;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetStockOutHistory")]
        public IActionResult GetStockOutHistory([FromBody] GetStockOutHistoryRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<GetStockOutHistoryList> list = new List<GetStockOutHistoryList>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                SqlConnection con = new SqlConnection(connString);
                SqlCommand cmd = new SqlCommand("GetStockOutHistory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@RawMaterialId", SqlDbType.BigInt).Value = request.RawMaterialId == 0 ? 0 : request.RawMaterialId;

                DataTable dt = new DataTable();

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                dt.Load(dr);
                con.Close();

                foreach (DataRow row in dt.Rows)
                {
                    GetStockOutHistoryList obj = new GetStockOutHistoryList();

                    obj.Id = row["Id"] == DBNull.Value ? 0 : Convert.ToInt64(row["Id"]);
                    obj.RawMaterialId = row["RawMaterialId"] == DBNull.Value ? 0 : Convert.ToInt64(row["RawMaterialId"]);
                    obj.RawMaterialName = row["RawMaterialName"]?.ToString() ?? "";

                    obj.UnitId = row["UnitId"] == DBNull.Value ? 0 : Convert.ToInt64(row["UnitId"]);
                    obj.UnitName = row["UnitName"]?.ToString() ?? "";

                    obj.Quantity_Out = row["Quantity_Out"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Quantity_Out"]);
                    obj.Quantity_In = row["Quantity_In"] == DBNull.Value ? 0 : Convert.ToDecimal(row["Quantity_In"]);
                    obj.Remarks = row["Remarks"]?.ToString() ?? "";

                    obj.ModifiedOn = Convert.ToString(row["ModifiedOn"]);
                    obj.ModifiedOn2 = row["ModifiedOn2"]?.ToString() ?? "";

                    obj.ModifiedBy = row["ModifiedBy"]?.ToString() ?? "";

                    list.Add(obj);
                }

                apiResponse.status = "Success";
                apiResponse.message = "Stock Out History fetched successfully!";
                apiResponse.data = list;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, connString);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("DownloadStockReport_Pdf")]
        public IActionResult DownloadStockReport_Pdf([FromBody] StockReportRequest request)
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

                    // Stock Report SP
                    cmd = new SqlCommand("Report_GetStock_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemCategoryId", request.ItemCategoryId);
                    cmd.Parameters.AddWithValue("@RawMaterialId", request.RawMaterialId);
                    cmd.Parameters.AddWithValue("@UnitId", request.UnitId);

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

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string colName = col.ColumnName.Trim().ToLower();
                        int align = colName switch
                        {
                            "sr. no." => Element.ALIGN_CENTER,
                            "category" => Element.ALIGN_LEFT,
                            "raw material" => Element.ALIGN_LEFT,
                            "unit" => Element.ALIGN_CENTER,
                            _ => Element.ALIGN_RIGHT
                        };

                        table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont, align));

                    }
                }

                // ── Totals row ──
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 8, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = col.ColumnName switch
                        {
                            "Stock Available" => totalRow["TotalStock"].ToString(),
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

                Utils.deleteOldFiles(folder, "Stock_Report_*.pdf");

                string fileName = $"Stock_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadStockReport_Excel")]
        public IActionResult DownloadStockReport_Excel([FromBody] StockReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetStock_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ItemCategoryId", request.ItemCategoryId);
                    cmd.Parameters.AddWithValue("@RawMaterialId", request.RawMaterialId);
                    cmd.Parameters.AddWithValue("@UnitId", request.UnitId);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Stock report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Stock Report");

                // ─────────────────────────────
                // Headers
                // ─────────────────────────────
                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    var cell = worksheet.Cell(1, col);
                    cell.Value = hRow["HeaderName"].ToString();
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // ─────────────────────────────
                // Data
                // ─────────────────────────────
                int row = 2;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = dr[i]?.ToString() ?? "-";

                        // Alignment logic
                        string colName = dataTableDB.Columns[i].ColumnName.ToLower();

                        cell.Style.Alignment.Horizontal = colName switch
                        {
                            "sr. no." => XLAlignmentHorizontalValues.Center,
                            "category" => XLAlignmentHorizontalValues.Left,
                            "raw material" => XLAlignmentHorizontalValues.Left,
                            "unit" => XLAlignmentHorizontalValues.Center,
                            _ => XLAlignmentHorizontalValues.Right
                        };
                    }
                    row++;
                }

                // ─────────────────────────────
                // Totals Row
                // ─────────────────────────────
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string value = columnName switch
                        {
                            "Stock Available" => totalRow["TotalStock"].ToString(),
                            _ => i == 0 ? "TOTAL" : ""
                        };

                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }
                }

                // Auto size
                worksheet.Columns().AdjustToContents();

                // ─────────────────────────────
                // Save File
                // ─────────────────────────────
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "Stock_Report_*.xlsx");

                string fileName = $"Stock_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Stock Report Excel generated successfully.";
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
