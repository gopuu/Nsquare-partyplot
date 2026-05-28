using ClosedXML.Excel;
//using DocumentFormat.OpenXml.EMMA;
//using DocumentFormat.OpenXml.Office2010.Excel;
//using DocumentFormat.OpenXml.Office2016.Excel;
using ERPAdmin.ApiControllers;
using ERPAdmin.Helper;
using ERPAdmin.Models;
using ERPAdmin.PDF;
using ERPAdmin.Services;
using Humanizer;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Razorpay.Api;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Web.Helpers;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerInvoiceController : MasterController
    {
        private readonly string domainUrl;
        private string connString = string.Empty;
        private readonly IWebHostEnvironment _env;
        private readonly WhatsAppService _whatsAppService;
        private readonly IInvoicePdfService _invoicePdfService;

        public CustomerInvoiceController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, 
            WhatsAppService whatsAppService, IInvoicePdfService invoicePdfService) : base(configuration, httpContextAccessor) 
        {
            _env = env;
            _whatsAppService = whatsAppService;
            _invoicePdfService = invoicePdfService;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetCustomerInvoice_GRID")]
        public IActionResult GetCustomerInvoice_GRID(CustomerInvoiceRequest_GRID request)
        {
            apiResponse apiResponse = new apiResponse();
            List<CustomerInvoiceResponse_GRID> list = new List<CustomerInvoiceResponse_GRID>();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoice_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@NameEmailContact", SqlDbType.NVarChar, 100).Value = (object?)request.NameEmailContact ?? DBNull.Value;
                    cmd.Parameters.Add("@SlotId", SqlDbType.BigInt).Value = request.SlotId;
                    cmd.Parameters.Add("@EventFrom", SqlDbType.NVarChar, 20).Value = (object?)request.EventFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@EventTo", SqlDbType.NVarChar, 20).Value = (object?)request.EventTo ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@FilterDuration",string.IsNullOrWhiteSpace(request.FilterDuration) ? (object)DBNull.Value : request.FilterDuration);
                    cmd.Parameters.Add("@BookedFrom", SqlDbType.NVarChar, 20).Value = (object?)request.BookedFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@BookedTill", SqlDbType.NVarChar, 20).Value = (object?)request.BookedTill ?? DBNull.Value;
                    cmd.Parameters.Add("@SubEventId", SqlDbType.BigInt).Value = request.SubEventId;
                    cmd.Parameters.Add("@PaymentStatusId", SqlDbType.BigInt).Value = request.PaymentStatusId;
                    cmd.Parameters.Add("@IsInvoiceGenerated", SqlDbType.Bit).Value = (object?)request.IsInvoiceGenerated ?? DBNull.Value;
                    cmd.Parameters.Add("@IsExclusiveOfGST", SqlDbType.Bit).Value = (object?)request.IsExclusiveOfGST ?? DBNull.Value;
                    cmd.Parameters.Add("@IsInclusiveOfGST", SqlDbType.Bit).Value = (object?)request.IsInclusiveOfGST ?? DBNull.Value;
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        /* =========================
                           🔹 RESULT 1: ROWS
                        ========================= */
                        while (dr.Read())
                        {
                            list.Add(new CustomerInvoiceResponse_GRID
                            {
                                BookingId = Convert.ToInt64(dr["BookingId"]),
                                ProformaInvoiceNo = Convert.ToInt64(dr["ProformaInvoiceNo"]),
                                FinalInvoiceNo = Convert.ToString(dr["FinalInvoiceNo"]),
                                InvoiceNo = Convert.ToInt64(dr["InvoiceNo"]),
                                GSTInvoiceNo = Convert.ToString(dr["GSTInvoiceNo"]),
                                IsExclusiveOfGST = Convert.ToBoolean(dr["IsExclusiveOfGST"]),
                                IsInclusiveOfGST = Convert.ToBoolean(dr["IsInclusiveOfGST"]),
                                EventDate = dr["EventDate"].ToString(),
                                EventDate_Format = dr["EventDate_Format"].ToString(),
                                IsInvoiceBlocked = Convert.ToBoolean(dr["IsInvoiceBlocked"]),
                                BookedOn = dr["BookedOn"].ToString(),
                                EventName = dr["EventName"].ToString(),
                                HallName = dr["HallName"].ToString(),
                                ClientName = dr["ClientName"].ToString(),
                                ClientContactNo = dr["ClientContactNo"].ToString(),
                                TotalAmount = Convert.ToDecimal(dr["TotalAmount"]),
                                PaidAmount = Convert.ToDecimal(dr["PaidAmount"]), // NEW
                                OutstandingAmount = Convert.ToDecimal(dr["OutstandingAmount"])
                            });
                        }

                        /* =========================
                           🔹 RESULT 2: TOTALS
                        ========================= */
                        decimal totalAmount = 0;
                        decimal totalPaid = 0;
                        decimal totalOutstanding = 0;
                        int totalBookings = 0;

                        if (dr.NextResult())
                        {
                            if (dr.Read())
                            {
                                totalBookings = Convert.ToInt32(dr["TotalBookings"]);
                                totalAmount = dr["TotalAmount"] != DBNull.Value ? Convert.ToDecimal(dr["TotalAmount"]) : 0;
                                totalPaid = dr["TotalPaid"] != DBNull.Value ? Convert.ToDecimal(dr["TotalPaid"]) : 0;
                                totalOutstanding = dr["TotalOutstanding"] != DBNull.Value ? Convert.ToDecimal(dr["TotalOutstanding"]) : 0;
                            }
                        }

                        /* =========================
                           🔹 FINAL RESPONSE
                        ========================= */
                        apiResponse.status = "Success";
                        apiResponse.message = "Customer invoice fetched successfully.";
                        apiResponse.data = new
                        {
                            Rows = list,
                            Totals = new
                            {
                                TotalBookings = totalBookings,
                                TotalAmount = totalAmount,
                                TotalPaid = totalPaid,
                                TotalOutstanding = totalOutstanding
                            }
                        };
                    }
                }

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
        [Route("DownloadCustomerInvoiceReport_Pdf")]
        public IActionResult DownloadCustomerInvoiceReport_Pdf([FromBody] Report_CustomerInvoiceRequest request)
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

                    // ✅ Company Header
                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@ActionBy", SqlDbType.BigInt).Value = request.ActionBy;
                    new SqlDataAdapter(cmd).Fill(dtCompany);

                    // ✅ Report SP
                    cmd = new SqlCommand("Report_GetCustomerInvoice_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;

                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@NameEmailContact", SqlDbType.NVarChar).Value = (object?)request.NameEmailContact ?? DBNull.Value;
                    cmd.Parameters.Add("@SlotId", SqlDbType.BigInt).Value = request.SlotId;

                    cmd.Parameters.Add("@EventFrom", SqlDbType.NVarChar, 20).Value = (object?)request.EventFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@EventTo", SqlDbType.NVarChar, 20).Value = (object?)request.EventTo ?? DBNull.Value;

                    cmd.Parameters.Add("@BookedFrom", SqlDbType.NVarChar, 20).Value = (object?)request.BookedFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@BookedTill", SqlDbType.NVarChar, 20).Value = (object?)request.BookedTill ?? DBNull.Value;

                    cmd.Parameters.Add("@SubEventId", SqlDbType.BigInt).Value = request.SubEventId;

                    cmd.Parameters.Add("@IsInvoiceGenerated", SqlDbType.Bit).Value =
                        request.IsInvoiceGenerated.HasValue ? (object)request.IsInvoiceGenerated.Value : DBNull.Value;

                    cmd.Parameters.Add("@PaymentStatusId", SqlDbType.BigInt).Value = request.PaymentStatusId;

                    cmd.Parameters.Add("@IsExclusiveOfGST", SqlDbType.Bit).Value =
                        request.IsExclusiveOfGST.HasValue ? (object)request.IsExclusiveOfGST.Value : DBNull.Value;

                    cmd.Parameters.Add("@IsInclusiveOfGST", SqlDbType.Bit).Value =
                        request.IsInclusiveOfGST.HasValue ? (object)request.IsInclusiveOfGST.Value : DBNull.Value;

                    cmd.Parameters.Add("@FilterDuration", SqlDbType.NVarChar, 50).Value =
                        (object?)request.FilterDuration ?? DBNull.Value;

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

                // Title
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

                Font thFont = new Font(Font.FontFamily.HELVETICA, 7, Font.BOLD, BaseColor.BLACK);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 7);
                BaseColor thColor = new BaseColor(240, 240, 240);

                // Header row
                foreach (DataRow hRow in headerTableDB.Rows)
                    table.AddCell(Helper.Utils.CreateHeaderCell(hRow[0].ToString(), thFont, thColor));

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
                            "event date" => Element.ALIGN_CENTER,
                            "event name" => Element.ALIGN_LEFT,
                            "hall name" => Element.ALIGN_LEFT,
                            "client name" => Element.ALIGN_LEFT,
                            "contact no" => Element.ALIGN_CENTER,
                            _ => Element.ALIGN_RIGHT
                        };

                        table.AddCell(Helper.Utils.CreateBodyCell(row[col]?.ToString() ?? "-", tdFont, align));
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
                            "Total Amount" => totalRow["TotalAmount"].ToString() ?? "-",
                            "Total Outstanding" => totalRow["TotalOutstanding"].ToString() ?? "-",
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

                //SAVE
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Helper.Utils.deleteOldFiles(folder, "CustomerInvoice_*.pdf");

                string fileName = $"CustomerInvoice_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadCustomerInvoiceReport_Excel")]
        public IActionResult DownloadCustomerInvoiceReport_Excel([FromBody] Report_CustomerInvoiceRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetCustomerInvoice_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    string columnsJson = request.Columns != null
                        ? JsonConvert.SerializeObject(request.Columns)
                        : null;

                    // ✅ Parameters (same as SP)
                    cmd.Parameters.Add("@Columns", SqlDbType.NVarChar).Value = (object?)columnsJson ?? DBNull.Value;

                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@NameEmailContact", SqlDbType.NVarChar).Value = (object?)request.NameEmailContact ?? DBNull.Value;
                    cmd.Parameters.Add("@SlotId", SqlDbType.BigInt).Value = request.SlotId;

                    cmd.Parameters.Add("@EventFrom", SqlDbType.NVarChar, 20).Value = (object?)request.EventFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@EventTo", SqlDbType.NVarChar, 20).Value = (object?)request.EventTo ?? DBNull.Value;

                    cmd.Parameters.Add("@BookedFrom", SqlDbType.NVarChar, 20).Value = (object?)request.BookedFrom ?? DBNull.Value;
                    cmd.Parameters.Add("@BookedTill", SqlDbType.NVarChar, 20).Value = (object?)request.BookedTill ?? DBNull.Value;

                    cmd.Parameters.Add("@SubEventId", SqlDbType.BigInt).Value = request.SubEventId;

                    cmd.Parameters.Add("@IsInvoiceGenerated", SqlDbType.Bit).Value =
                        request.IsInvoiceGenerated.HasValue ? (object)request.IsInvoiceGenerated.Value : DBNull.Value;

                    cmd.Parameters.Add("@PaymentStatusId", SqlDbType.BigInt).Value = request.PaymentStatusId;

                    cmd.Parameters.Add("@IsExclusiveOfGST", SqlDbType.Bit).Value =
                        request.IsExclusiveOfGST.HasValue ? (object)request.IsExclusiveOfGST.Value : DBNull.Value;

                    cmd.Parameters.Add("@IsInclusiveOfGST", SqlDbType.Bit).Value =
                        request.IsInclusiveOfGST.HasValue ? (object)request.IsInclusiveOfGST.Value : DBNull.Value;

                    cmd.Parameters.Add("@FilterDuration", SqlDbType.NVarChar, 50).Value =
                        (object?)request.FilterDuration ?? DBNull.Value;

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable totalTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add("Customer Invoice");

                // ================= HEADER =================
                int col = 1;
                foreach (DataRow hRow in headerTableDB.Rows)
                {
                    worksheet.Cell(1, col).Value = hRow["HeaderName"].ToString();
                    worksheet.Cell(1, col).Style.Font.Bold = true;
                    worksheet.Cell(1, col).Style.Fill.BackgroundColor = XLColor.LightBlue;
                    worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    col++;
                }

                // ================= DATA =================
                int row = 2;
                foreach (DataRow dr in dataTableDB.Rows)
                {
                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                        worksheet.Cell(row, i + 1).Value = dr[i]?.ToString() ?? "-";
                    row++;
                }

                // ================= TOTAL =================
                if (totalTableDB != null && totalTableDB.Rows.Count > 0)
                {
                    DataRow totalRow = totalTableDB.Rows[0];

                    for (int i = 0; i < dataTableDB.Columns.Count; i++)
                    {
                        string columnName = dataTableDB.Columns[i].ColumnName;

                        string? value = columnName switch
                        {
                            "Total Amount" => totalRow["TotalAmount"].ToString(),
                            "Total Outstanding" => totalRow["TotalOutstanding"].ToString() ?? "-",
                            _ => i == 0 ? "TOTAL" : ""
                        };

                        var cell = worksheet.Cell(row, i + 1);
                        cell.Value = value;
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;

                        if (columnName.ToLower().Contains("amount"))
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    }
                }

                worksheet.Columns().AdjustToContents();

                // ================= SAVE FILE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Helper.Utils.deleteOldFiles(folder, "CustomerInvoice_*.xlsx");

                string fileName = $"CustomerInvoice_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                string filePath = Path.Combine(folder, fileName);

                workbook.SaveAs(filePath);

                response.status = "Success";
                response.message = "Customer Invoice Excel generated successfully.";
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
        [Route("GetCustomerInvoiceByBookingId")]
        public IActionResult GetCustomerInvoiceByBookingId(CustomerInvoiceRequest_BookingId request)
        {
            apiResponse apiResponse = new apiResponse();
            CustomerInvoiceResponse_BookingId invoice = null;

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoiceByBookingId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@IsInclusiveGST", SqlDbType.Bit).Value = request.IsInclusiveGST;
                    cmd.Parameters.Add("@IsProformaInvoice", SqlDbType.Bit).Value = request.IsProformaInvoice;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            invoice = new CustomerInvoiceResponse_BookingId
                            {
                                ClientName = dr["Client_Name"]?.ToString(),
                                InvoiceDate = dr["InvoiceDate"]?.ToString(),
                                GSTIN = dr["Client_GSTIN"]?.ToString(),
                                Email = dr["Client_Email"]?.ToString(),
                                ClientStateId = dr["Client_StateId"] == DBNull.Value
                                                ? null
                                                : Convert.ToInt64(dr["Client_StateId"]),
                                Address = dr["Client_Address"]?.ToString(),
                                IsGSTIncluded = dr["IsGSTIncluded"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsGSTIncluded"]),
                                IsExclusiveOfGST = dr["IsExclusiveOfGST"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsExclusiveOfGST"]),
                                IsInclusiveOfGST = dr["IsInclusiveOfGST"] == DBNull.Value ? null : Convert.ToBoolean(dr["IsInclusiveOfGST"]),
                                PaidAmount = Convert.ToDecimal(dr["PaidAmount"]?.ToString())
                            };

                            string itemsJson = dr["Items"]?.ToString();

                            if (!string.IsNullOrWhiteSpace(itemsJson))
                            {
                                invoice.ItemsJson =
                                    JsonConvert.DeserializeObject<List<CustomerInvoiceItem_BookingId>>(itemsJson);
                            }
                            else
                            {
                                invoice.ItemsJson = new List<CustomerInvoiceItem_BookingId>();
                            }
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice fetched successfully.";
                apiResponse.data = invoice;

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
        [Route("GetCustomerInvoiceById")]
        public IActionResult GetCustomerInvoiceById(CustomerInvoiceRequest_Id request)
        {
            apiResponse apiResponse = new apiResponse();
            CustomerInvoiceResponse_Id invoice = null;

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoiceById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = request.BookingId;
                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.BigInt).Value = request.InvoiceNo;
                    cmd.Parameters.Add("@GSTInvoiceNo", SqlDbType.NVarChar, 20).Value = request.GSTInvoiceNo;
                    cmd.Parameters.Add("@ProformaInvoiceNo", SqlDbType.BigInt).Value = request.ProformaInvoiceNo;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            invoice = new CustomerInvoiceResponse_Id
                            {
                                Id = Convert.ToInt64(dr["Id"]),
                                InvoiceNo = Convert.ToInt64(dr["InvoiceNo"]?.ToString()),
                                ProformaInvoiceNo = Convert.ToInt64(dr["ProformaInvoiceNo"]?.ToString()),
                                GSTInvoiceNo = Convert.ToString(dr["GSTInvoiceNo"]?.ToString()),

                                IsExclusiveOfGST = Convert.ToBoolean(dr["IsExclusiveOfGST"]?.ToString()),
                                IsInclusiveOfGST = Convert.ToBoolean(dr["IsInclusiveOfGST"]?.ToString()),

                                CustomerName = dr["CustomerName"]?.ToString(),
                                InvoiceDate = dr["InvoiceDate"]?.ToString(),              // yyyy-MM-dd
                                InvoiceDate_Format = dr["InvoiceDate_Format"]?.ToString(), // dd-MMM-yyyy
                                GSTIN = dr["GSTIN"]?.ToString(),
                                Address = dr["Address"]?.ToString(),
                                EmailId = dr["EmailId"]?.ToString(),
                                CustomerStateId = Convert.ToInt64(dr["CustomerStateId"]),

                                SubTotal = Convert.ToDecimal(dr["SubTotal"]),
                                BookingDiscount = Convert.ToDecimal(dr["BookingDiscount"]),
                                TotalTaxableAmount = Convert.ToDecimal(dr["TotalTaxableAmount"]),
                                TotalDiscountAcrossAllServices = Convert.ToDecimal(dr["TotalDiscountAcrossAllServices"]),
                                TotalGST = Convert.ToDecimal(dr["TotalGST"]),
                                TotalVAT = Convert.ToDecimal(dr["TotalVAT"]),
                                GrandTotal = Convert.ToDecimal(dr["GrandTotal"]),
                                RoundOff = Convert.ToDecimal(dr["RoundOff"]),
                                TotalAdvancePaid = Convert.ToDecimal(dr["TotalAdvancePaid"]),
                                BalanceAmount = Convert.ToDecimal(dr["BalanceAmount"])
                            };

                            string itemsJson = dr["JSON_Items"]?.ToString();

                            invoice.Items = !string.IsNullOrWhiteSpace(itemsJson)
                                ? JsonConvert.DeserializeObject<List<CustomerInvoiceItem_Id>>(itemsJson)
                                : new List<CustomerInvoiceItem_Id>();
                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice fetched successfully.";
                apiResponse.data = invoice;

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
        [Route("SaveCustomerInvoice")]
        public async Task<IActionResult> SaveCustomerInvoice([FromBody] SaveCustomerInvoiceRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                long invoiceId=0;

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SaveCustomerInvoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@BookingId", request.BookingId);

                    cmd.Parameters.Add("@InvoiceNo", SqlDbType.BigInt).Value =
                        request.InvoiceNo.HasValue ? (object)request.InvoiceNo.Value : DBNull.Value;

                    cmd.Parameters.Add("@ProformaInvoiceNo", SqlDbType.BigInt).Value =
                        request.ProformaInvoiceNo.HasValue ? (object)request.ProformaInvoiceNo.Value : DBNull.Value;

                    cmd.Parameters.Add("@GSTInvoiceNo", SqlDbType.NVarChar, 50).Value =
                        string.IsNullOrWhiteSpace(request.GSTInvoiceNo)
                            ? (object)DBNull.Value
                            : request.GSTInvoiceNo.Trim();

                    cmd.Parameters.AddWithValue("@IsExclusiveOfGST", request.IsExclusiveOfGST);
                    cmd.Parameters.AddWithValue("@IsInclusiveOfGST", request.IsInclusiveOfGST);

                    cmd.Parameters.AddWithValue("@CustomerName", request.CustomerName);
                    cmd.Parameters.AddWithValue("@InvoiceDate", request.InvoiceDate);
                    cmd.Parameters.AddWithValue("@GSTIN", (object)request.GSTIN ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Address", (object)request.Address ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailId", (object)request.EmailId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CustomerStateId", (object)request.CustomerState ?? DBNull.Value);

                    // ================= PAYMENT PARAMETERS =================
                    cmd.Parameters.AddWithValue("@Payment_ModeId", (object?)request.Payment_ModeId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_AccountId", (object?)request.Payment_AccountId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_TransactionId", (object?)request.Payment_TransactionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_ChequeBank", (object?)request.Payment_ChequeBank ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_ChequeNo", (object?)request.Payment_ChequeNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_ChequeDate", (object?)request.Payment_ChequeDate ?? DBNull.Value);

                    // 🔹 Numeric (Better to define SqlDbType)
                    var amountParam = cmd.Parameters.Add("@Payment_Amount", SqlDbType.Decimal);
                    amountParam.Precision = 18;
                    amountParam.Scale = 2;
                    amountParam.Value = (object?)request.Payment_Amount ?? DBNull.Value;

                    cmd.Parameters.AddWithValue("@Payment_PaidDate", (object?)request.Payment_PaidDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_PaidTime", (object?)request.Payment_PaidTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_DueDate", (object?)request.Payment_DueDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Payment_Note", (object?)request.Payment_Note ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@SubTotal", request.SubTotal);
                    cmd.Parameters.AddWithValue("@BookingDiscount", request.BookingDiscount);
                    cmd.Parameters.AddWithValue("@TotalTaxableAmount", request.TotalTaxableAmount);
                    cmd.Parameters.AddWithValue("@TotalDiscountAcrossAllServices", request.TotalDiscountAcrossAllServices);
                    cmd.Parameters.AddWithValue("@TotalGST", request.TotalGST);
                    cmd.Parameters.AddWithValue("@TotalVAT", request.TotalVAT);
                    cmd.Parameters.AddWithValue("@GrandTotal", request.GrandTotal);
                    cmd.Parameters.AddWithValue("@RoundOff", request.RoundOff);
                    cmd.Parameters.AddWithValue("@TotalAdvancePaid", request.TotalAdvancePaid);
                    cmd.Parameters.AddWithValue("@BalanceAmount", request.BalanceAmount);

                    cmd.Parameters.AddWithValue(
                        "@JSON_Items",
                        request.JSON_Items != null
                            ? JsonConvert.SerializeObject(request.JSON_Items)
                            : DBNull.Value
                    );
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    //invoiceId = Convert.ToInt64(cmd.ExecuteScalar());

                    string whatsapp_name = "", whatsapp_bName = "", whatsapp_EDate = "", whatsapp_url = "", whatsapp_orgMobile = "", whatsapp_orgName = "",
                           whatsapp_client_contactno = "", templateName = "", templateName_Feedback = "";

                    int companyId = 0;

                    bool isSendMsg = false, isSendMsg_Feedback = false;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            //bookingId = reader["BookingId"] != DBNull.Value ? Convert.ToInt64(reader["BookingId"]) : 0;
                            invoiceId = reader["CustomerInvoiceId"] != DBNull.Value ? Convert.ToInt64(reader["CustomerInvoiceId"]) : 0;

                            whatsapp_client_contactno = reader["Client_Contact"]?.ToString() ?? "";

                            whatsapp_name = reader["Name"]?.ToString() ?? "";
                            whatsapp_bName = reader["BName"]?.ToString() ?? "";
                            whatsapp_EDate = reader["EDate"]?.ToString() ?? "";

                            whatsapp_orgMobile = reader["OrgMobile"]?.ToString() ?? "";
                            whatsapp_orgName = reader["OrgName"]?.ToString() ?? "";

                            companyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : 0;

                            templateName = reader["TempName"]?.ToString() ?? "";
                            isSendMsg = reader["IsSendMsg"] != DBNull.Value ? Convert.ToBoolean(reader["IsSendMsg"]) : false;

                            templateName_Feedback = reader["TempName_Feedback"]?.ToString() ?? "";
                            isSendMsg_Feedback = reader["IsSendMsg_Feedback"] != DBNull.Value ? Convert.ToBoolean(reader["IsSendMsg_Feedback"]) : false;
                        }
                    }

                    //NOTIFY 4 SEND WHATSAPP - NEW INVOICE
                    if (request.Id==0 && isSendMsg)
                    {
                        //Generate Invoice PDF
                        CustomerInvoiceReportRequest pdfRequest = new CustomerInvoiceReportRequest
                        {
                            BookingId = request.BookingId,
                            InvoiceNo = request.InvoiceNo,
                            GSTInvoiceNo = request.GSTInvoiceNo,
                            ProformaInvoiceNo = request.ProformaInvoiceNo,
                            ActionBy = request.ActionBy
                        };
                        string fileUrl = await _invoicePdfService.GenerateCustomerInvoicePdf(pdfRequest, connString, domain, domainUrl);
                        whatsapp_url = fileUrl;

                        List<string> variables = new List<string>
                        {
                            whatsapp_name,
                            whatsapp_bName,
                            whatsapp_url,
                            whatsapp_orgMobile,
                            whatsapp_orgName
                        };

                        await _whatsAppService.SendTemplateWhatsAppCommon(
                            companyId: companyId,
                            mobileNo: whatsapp_client_contactno,
                            templateName: templateName,
                            variables: variables,
                            actionBy: request.ActionBy,
                            isPromotional: false,
                            domainUrl: domainUrl
                        );
                    }

                    //NOTIFY 5 SEND WHATSAPP - FEEDBACK REQUEST WHEN INVOICE GENERATED
                    if (request.Id == 0 && isSendMsg_Feedback)
                    {
                        whatsapp_url = $"{domainUrl}/feedbackForm" ;

                        List<string> variables = new List<string>
                        {
                            whatsapp_name,
                            whatsapp_bName,
                            whatsapp_EDate,
                            whatsapp_url,
                            whatsapp_orgMobile,
                            whatsapp_orgName
                        };

                        await _whatsAppService.SendTemplateWhatsAppCommon(
                            companyId: companyId,
                            mobileNo: whatsapp_client_contactno,
                            templateName: templateName_Feedback,
                            variables: variables,
                            actionBy: request.ActionBy,
                            isPromotional: false,
                            domainUrl: domainUrl
                        );
                    }

                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice saved successfully.";
                apiResponse.data = new { InvoiceId = invoiceId };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        [HttpPost]
        [Route("DeleteCustomerInvoice")]
        public IActionResult DeleteCustomerInvoice(DeleteCustomerInvoice request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("DeleteCustomerInvoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", request.Id);
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                apiResponse.status = "Success";
                apiResponse.message = "Customer invoice deleted successfully.";
                apiResponse.data = null;

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return StatusCode(200, apiResponse);
            }
        }

        [HttpGet]
        [Route("GetCustomerInvoice_NextInvoiceNumbers")]
        public IActionResult GetCustomerInvoice_NextInvoiceNumbers()
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext,
                        Request,
                        out string connString,
                        out string domain,
                        out IActionResult? errorResult))
                {
                    return errorResult!;
                }

                NextInvoiceNumberResponse data = new NextInvoiceNumberResponse();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("GetCustomerInvoice_NextInvoiceNumbers", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            data.NextInvoiceNo = dr["NextInvoiceNo"] == DBNull.Value
                                ? 0L
                                : Convert.ToInt64(dr["NextInvoiceNo"]);

                            data.NextProformaInvoiceNo = dr["NextProformaInvoiceNo"] == DBNull.Value
                                ? 0L
                                : Convert.ToInt64(dr["NextProformaInvoiceNo"]);

                            data.NextGSTInvoiceNo = dr["NextGSTInvoiceNo"]?.ToString();

                        }
                    }
                }

                apiResponse.status = "Success";
                apiResponse.message = "Next invoice numbers fetched successfully.";
                apiResponse.data = data;

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

        //Send Invoice
        [HttpPost]
        [Route("SendCustomerInvoice")]
        public async Task<IActionResult> SendCustomerInvoice([FromBody] SendInvoiceRequest model)
        {
            apiResponse apiResponse = new apiResponse();
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                { return errorResult!; }

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("SendCustomerInvoice", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BookingId", model.BookingId);
                    cmd.Parameters.AddWithValue("@InvoiceNo", model.InvoiceNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GSTInvoiceNo", model.GSTInvoiceNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProformaInvoiceNo", model.ProformaInvoiceNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmailId", model.EmailId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ContactNo", model.ContactNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@WhatsappNo", model.WhatsappNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActionBy", model.ActionBy);

                    con.Open();

                    string whatsapp_name = "", whatsapp_bName = "", whatsapp_url = "", whatsapp_orgMobile = "", whatsapp_orgName = "",
                           whatsapp_client_contactno = "", templateName = "";
                    int companyId = 0;
                    bool isSendMsg = false;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            apiResponse.status = Convert.ToInt32(reader["Status"]) == 1 ? "Success" : "Error";
                            apiResponse.message = reader["Message"]?.ToString() ?? "Unknown response";

                            whatsapp_client_contactno = model.WhatsappNo ?? "";

                            whatsapp_name = reader["Name"]?.ToString() ?? "";
                            whatsapp_bName = reader["BName"]?.ToString() ?? "";
                            whatsapp_orgMobile = reader["OrgMobile"]?.ToString() ?? "";
                            whatsapp_orgName = reader["OrgName"]?.ToString() ?? "";

                            companyId = reader["CompanyId"] != DBNull.Value ? Convert.ToInt32(reader["CompanyId"]) : 0;
                            templateName = reader["TempName"]?.ToString() ?? "";
                            isSendMsg = reader["IsSendMsg"] != DBNull.Value ? Convert.ToBoolean(reader["IsSendMsg"]) : false;
                        }
                    }

                    //NOTIFY 4 SEND WHATSAPP - REQUEST SEND INVOICE
                    if (isSendMsg)
                    {
                        //Generate Invoice PDF
                        CustomerInvoiceReportRequest pdfRequest = new CustomerInvoiceReportRequest
                        {
                            BookingId = model.BookingId,
                            InvoiceNo = model.InvoiceNo,
                            GSTInvoiceNo = model.GSTInvoiceNo,
                            ProformaInvoiceNo = model.ProformaInvoiceNo,
                            ActionBy = model.ActionBy
                        };
                        string fileUrl = await _invoicePdfService.GenerateCustomerInvoicePdf(pdfRequest, connString, domain, domainUrl);
                        whatsapp_url = fileUrl;

                        List<string> variables = new List<string>
                        {
                            whatsapp_name,
                            whatsapp_bName,
                            whatsapp_url,
                            whatsapp_orgMobile,
                            whatsapp_orgName
                        };

                        await _whatsAppService.SendTemplateWhatsAppCommon(
                            companyId: companyId,
                            mobileNo: whatsapp_client_contactno,
                            templateName: templateName,
                            variables: variables,
                            actionBy: model.ActionBy,
                            isPromotional: false,
                            domainUrl: domainUrl
                        );
                    }

                    con.Close();
                }

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                return StatusCode(500, apiResponse);
            }
        }

        [HttpPost]
        [Route("GetDashboardPaymentCollectionStatus")]
        public IActionResult GetDashboardPaymentCollectionStatus([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            DashboardPaymentCollectionStatus result = null;

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid domain or connection.";
                    apiResponse.data = null;
                    return Ok(apiResponse); // ✅ ALWAYS 200
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_PaymentCollection", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    result = new DashboardPaymentCollectionStatus
                    {
                        PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                        PendingAmount = Convert.ToDecimal(row["PendingAmount"]),
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Payment collection status fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse); // ✅ ALWAYS 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse); // ✅ STILL 200
            }
        }

        [HttpPost]
        [Route("GetDashboardPaymentCollectionDetails")]
        public IActionResult GetDashboardPaymentCollectionDetails([FromBody] DashboardPaymentDetailsRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            List<DashboardPaymentCollectionDetails> details = new List<DashboardPaymentCollectionDetails>();
            DashboardPaymentCollectionTotals totals = new DashboardPaymentCollectionTotals();

            try
            {
                if (!TryGetConnectionAndDomain(
                    ControllerContext, Request,
                    out string connString, out string domain, out IActionResult? errorResult))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid domain or connection.";
                    apiResponse.data = null;
                    return Ok(apiResponse);
                }

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_PaymentCollection_Details", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("@Type", SqlDbType.NVarChar, 20).Value = (object?)request.Type ?? DBNull.Value;
                    cmd.Parameters.Add("@MonthDuration", SqlDbType.NVarChar, 50).Value = (object?)request.MonthDuration ?? DBNull.Value;
                    cmd.Parameters.Add("@FromDate", SqlDbType.NVarChar, 20).Value = (object?)request.FromDate ?? DBNull.Value;
                    cmd.Parameters.Add("@ToDate", SqlDbType.NVarChar, 20).Value = (object?)request.ToDate ?? DBNull.Value;

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(ds);
                }

                /* =========================
                   TABLE 1 → DETAILS
                ========================= */
                if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        details.Add(new DashboardPaymentCollectionDetails
                        {
                            BookingId = Convert.ToInt64(row["BookingId"]),
                            BookingRefNo = Convert.ToString(row["BookingRefNo"]),
                            CustomerName = Convert.ToString(row["Client_Name"]),
                            MobileNo = Convert.ToString(row["Client_Contact"]),
                            EventDate = Convert.ToString(row["EventDate"]),
                            EventDate_Format = Convert.ToString(row["EventDate_Format"]),

                            TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                            PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                            PendingAmount = Convert.ToDecimal(row["PendingAmount"]),

                            PaymentStatus = Convert.ToString(row["PaymentStatus"])
                        });
                    }
                }

                /* =========================
                   TABLE 2 → TOTALS
                ========================= */
                if (ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[1].Rows[0];

                    totals = new DashboardPaymentCollectionTotals
                    {
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                        PaidAmount = Convert.ToDecimal(row["PaidAmount"]),
                        PendingAmount = Convert.ToDecimal(row["PendingAmount"]),
                        TotalBookings = Convert.ToInt32(row["TotalBookings"])
                    };
                }

                /* =========================
                   FINAL RESPONSE
                ========================= */
                apiResponse.status = "Success";
                apiResponse.message = "Payment collection details fetched successfully.";
                apiResponse.data = new
                {
                    Rows = details,
                    Totals = totals
                };

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse);
            }
        }

        [HttpPost]
        [Route("GetDashboardInvoiceStatus")]
        public IActionResult GetDashboardInvoiceStatus([FromBody] DashboardFeedbackRequest request)
        {
            apiResponse apiResponse = new apiResponse();
            DashboardInvoiceStatus result = null;
            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                {
                    apiResponse.status = "Error";
                    apiResponse.message = "Invalid domain or connection.";
                    apiResponse.data = null;
                    return Ok(apiResponse); // ✅ ALWAYS 200
                }

                DataTable dt = new DataTable();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand("Dashboard_Finance_InvoiceStatus", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@MonthDuration", request.MonthDuration ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FromDate", request.FromDate ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ToDate", request.ToDate ?? (object)DBNull.Value);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];

                    result = new DashboardInvoiceStatus
                    {
                        InvoiceGeneratedCount = Convert.ToInt64(row["InvoiceGeneratedCount"]),
                        InvoicePendingCount = Convert.ToInt64(row["InvoicePendingCount"]),
                        InvoiceTotalCount = Convert.ToInt64(row["InvoiceTotalCount"])
                    };
                }

                apiResponse.status = "Success";
                apiResponse.message = "Invoice status fetched successfully.";
                apiResponse.data = result;

                return Ok(apiResponse); // ✅ ALWAYS 200
            }
            catch (Exception ex)
            {
                ErrorLogger.LogException(ex, ControllerContext, Request, null);

                apiResponse.status = "Error";
                apiResponse.message = ex.Message;
                apiResponse.data = null;

                return Ok(apiResponse); // ✅ STILL 200
            }
        }

    }
}
