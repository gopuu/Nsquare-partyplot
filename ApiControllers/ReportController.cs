using ERPAdmin.Helper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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


        [HttpGet]
        [Route("DownloadBookingReport")]
        public IActionResult DownloadBookingReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                 /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(70f, 70f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,   // 👈 CELL CENTER
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("BOOKING REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);
                
                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++; 
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Booking_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("DownloadRevenueReport")]
        public IActionResult DownloadRevenueReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(70f, 70f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,   
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("REVENUE REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);


                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Revenue_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,   
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("GST REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"GST_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("REVENUELOSS REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"RevenueLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
                return Ok(response);
            }
        }

        [HttpGet]
        [Route("DownloadPurchaseReport")]
        public IActionResult DownloadPurchaseReport()
        {
            apiResponse response = new apiResponse();

            try
            {
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("Purchase REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Purchase_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("FOODWASTAGE REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"FoodWastage_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("EXPENSE REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Expense_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("PROFITLOSS REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"ProfitLoss_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("CASHFLOW REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Cashflow_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                using var stream = new MemoryStream();

                Document document = new Document(PageSize.A4, 30, 30, 40, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;
                writer.PageEvent = new PdfPageEvents("Admin");

                document.Open();

                /* ================= HEADER ================= */

                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 20f, 80f });
                headerTable.SpacingAfter = 15f;   // 👈 TABLE SE GAP

                BaseColor headerBg = new BaseColor(245, 247, 250);
                BaseColor borderColor = new BaseColor(200, 200, 200);

                // LOGO
                string logoPath = Path.Combine(
                    _env.WebRootPath,
                    "assets2",
                    "img",
                    "banquatlogo1.png"
                );

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(80f, 80f);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // COMPANY DETAILS
                Font companyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                Font addressFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);
                Font reportFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);

                PdfPCell detailCell = new PdfPCell
                {
                    Border = Rectangle.BOTTOM_BORDER,
                    BorderColor = borderColor,
                    BackgroundColor = headerBg,
                    Padding = 10,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };

                // Paragraphs with CENTER alignment
                Paragraph pCompany = new Paragraph("Nsquare Software Solution", companyFont);
                pCompany.Alignment = Element.ALIGN_CENTER;

                Paragraph pAddress = new Paragraph("Surat, Gujarat", addressFont);
                pAddress.Alignment = Element.ALIGN_CENTER;

                Paragraph pReport = new Paragraph("MODIFICATION REPORT", reportFont);
                pReport.Alignment = Element.ALIGN_CENTER;

                detailCell.AddElement(pCompany);
                detailCell.AddElement(pAddress);
                detailCell.AddElement(pReport);

                headerTable.AddCell(logoCell);
                headerTable.AddCell(detailCell);
                document.Add(headerTable);
                //document.Add(new Paragraph("\n\n"));

                /* ================= TABLE ================= */
                PdfPTable table = new PdfPTable(8);
                table.WidthPercentage = 100;
                table.SetWidths(new float[] { 2f, 2f, 2f, 2f, 2f, 2f, 2f, 2f });
                table.SpacingBefore = 5f;

                Font thFont = new Font(Font.FontFamily.HELVETICA, 10, Font.BOLD, BaseColor.WHITE);
                Font tdFont = new Font(Font.FontFamily.HELVETICA, 9);
                BaseColor thColor = new BaseColor(0, 121, 182);

                string[] headers = { "Sr. No", "A1", "A2", "A3", "A4", "A5", "A6", "A7" };
                foreach (var h in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(h, thFont))
                    {
                        BackgroundColor = thColor,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        Padding = 6
                    };
                    table.AddCell(cell);
                }

                // STATIC DATA
                int srNo = 1;

                for (int i = 1; i <= 10; i++)
                {
                    table.AddCell(new PdfPCell(new Phrase(srNo.ToString(), tdFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER
                    });

                    table.AddCell(new PdfPCell(new Phrase("Data 1", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 2", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 3", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 4", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 5", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 6", tdFont)));
                    table.AddCell(new PdfPCell(new Phrase("Data 7", tdFont)));

                    srNo++;
                }


                document.Add(table);
                document.Close();

                /* ================= SAVE FILE ================= */

                string folderPath = Path.Combine(_env.WebRootPath, "reports");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"Modification_Report_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(folderPath, fileName);

                System.IO.File.WriteAllBytes(filePath, stream.ToArray());

                string fileUrl = $"{Request.Scheme}://{Request.Host}/reports/{fileName}";

                response.status = "Success";
                response.message = "Report Generated";
                response.data = new { FileUrl = fileUrl };

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.status = "Fail";
                response.message = ex.Message;
                response.data = new { FileUrl = "" };
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
                PdfPTable footerTable = new PdfPTable(3);
                footerTable.TotalWidth = document.PageSize.Width - 60;
                footerTable.SetWidths(new float[] { 33f, 34f, 33f });
                footerTable.LockedWidth = true;

                // 🔹 LEFT – Username
                PdfPCell leftCell = new PdfPCell(new Phrase($"UserName: {_username}", footerFont))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderColor = lineColor,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    PaddingTop = 5,
                    PaddingBottom = 5
                };

                // 🔹 CENTER – Page No
                PdfPCell centerCell = new PdfPCell(new Phrase($"Page | {writer.PageNumber}", footerFont))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderColor = lineColor,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    PaddingTop = 5,
                    PaddingBottom = 5
                };

                // 🔹 RIGHT – DateTime
                PdfPCell rightCell = new PdfPCell(
                    new Phrase(DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt"), footerFont))
                {
                    Border = Rectangle.TOP_BORDER,
                    BorderColor = lineColor,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    PaddingTop = 5,
                    PaddingBottom = 5
                };

                footerTable.AddCell(leftCell);
                footerTable.AddCell(centerCell);
                footerTable.AddCell(rightCell);

                // Draw footer
                footerTable.WriteSelectedRows(
                    0, -1,
                    document.LeftMargin,
                    document.BottomMargin + 20,
                    writer.DirectContent
                );
            }
        }

}
}
