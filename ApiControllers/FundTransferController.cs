using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Web.Helpers;
using DocumentFormat.OpenXml.Office.Word;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;


namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundTransferController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;
        public FundTransferController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
            _env = env;
        }

        [HttpPost]
        [Route("GetFundTransferList")]
        public IActionResult GetFundTransferList([FromBody] GetFundTransferRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            List<FundTransferModel> list = new List<FundTransferModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetFundTransfer", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@FromAccountId", request.FromAccountId);
                    cmd.Parameters.AddWithValue("@ToAccountId", request.ToAccountId);
                    cmd.Parameters.AddWithValue("@Amount", request.Amount);

                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);

                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ds);
                }

                if (ds.Tables.Count == 0)
                    throw new Exception("Fund transfer data not found.");

                DataTable dt = ds.Tables[0];

                foreach (DataRow row in dt.Rows)
                {
                    list.Add(new FundTransferModel
                    {
                        Id = row["Id"] != DBNull.Value ? Convert.ToInt64(row["Id"]) : 0,
                        FromAccountId = row["FromAccountId"] != DBNull.Value ? Convert.ToInt64(row["FromAccountId"]) : 0,
                        FromAccountName = row["FromAccountName"]?.ToString(),
                        ToAccountId = row["ToAccountId"] != DBNull.Value ? Convert.ToInt64(row["ToAccountId"]) : 0,
                        ToAccountName = row["ToAccountName"]?.ToString(),
                        Amount = row["Amount"] != DBNull.Value ? Convert.ToDecimal(row["Amount"]) : 0,
                        TransferDate = row["TransferDate"]?.ToString(),
                        TransferDate_Format = row["TransferDate_Format"]?.ToString(),
                        ReferenceNo = row["ReferenceNo"]?.ToString(),
                        Notes = row["Notes"]?.ToString()
                    });
                }

                decimal totalAmount = 0;

                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    totalAmount = ds.Tables[1].Rows[0]["TotalAmount"] != DBNull.Value
                        ? Convert.ToDecimal(ds.Tables[1].Rows[0]["TotalAmount"])
                        : 0;
                }

                apiResponse.status = "Success";
                apiResponse.message = "Fund transfers fetched successfully!";
                apiResponse.data = new
                {
                    Rows = list,
                    Totals = totalAmount
                };

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
        [Route("SaveFundTransfer")]
        public IActionResult SaveFundTransfer([FromBody] SaveFundTransferRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                long rowId = 0;
                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveFundTransfer", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@FromAccountId", model.FromAccountId);
                    cmd.Parameters.AddWithValue("@ToAccountId", model.ToAccountId);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@TransferDate", model.TransferDate);
                    cmd.Parameters.AddWithValue("@ReferenceNo", (object)model.ReferenceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Notes", (object)model.Notes ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    rowId = Convert.ToInt64(cmd.ExecuteScalar());
                }

                apiResponse.status = "Success";
                apiResponse.message = model.Id > 0 ? "Fund transfer updated successfully!" : "Fund transfer inserted successfully!";
                apiResponse.data = new { FundTransferId = rowId };
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
        [Route("DeleteFundTransfer")]
        public IActionResult DeleteFundTransfer([FromBody] DeleteFundTransferRequest model)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteFundTransfer", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Fund transfer deleted successfully!";
                apiResponse.data = null;
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
        [Route("DownloadFundTransferReport_Pdf")]
        public IActionResult DownloadFundTransferReport_Pdf([FromBody] FundTransferReportRequest request)
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

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
                    cmd = new SqlCommand("Report_GetFundTransfer_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@FromAccountId", request.FromAccountId);
                    cmd.Parameters.AddWithValue("@ToAccountId", request.ToAccountId);
                    cmd.Parameters.AddWithValue("@Amount", request.Amount);
                    cmd.Parameters.AddWithValue("@FromDate",
                        string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate",
                        string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@FilterDuration",
                        string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

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
                    throw new Exception("Report data is incomplete.");

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                writer.PageEvent = new PdfPageEvents("");
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


                document.Add(new Paragraph(reportName.ToUpper(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 18f
                });

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;
                table.SetWidths(Enumerable.Repeat(1f, columnCount).ToArray());

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
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
                            "reference #" => Element.ALIGN_CENTER,
                            "transfer date" => Element.ALIGN_CENTER,
                            "from account" => Element.ALIGN_CENTER,
                            "to account" => Element.ALIGN_CENTER,
                            "notes" => Element.ALIGN_LEFT,
                            _ => Element.ALIGN_RIGHT
                        };

                        table.AddCell(Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont, align));
                    }
                }

                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];
                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = col.ColumnName switch
                        {
                            "Amount" => $"{totalRow["TotalAmount"]}",
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

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "FundTransfer_Report_*.pdf");

                string fileName = $"FundTransfer_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    stream.WriteTo(fs);

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
        [Route("DownloadFundTransferReport_Excel")]
        public IActionResult DownloadFundTransferReport_Excel([FromBody] FundTransferReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetFundTransfer_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@FromAccountId", request.FromAccountId);
                    cmd.Parameters.AddWithValue("@ToAccountId", request.ToAccountId);
                    cmd.Parameters.AddWithValue("@Amount", request.Amount);
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
                var worksheet = workbook.Worksheets.Add("Fund Transfer Report");

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
                            "Amount" => totalRow["TotalAmount"].ToString(),
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

                Utils.deleteOldFiles(folder, "FundTransfer_Report_*.xlsx");

                string fileName = $"FundTransfer_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Fund Transfer Excel generated successfully.";
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
