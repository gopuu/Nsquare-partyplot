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
    public class Report_BookingController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_BookingController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetBookingReportGrid")]
        public IActionResult GetBookingReportGrid([FromBody] BookingReportRequestGRID request)
        {
            apiResponse apiResponse = new apiResponse();
            List<BookingReportGridModel> bookings = new List<BookingReportGridModel>();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetBookingDetails_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId == 0 ? 0 : request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId == 0 ? 0 : request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId == 0 ? 0 : request.HallId);
                    cmd.Parameters.AddWithValue("@FromDate", string.IsNullOrEmpty(request.FromDate) ? (object)DBNull.Value : request.FromDate);
                    cmd.Parameters.AddWithValue("@ToDate", string.IsNullOrEmpty(request.ToDate) ? (object)DBNull.Value : request.ToDate);
                    cmd.Parameters.AddWithValue("@MonthDuration", string.IsNullOrEmpty(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);

                    con.Open();

                    SqlDataReader dr = cmd.ExecuteReader();
                    dt.Load(dr);
                }

                foreach (DataRow row in dt.Rows)
                {
                    bookings.Add(new BookingReportGridModel
                    {
                        SrNo = Convert.ToInt64(row["SrNo"]),
                        EventDate = Convert.ToString(row["EventDate"]),
                        EventTypeName = Convert.ToString(row["EventTypeName"]),
                        EventSubTypeName = Convert.ToString(row["EventSubTypeName"]),
                        EventDisplay = Convert.ToString(row["EventDisplay"]),

                        HallName = Convert.ToString(row["HallName"]),
                        Pax = Convert.ToInt32(row["Pax"]),
                        Package = Convert.ToString(row["Package"]),

                        InvoiceAmount = Convert.ToDecimal(row["InvoiceAmount"]),
                        DealAmount = Convert.ToDecimal(row["DealAmount"]),
                        Discount = Convert.ToDecimal(row["Discount"]),
                        Tax = Convert.ToDecimal(row["Tax"]),

                        BookingId = Convert.ToInt64(row["BookingId"]),
                        Client_Name = Convert.ToString(row["Client_Name"]),
                        Client_Contact = Convert.ToString(row["Client_Contact"]),
                        Client_Email = Convert.ToString(row["Client_Email"]),
                        InvoiceNo = Convert.ToInt64(row["InvoiceNo"])

                    });
                }

                apiResponse.message = "Booking report fetched successfully!";
                apiResponse.data = bookings;
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

        #region REPORT 1 - Booking REPORT
        [HttpPost]
        [Route("DownloadBookingReport")]
        public IActionResult DownloadBookingReport([FromBody] BookingReportRequest request)
        {
            apiResponse response = new apiResponse();

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

                float topMargin = (!request.WithHeader ?? false) ? 125f : 36f;

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4.Rotate(), 36, 36, topMargin, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // 🔹 Report
                    cmd = new SqlCommand("Report_GetBookingDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;
                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);
                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MonthDuration", request.FilterDuration ?? (object)DBNull.Value);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================

                string companyName = string.Empty;
                string companyAddress = string.Empty;
                string companyphoneNo = string.Empty;
                string logoFile = string.Empty;
                string userName = string.Empty;

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
                    throw new Exception("Report data is incomplete. Required tables are missing.");

                // ================= EXTRACT DATA =================

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"]?.ToString() ?? "";

                writer.PageEvent = new PdfPageEvents(userName);
                document.Open();

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                // ================= HEADER =================

                if (request.WithHeader ?? false)
                {
                    PdfPTable headerTable = new PdfPTable(2);
                    headerTable.WidthPercentage = 100;
                    headerTable.SetWidths(new float[] { 20f, 80f });

                    PdfPCell logoCell;

                    if (logo != null)
                    {
                        logo.ScaleToFit(70f, 70f);
                        logoCell = new PdfPCell(logo);
                    }
                    else
                    {
                        logoCell = new PdfPCell(new Phrase(""));
                    }

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

                // ================= REPORT TITLE =================

                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);

                // ================= TABLE =================

                int columnCount = dataTableDB.Columns.Count;

                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                for (int i = 0; i < headerTableDB.Rows.Count; i++)
                {
                    table.AddCell(Utils.CreateHeaderCell(headerTableDB.Rows[i][0].ToString(), thFont, thColor));
                }

                table.HeaderRows = 1;

                foreach (DataRow row in dataTableDB.Rows)
                {
                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        table.AddCell(Utils.CreateBodyCell(
                            row[col]?.ToString() ?? "-",
                            tdFont
                        ));
                    }
                }

                //SUMMARY TOTALS
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    Font totalFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD);

                    foreach (DataColumn col in dataTableDB.Columns)
                    {
                        string value = "";

                        switch (col.ColumnName)
                        {
                            case "Pax":
                                value = $"{totalRow["TotalPax"]}";
                                break;

                            case "Deal Amount":
                                value = $"{totalRow["TotalDealAmount"]}";
                                break;

                            case "Discount":
                                value = $"{totalRow["TotalDiscount"]}";
                                break;

                            case "Tax":
                                value = $"{totalRow["TotalTax"]}";
                                break;

                            case "Invoice Amount":
                                value = $"{totalRow["TotalInvoiceAmount"]}";
                                break;

                            default:
                                value = "";
                                if (col.Ordinal == 0)
                                {
                                    value = "TOTAL";
                                }
                                break;
                        }

                        PdfPCell cell = new PdfPCell(new Phrase(value, totalFont))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT,
                            BackgroundColor = new BaseColor(240, 240, 240),
                            Padding = 6
                        };

                        table.AddCell(cell);
                    }
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================

                string folder = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                string fileName = $"Booking_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Booking_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
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
        [Route("DownloadBookingReport_Excel")]
        public IActionResult DownloadBookingReport_Excel([FromBody] BookingReportRequest request)
        {
            apiResponse response = new apiResponse();

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

                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    SqlCommand cmd = new SqlCommand("Report_GetBookingDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // SAME LOGIC AS PDF
                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.AddWithValue("@NameEmailContactNo", string.IsNullOrEmpty(request.NameEmailContactNo) ? (object)DBNull.Value : request.NameEmailContactNo);
                    cmd.Parameters.AddWithValue("@Columns", columnsJson ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EventTypeId", request.EventTypeId);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", request.EventSubTypeId);
                    cmd.Parameters.AddWithValue("@HallId", request.HallId);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MonthDuration", request.FilterDuration ?? (object)DBNull.Value);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("BookingReport");

                int col = 1;

                foreach (DataRow header in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = header["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                int row = 2;

                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    }
                    row++;
                }

                // Add TOTAL ROW
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;
                        string value = "";

                        switch (columnName)
                        {
                            case "Pax":
                                value = totalRow["TotalPax"].ToString();
                                break;

                            case "Deal Amount":
                                value = totalRow["TotalDealAmount"].ToString();
                                break;

                            case "Discount":
                                value = totalRow["TotalDiscount"].ToString();
                                break;

                            case "Tax":
                                value = totalRow["TotalTax"].ToString();
                                break;

                            case "Invoice Amount":
                                value = totalRow["TotalInvoiceAmount"].ToString();
                                break;

                            default:
                                if (i == 0)
                                    value = "TOTAL";
                                break;
                        }

                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();

                string reportsDir = Path.Combine(_env.WebRootPath, "reports");

                if (!Directory.Exists(reportsDir))
                    Directory.CreateDirectory(reportsDir);

                Utils.deleteOldFiles(reportsDir, "BookingReport_*.xlsx");

                string fileName = $"BookingReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(reportsDir, fileName);

                workbook.SaveAs(filePath);

                string fileUrl = $"{domainUrl}/reports/{fileName}";

                response.status = "Success";
                response.message = "Booking Report Excel generated successfully.";
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
        #endregion

    }
}
