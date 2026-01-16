using ERPAdmin.Helper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace ERPAdmin.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : MasterController
    {
        private readonly string domainUrl;
        private readonly IWebHostEnvironment _env;

        private string connString = string.Empty;

        public ReportController(IConfiguration configuration, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor) : base(configuration, httpContextAccessor) // ✅ Call base controller constructor properly
        {
            _env = env;
            var request = httpContextAccessor.HttpContext?.Request;
            domainUrl = $"{request?.Scheme}://{request?.Host.Value}/";
        }


        //[HttpGet]
        //[Route("DownloadBookingReport")]
        //public IActionResult DownloadBookingReport()
        //{
        //    apiResponse response = new apiResponse();

        //    try
        //    {
        //        using var stream = new MemoryStream();

        //        Document document = new Document(PageSize.A4, 30, 30, 40, 70);

        //        PdfWriter writer = PdfWriter.GetInstance(document, stream);
        //        writer.CloseStream = false;

        //        writer.PageEvent = new PdfPageEvents("Admin");
        //        document.Open();

        //         /* ================= HEADER ================= */
        //        PdfPTable headerTable = new PdfPTable(2);
        //        headerTable.WidthPercentage = 100;
        //        headerTable.SetWidths(new float[] { 20f, 80f });
        //        headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

        //        BaseColor headerBg = new BaseColor(245, 247, 250);
        //        BaseColor borderColor = new BaseColor(200, 200, 200);

        //        // LOGO
        //        string logoPath = Path.Combine(
        //            _env.WebRootPath,
        //            "assets2",
        //            "img",
        //            "banquatlogo1.png"
        //        );

        //        Image logo = Image.GetInstance(logoPath);
        //        logo.ScaleToFit(70f, 70f);

        //        PdfPCell logoCell = new PdfPCell(logo)
        //        {
        //            Border = Rectangle.BOTTOM_BORDER,
        //            BorderColor = borderColor,
        //            BackgroundColor = headerBg,
        //            Padding = 10,
        //            HorizontalAlignment = Element.ALIGN_LEFT,
        //            VerticalAlignment = Element.ALIGN_MIDDLE
        //        };

        //        // COMPANY DETAILS
        //        Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        //        Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
        //        Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

        //        PdfPCell detailCell = new PdfPCell
        //        {
        //            Border = Rectangle.BOTTOM_BORDER,
        //            BorderColor = borderColor,
        //            BackgroundColor = headerBg,
        //            Padding = 10,
        //            HorizontalAlignment = Element.ALIGN_CENTER,   // 👈 CELL CENTER
        //            VerticalAlignment = Element.ALIGN_MIDDLE
        //        };

        //        // Paragraphs with CENTER alignment
        //        Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
        //        pCompany.Alignment = Element.ALIGN_CENTER;

        //        Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
        //        pAddress.Alignment = Element.ALIGN_CENTER;

        //        Paragraph pReport = new Paragraph("BOOKING REPORT", reportFont);
        //        pReport.Alignment = Element.ALIGN_CENTER;

        //        detailCell.AddElement(pCompany);
        //        detailCell.AddElement(pAddress);
        //        detailCell.AddElement(pReport);

        //        headerTable.AddCell(logoCell);
        //        headerTable.AddCell(detailCell);
        //        document.Add(headerTable);
        //        //document.Add(new Paragraph("\n\n"));

        //        /* ================= TABLE ================= */
        //        PdfPTable table = new PdfPTable(8);
        //        table.WidthPercentage = 100;
        //        table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
        //        table.SpacingBefore = 5f;

        //        Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
        //        Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
        //        BaseColor thColor = new BaseColor(0, 121, 182);

        //        string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
        //        foreach (var h in headers)
        //        {
        //            PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
        //            {
        //                BackgroundColor = thColor,
        //                HorizontalAlignment = Element.ALIGN_CENTER,
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                Padding = 6
        //            };
        //            table.AddCell(cell);
        //        }

        //        table.HeaderRows = 1;        
        //        table.SplitLate = false;   
        //        table.SplitRows = true;     
        //        //table.SpacingAfter = 30f;   

        //        // STATIC DATA
        //        int srNo = 1;

        //        for (int i = 1; i <= 100; i++)
        //        {
        //            table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
        //            {
        //                HorizontalAlignment = Element.ALIGN_CENTER
        //            });

        //            table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
        //            table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
        //            table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
        //            table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
        //            table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
        //            table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
        //            table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

        //            srNo++; 
        //        }
        //        document.Add(table);
        //        document.Close();

        //        /* ================= SAVE FILE ================= */

        //        string folderPath = Path.Combine(_env.WebRootPath, "reports");
        //        if (!Directory.Exists(folderPath))
        //            Directory.CreateDirectory(folderPath);

        //        string fileName = $"Booking_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        //        string filePath = Path.Combine(folderPath, fileName);

        //        System.IO.File.WriteAllBytes(filePath, stream.ToArray());

        //        string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

        //        response.status = "Success";
        //        response.message = "Report Generated";
        //        response.data = new { FileUrl = fileUrl };

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        response.status = "Fail";
        //        response.message = ex.Message;
        //        response.data = new { FileUrl = "" };
        //        return Ok(response);
        //    }
        //}

        [HttpPost]
        [Route("DownloadBookingReport")]
        public IActionResult DownloadBookingReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetBookingDetails", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EventTypeId", 0);
                    cmd.Parameters.AddWithValue("@EventSubTypeId", 0);
                    cmd.Parameters.AddWithValue("@HallId", 0);
                    cmd.Parameters.AddWithValue("@EventDate", DBNull.Value);

                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
               // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Booking_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Booking_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

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
        [Route("DownloadDailySalesReport")]
        public IActionResult DownloadDailySalesReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetDailySales", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"DailySales_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "DailySales_Report_*.pdf");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    stream.WriteTo(fileStream);
                }

                string fileUrl = $"/reports/{fileName}";

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

        [HttpGet]
        [Route("DownloadMonthlySalesReport")]
        public IActionResult DownloadMonthlySalesReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetMonthlySales", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"MonthlySales_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }


        [HttpGet]
        [Route("DownloadRevenueLossReport")]
        public IActionResult DownloadRevenueLossReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetRevenueLoss", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"RevenueLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }


        [HttpGet]
        [Route("DownloadGSTReport")]
        public IActionResult DownloadGSTReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetGSTDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"GST_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }


        private PdfPCell CreateHeaderCell(string text, Font font, BaseColor bgColor)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = bgColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 6
            };
        }

        private PdfPCell CreateBodyCell(string text, Font font, int align = Element.ALIGN_LEFT)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = align,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5
            };
        }



        [HttpGet]
        [Route("DownloadPurchaseReport")]
        public IActionResult DownloadPurchaseReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetPurchaseDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("DownloadFoodWastageReport")]
        public IActionResult DownloadFoodWastageReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetFoodWastageDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"FoodWastage_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("DownloadExpenseReport")]
        public IActionResult DownloadExpenseReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetExpenseDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Expense_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("DownloadProfitLossReport")]
        public IActionResult DownloadProfitLossReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetProfitLossDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }


        [HttpGet]
        [Route("DownloadCashflowReport")]
        public IActionResult DownloadCashflowReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetCashflowDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Cashflow_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("DownloadModificationReport")]
        public IActionResult DownloadModificationReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                if (!TryGetConnectionAndDomain(
                        ControllerContext, Request,
                        out string connString,
                        out string domain,
                        out IActionResult errorResult))
                {
                    return errorResult;
                }

                using var stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 30, 30, 40, 70);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // 🔹 Company
                    new SqlDataAdapter(
                        new SqlCommand("Report_PageHeader_GetCompany", con)
                        { CommandType = CommandType.StoredProcedure }
                    ).Fill(dtCompany);

                    // 🔹 Report
                    using SqlCommand cmd = new SqlCommand("Report_GetModificationDetail", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    new SqlDataAdapter(cmd).Fill(dsReport);
                }

                // ================= VALIDATION =================
                if (dtCompany.Rows.Count == 0 || dsReport.Tables.Count < 3)
                    throw new Exception("Report data not found");

                // ================= EXTRACT DATA =================
                string companyName = dtCompany.Rows[0]["CompanyName"].ToString();
                string companyAddress = dtCompany.Rows[0]["CompanyAddress"].ToString();
                string companyphoneNo = dtCompany.Rows[0]["CompanyMobile"].ToString();
                string logoFile = dtCompany.Rows[0]["CompanyLogo"].ToString();

                string reportName = dsReport.Tables[0].Rows[0]["ReportName"].ToString();
                DataTable headerTableDB = dsReport.Tables[1];
                DataTable dataTableDB = dsReport.Tables[2];

                // ================= HEADER =================
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });

                Image logo = Image.GetInstance(
                    Path.Combine(_env.WebRootPath, "assets2", "img", logoFile)
                );
                logo.ScaleToFit(70f, 70f);

                headerTable.AddCell(new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10
                });

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };

                detailCell.AddElement(new Paragraph(companyName,
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                detailCell.AddElement(new Paragraph(companyAddress,
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                detailCell.AddElement(new Paragraph(companyphoneNo,
                   FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));

                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                // document.Add(new Paragraph("\n"));

                // ================= REPORT NAME (CENTER) =================
                Font reportTitleFont = FontFactory.GetFont(
                    FontFactory.HELVETICA_BOLD,
                    14,
                    BaseColor.BLACK
                );

                Paragraph reportTitle = new Paragraph(reportName.ToUpper(), reportTitleFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    //SpacingBefore = 10f,
                    SpacingAfter = 18f
                };

                document.Add(reportTitle);


                // ================= TABLE =================
                int columnCount = dataTableDB.Columns.Count + 1;
                PdfPTable table = new PdfPTable(columnCount);
                table.WidthPercentage = 100;

                float[] widths = Enumerable.Repeat(1f, columnCount).ToArray();
                table.SetWidths(widths);

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                table.AddCell(CreateHeaderCell("Sr. No", thFont, thColor));

                for (int i = 0; i < headerTableDB.Columns.Count; i++)
                    table.AddCell(CreateHeaderCell(headerTableDB.Rows[0][i].ToString(), thFont, thColor));

                table.HeaderRows = 1;

                int srNo = 1;
                foreach (DataRow row in dataTableDB.Rows)
                {
                    table.AddCell(CreateBodyCell(srNo++.ToString(), tdFont, Element.ALIGN_CENTER));

                    foreach (DataColumn col in dataTableDB.Columns)
                        table.AddCell(CreateBodyCell(row[col]?.ToString() ?? "-", tdFont));
                }

                document.Add(table);
                document.Close();

                // ================= SAVE =================
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"Modification_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}" };
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                return Ok(response);
            }
        }


       
        public class PdfPageEvents : PdfPageEventHelper
        {
            private readonly string _username;

            public PdfPageEvents(string username)
            {
                _username = username;
            }

            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
            BaseColor lineColor = new BaseColor(180, 180, 180);

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfContentByte cb = writer.DirectContent;

                float lineY = document.Bottom - 5;   // 👈 LINE position
                float textY = document.Bottom - 20;  // 👈 FOOTER TEXT position

                // ================= LINE (BORDER) =================
                cb.SetLineWidth(0.5f);
                cb.SetColorStroke(BaseColor.GRAY);
                cb.MoveTo(document.Left, lineY);
                cb.LineTo(document.Right, lineY);
                cb.Stroke();

                // ================= FOOTER TEXT =================
                cb.BeginText();

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
                cb.SetFontAndSize(bf, 8);
                cb.SetColorFill(BaseColor.DARK_GRAY);

                cb.ShowTextAligned(
                    Element.ALIGN_LEFT,
                    "UserName : Admin",
                    document.Left,
                    textY,
                    0
                );

                cb.ShowTextAligned(
                    Element.ALIGN_CENTER,
                    "Page " + writer.PageNumber,
                    (document.Left + document.Right) / 2,
                    textY,
                    0
                );

                cb.ShowTextAligned(
                    Element.ALIGN_RIGHT,
                    DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"),
                    document.Right,
                    textY,
                    0
                );

                cb.EndText();
            }


        }

    }
}
