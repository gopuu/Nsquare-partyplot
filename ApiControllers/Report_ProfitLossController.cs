using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Presentation;
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
    public class Report_ProfitLossController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;
        private string connString = string.Empty;

        public Report_ProfitLossController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }

        [HttpPost]
        [Route("GetProfitLossGrid")]
        public IActionResult GetProfitLossGrid([FromBody] ProfitLossGridRequest request)
        {
            apiResponse apiResponse = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(ControllerContext, Request, out string connString, out string domain, out IActionResult? errorResult))
                    return errorResult!;

                DataSet ds = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                using (SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail_GRID", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@YearId", request.YearId == 0 ? (object)DBNull.Value : request.YearId);
                    cmd.Parameters.AddWithValue("@Year", request.Year == 0 ? (object)DBNull.Value : request.Year);
                    cmd.Parameters.AddWithValue("@MonthId_From", request.MonthId_From == 0 ? (object)DBNull.Value : request.MonthId_From);
                    cmd.Parameters.AddWithValue("@MonthId_To", request.MonthId_To == 0 ? (object)DBNull.Value : request.MonthId_To);

                    con.Open();
                    new SqlDataAdapter(cmd).Fill(ds);
                }

                if (ds.Tables.Count < 3)
                    throw new Exception("P&L data incomplete.");

                DataTable dt = ds.Tables[0];
                DataTable dtSummary = ds.Tables[1];
                DataTable dtNet = ds.Tables[2];

                /* ===============================
                   SECTION SPLIT (IMPORTANT)
                   =============================== */

                var revenue = dt.AsEnumerable()
                    .Where(x => x["Type"].ToString() == "Revenue")
                    .Select(x => new ProfitLossLineItem
                    {
                        Description = x["Description"].ToString(),
                        Amount = Convert.ToDecimal(x["Amount"])
                    }).ToList();

                var cogs = dt.AsEnumerable()
                    .Where(x => x["Type"].ToString() == "COGS")
                    .Select(x => new ProfitLossLineItem
                    {
                        Description = x["Description"].ToString(),
                        Amount = Convert.ToDecimal(x["Amount"])
                    }).ToList();

                var expenses = dt.AsEnumerable()
                    .Where(x => x["Type"].ToString() == "Operating Expenses")
                    .Select(x => new ProfitLossLineItem
                    {
                        Description = x["Description"].ToString(),
                        Amount = Convert.ToDecimal(x["Amount"])
                    }).ToList();

                /* ===============================
                   SUMMARY
                   =============================== */

                decimal totalRevenue = 0, totalCOGS = 0, totalExpense = 0;

                if (dtSummary.Rows.Count > 0)
                {
                    var s = dtSummary.Rows[0];
                    totalRevenue = s["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(s["TotalRevenue"]) : 0;
                    totalCOGS = s["TotalCOGS"] != DBNull.Value ? Convert.ToDecimal(s["TotalCOGS"]) : 0;
                    totalExpense = s["TotalExpense"] != DBNull.Value ? Convert.ToDecimal(s["TotalExpense"]) : 0;
                }

                decimal netPL = 0;

                if (dtNet.Rows.Count > 0)
                {
                    netPL = dtNet.Rows[0]["NetProfitLoss"] != DBNull.Value
                        ? Convert.ToDecimal(dtNet.Rows[0]["NetProfitLoss"])
                        : 0;
                }

                /* ===============================
                   FINAL RESPONSE STRUCTURE
                   =============================== */

                var responseData = new
                {
                    Revenue = revenue,
                    TotalRevenue = totalRevenue,

                    COGS = cogs,
                    TotalCOGS = totalCOGS,

                    Expenses = expenses,
                    TotalExpense = totalExpense,

                    NetProfitLoss = netPL
                };

                apiResponse.message = "P&L report fetched successfully!";
                apiResponse.data = responseData;
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
        [Route("DownloadProfitLossReport_Pdf")]
        public IActionResult DownloadProfitLossReport_Pdf([FromBody] ProfitLossReportRequest request)
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

                    /* ── Company Header ── */

                    SqlCommand cmd = new SqlCommand("Report_PageHeader_GetCompany", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ActionBy", request.ActionBy);
                    new SqlDataAdapter(cmd).Fill(dtCompany);


                    /* ── P&L REPORT ── */

                    cmd = new SqlCommand("Report_GetProfitLossDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@YearId",
                        request.YearId == 0 ? (object)DBNull.Value : request.YearId);

                    cmd.Parameters.AddWithValue("@Year",
                        request.Year == 0 ? (object)DBNull.Value : request.Year);

                    cmd.Parameters.AddWithValue("@MonthId_From",
                        request.MonthId_From == 0 ? (object)DBNull.Value : request.MonthId_From);

                    cmd.Parameters.AddWithValue("@MonthId_To",
                        request.MonthId_To == 0 ? (object)DBNull.Value : request.MonthId_To);

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
                string reportName = dsReport.Tables[0].Rows[0]["ReportTitle"]?.ToString() ?? "P&L STATEMENT";

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

                //var dtHeader = dsReport.Tables[0];
                var dt = dsReport.Tables[1];
                var dtSummary = dsReport.Tables[2];
                var dtNet = dsReport.Tables[3];

                /* ── Report Title ── */

                document.Add(new Paragraph(reportName.ToUpper(), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.GRAY))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10f
                });

                var revenue = dt.AsEnumerable().Where(x => x["Type"].ToString() == "Revenue").ToList();
                var cogs = dt.AsEnumerable().Where(x => x["Type"].ToString() == "COGS").ToList();
                var expenses = dt.AsEnumerable().Where(x => x["Type"].ToString() == "Operating Expenses").ToList();

                decimal totalRevenue = dtSummary.Rows.Count > 0 ? Convert.ToDecimal(dtSummary.Rows[0]["TotalRevenue"]) : 0;
                decimal netPL = dtNet.Rows.Count > 0 ? Convert.ToDecimal(dtNet.Rows[0]["NetProfitLoss"]) : 0;


                /* =========================
                   COMMON STYLES
                   ========================= */

                iTextSharp.text.Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                iTextSharp.text.Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                iTextSharp.text.Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                string fontPath = Path.Combine(_env.WebRootPath, "fonts", "arial.ttf");
                BaseFont bf = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                iTextSharp.text.Font AmtFontNormal = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);
                iTextSharp.text.Font AmtFontBold = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD);

                BaseColor headerBg = new BaseColor(220, 220, 220);
                BaseColor totalBg = new BaseColor(212, 237, 218);


                /* ========================= (1) REVENUE ========================= */
                PdfPTable revenueTable = new PdfPTable(3);
                revenueTable.WidthPercentage = 70;
                revenueTable.SetWidths(new float[] { 10, 60, 30 });

                PdfPCell revHeader = new PdfPCell(new Phrase("(1) Revenue", headerFont))
                {
                    Colspan = 3,
                    BackgroundColor = headerBg,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };
                revenueTable.AddCell(revHeader);

                int i = 1;
                foreach (var r in revenue)
                {
                    // Sr No (Center)
                    revenueTable.AddCell(new PdfPCell(new Phrase(i++.ToString(), normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5
                    });

                    // Description (Left - default but explicit is better)
                    revenueTable.AddCell(new PdfPCell(new Phrase(r["Description"].ToString(), normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5
                    });

                    // Amount (Right)
                    revenueTable.AddCell(new PdfPCell(new Phrase("₹ " + Convert.ToDecimal(r["Amount"]).ToString("N2"), AmtFontNormal))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5
                    });
                }

                /* Total Revenue */
                PdfPCell totalLbl = new PdfPCell(new Phrase("Total Revenue", AmtFontNormal))
                {
                    Colspan = 2,
                    BackgroundColor = totalBg,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };
                PdfPCell totalVal = new PdfPCell(new Phrase("₹ " + totalRevenue.ToString("N2"), AmtFontNormal))
                {
                    BackgroundColor = totalBg,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 6
                };

                revenueTable.AddCell(totalLbl);
                revenueTable.AddCell(totalVal);

                document.Add(revenueTable);

                /* =========================
                   (2) COGS
                   ========================= */
                PdfPTable cogsTable = new PdfPTable(3);
                cogsTable.WidthPercentage = 70;
                cogsTable.SetWidths(new float[] { 10, 60, 30 });

                cogsTable.AddCell(new PdfPCell(new Phrase("(2) COGS", headerFont))
                {
                    Colspan = 3,
                    BackgroundColor = headerBg,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                });

                i = 1;
                foreach (var c in cogs)
                {
                    // Sr No (Center)
                    cogsTable.AddCell(new PdfPCell(new Phrase(i++.ToString(), normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5
                    });

                    // Description (Left)
                    cogsTable.AddCell(new PdfPCell(new Phrase(c["Description"].ToString(), normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5
                    });

                    // Amount (Right)
                    cogsTable.AddCell(new PdfPCell(new Phrase("₹ " + Convert.ToDecimal(c["Amount"]).ToString("N2"), AmtFontNormal))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5,
                        PaddingRight = 8
                    });
                }
                document.Add(cogsTable);

                /* =========================
                   (3) EXPENSES
                   ========================= */
                PdfPTable expTable = new PdfPTable(3);
                expTable.WidthPercentage = 70;
                expTable.SetWidths(new float[] { 10, 60, 30 });

                expTable.AddCell(new PdfPCell(new Phrase("(3) Operating Expenses", headerFont))
                {
                    Colspan = 3,
                    BackgroundColor = headerBg,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                });
                /* header row */
                // Sr. No. (Center)
                expTable.AddCell(new PdfPCell(new Phrase("Sr. No.", boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 6
                });

                // Category (Left)
                expTable.AddCell(new PdfPCell(new Phrase("Category", boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 6
                });

                // Amount (Right)
                expTable.AddCell(new PdfPCell(new Phrase("Amount", boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 6,
                    PaddingRight = 8
                });

                i = 1;
                foreach (var e in expenses)
                {
                    // Sr No (Center)
                    expTable.AddCell(new PdfPCell(new Phrase(i++.ToString(), normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5
                    });

                    // Description (Left)
                    expTable.AddCell(new PdfPCell(new Phrase(e["Description"].ToString(), normalFont))
                    {
                        HorizontalAlignment = Element.ALIGN_LEFT,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5
                    });

                    // Amount (Right)
                    expTable.AddCell(new PdfPCell(new Phrase("₹ " + Convert.ToDecimal(e["Amount"]).ToString("N2"), AmtFontNormal))
                    {
                        HorizontalAlignment = Element.ALIGN_RIGHT,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 5,
                        PaddingRight = 8
                    });
                }

                /* =========================
                   NET P&L (INSIDE SAME TABLE)
                   ========================= */
                bool isProfit = netPL > 0;
                bool isLoss = netPL < 0;

                BaseColor netBg = BaseColor.LIGHT_GRAY;
                if (isProfit) netBg = new BaseColor(183, 215, 168);
                else if (isLoss) netBg = new BaseColor(244, 199, 195);

                PdfPCell netLbl = new PdfPCell(new Phrase("Net P&L", AmtFontNormal))
                {
                    Colspan = 2,
                    BackgroundColor = netBg,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 6
                };

                PdfPCell netVal = new PdfPCell(new Phrase("₹ " + Math.Abs(netPL).ToString("N2"), AmtFontNormal))
                {
                    BackgroundColor = netBg,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    Padding = 6
                };

                expTable.AddCell(netLbl);
                expTable.AddCell(netVal);

                document.Add(expTable);

                document.Close();

                /* ── Save file ── */
                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "ProfitLoss_Report_*.pdf");

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
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
        [Route("DownloadProfitLossReport_Excel")]
        public IActionResult DownloadProfitLossReport_Excel([FromBody] ProfitLossReportRequest request)
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

                    SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    /* ── P&L REPORT ── */

                    cmd = new SqlCommand("Report_GetProfitLossDetail_PdfExcel", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@YearId",
                        request.YearId == 0 ? (object)DBNull.Value : request.YearId);

                    cmd.Parameters.AddWithValue("@Year",
                        request.Year == 0 ? (object)DBNull.Value : request.Year);

                    cmd.Parameters.AddWithValue("@MonthId_From",
                        request.MonthId_From == 0 ? (object)DBNull.Value : request.MonthId_From);

                    cmd.Parameters.AddWithValue("@MonthId_To",
                        request.MonthId_To == 0 ? (object)DBNull.Value : request.MonthId_To);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                if (dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];
                DataTable summaryTableDB = dsReport.Tables.Count > 3 ? dsReport.Tables[3] : null;

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("P&L Report");

                int row = 1;

                /* ========================= TITLE ========================= */
                ws.Cell(row, 1).Value = dsReport.Tables[0].Rows[0]["ReportTitle"]?.ToString() ?? "P&L STATEMENT";
                ws.Range(row, 1, row, 3).Merge();
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Font.FontSize = 16;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row += 2;

                /* ========================= EXTRACT DATA ========================= */
                var dt = dsReport.Tables[0];
                var dtSummary = dsReport.Tables[1];
                var dtNet = dsReport.Tables[2];

                var revenue = dt.AsEnumerable().Where(x => x["Type"].ToString() == "Revenue").ToList();
                var cogs = dt.AsEnumerable().Where(x => x["Type"].ToString() == "COGS").ToList();
                var expenses = dt.AsEnumerable().Where(x => x["Type"].ToString() == "Operating Expenses").ToList();

                decimal totalRevenue = Convert.ToDecimal(dtSummary.Rows[0]["TotalRevenue"]);
                decimal netPL = Convert.ToDecimal(dtNet.Rows[0]["NetProfitLoss"]);


                /* =========================STYLES========================= */
                var headerBg = XLColor.FromArgb(220, 220, 220);
                var totalBg = XLColor.FromArgb(212, 237, 218);
                var profitBg = XLColor.FromArgb(183, 215, 168);
                var lossBg = XLColor.FromArgb(244, 199, 195);
                var neutralBg = XLColor.FromArgb(230, 230, 230);


                /* =========================
                   (1) REVENUE
                   ========================= */
                ws.Cell(row, 1).Value = "(1) Revenue";
                ws.Range(row, 1, row, 3).Merge().Style.Fill.BackgroundColor = headerBg;
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row++;

                int i = 1;
                foreach (var r in revenue)
                {
                    ws.Cell(row, 1).Value = i++;
                    ws.Cell(row, 2).Value = r["Description"].ToString();
                    ws.Cell(row, 3).Value = Convert.ToDecimal(r["Amount"]);
                    ws.Cell(row, 3).Style.NumberFormat.Format = "₹ #,##0.00";
                    ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    row++;
                }

                /* Total Revenue */
                ws.Range(row, 1, row, 2).Merge().Value = "Total Revenue";
                ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = totalBg;
                ws.Cell(row, 3).Value = totalRevenue;
                ws.Cell(row, 3).Style.Fill.BackgroundColor = totalBg;
                ws.Cell(row, 3).Style.NumberFormat.Format = "₹ #,##0.00";
                ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Row(row).Style.Font.Bold = true;
                row += 2;


                /* =========================
                   (2) COGS
                   ========================= */

                ws.Cell(row, 1).Value = "(2) COGS";
                ws.Range(row, 1, row, 3).Merge().Style.Fill.BackgroundColor = headerBg;
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row++;

                i = 1;
                foreach (var c in cogs)
                {
                    ws.Cell(row, 1).Value = i++;
                    ws.Cell(row, 2).Value = c["Description"].ToString();
                    ws.Cell(row, 3).Value = Convert.ToDecimal(c["Amount"]);
                    ws.Cell(row, 3).Style.NumberFormat.Format = "₹ #,##0.00";
                    ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    row++;
                }

                row += 1;


                /* =========================
                   (3) EXPENSES
                   ========================= */

                ws.Cell(row, 1).Value = "(3) Operating Expenses";
                ws.Range(row, 1, row, 3).Merge().Style.Fill.BackgroundColor = headerBg;
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                row++;

                /* header */
                ws.Cell(row, 1).Value = "Sr. No.";
                ws.Cell(row, 2).Value = "Category Name";
                ws.Cell(row, 3).Value = "Amount";
                ws.Range(row, 1, row, 3).Style.Font.Bold = true;
                ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
                row++;

                i = 1;
                foreach (var e in expenses)
                {
                    ws.Cell(row, 1).Value = i++;
                    ws.Cell(row, 2).Value = e["Description"].ToString();
                    ws.Cell(row, 3).Value = Convert.ToDecimal(e["Amount"]);
                    ws.Cell(row, 3).Style.NumberFormat.Format = "₹ #,##0.00";
                    ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    row++;
                }


                /* =========================
                   NET P&L (INSIDE SAME TABLE)
                   ========================= */

                var netBg = neutralBg;
                if (netPL > 0) netBg = profitBg;
                else if (netPL < 0) netBg = lossBg;

                ws.Range(row, 1, row, 2).Merge().Value = "Net P&L";
                ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = netBg;
                ws.Cell(row, 3).Value = Math.Abs(netPL);
                ws.Cell(row, 3).Style.Fill.BackgroundColor = netBg;
                ws.Cell(row, 3).Style.NumberFormat.Format = "₹ #,##0.00";
                ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Row(row).Style.Font.Bold = true;


                /* =========================
                   FINAL FORMATTING
                   ========================= */

                ws.Columns(1, 3).AdjustToContents();
                ws.Columns(1, 1).Width = 10;
                ws.Columns(2, 2).Width = 40;
                ws.Columns(3, 3).Width = 18;

                ws.Columns().AdjustToContents();

                string folder = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                Utils.deleteOldFiles(folder, "ProfitLoss_Report_*.xlsx");

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                workbook.SaveAs(Path.Combine(folder, fileName));

                response.status = "Success";
                response.message = "P&L Report Excel generated successfully.";
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
