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
        [Route("DownloadBookingEventReport")]
        public IActionResult DownloadBookingEventReport(Int64 bookingId)
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

                using MemoryStream stream = new MemoryStream();
                Document document = new Document(PageSize.A4, 25, 25, 30, 40);
                PdfWriter writer = PdfWriter.GetInstance(document, stream);
                writer.CloseStream = false;

                document.Open();

                DataTable dtCompany = new DataTable();
                DataSet dsReport = new DataSet();

                using (SqlConnection con = new SqlConnection(connString))
                {
                    con.Open();

                    // ================= COMPANY HEADER =================
                    using (SqlCommand cmdCompany = new SqlCommand("Report_PageHeader_GetCompany", con))
                    {
                        cmdCompany.CommandType = CommandType.StoredProcedure;

                        SqlDataAdapter daCompany = new SqlDataAdapter(cmdCompany);
                        daCompany.Fill(dtCompany);
                    }

                    // ================= BOOKING EVENT DETAILS =================
                    using (SqlCommand cmdReport = new SqlCommand("Report_GetBookingEventDetails", con))
                    {
                        cmdReport.CommandType = CommandType.StoredProcedure;

                        // 🔹 BookingId Parameter
                        cmdReport.Parameters.Add("@BookingId", SqlDbType.BigInt).Value = bookingId;

                        SqlDataAdapter daReport = new SqlDataAdapter(cmdReport);
                        daReport.Fill(dsReport);
                    }
                }


                /* ================= FONT ================= */
                Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                Font boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9);
                Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                BaseColor gray = new BaseColor(230, 230, 230);

                /* ================= DATASET MAPPING ================= */
                DataTable dtFormTitle = dsReport.Tables[0];
                DataTable dtBasic = dsReport.Tables[1];
                DataTable dtHeaders = dsReport.Tables[2];
                DataTable dtPlateRows = dsReport.Tables[3];
                DataTable dtSummary = dsReport.Tables[4];
                DataTable dtPayment = dsReport.Tables[5];
                DataTable dtFooter = dsReport.Tables[6];
                DataTable dtStaffTitle = dsReport.Tables[7];
                DataTable dtFoodTitles = dsReport.Tables[8];
                DataTable dtStaffDuty = dsReport.Tables[9];
                DataTable dtChkTitle = dsReport.Tables[10];
                DataTable dtChkItems = dsReport.Tables[11];
                DataTable dtNotes = dsReport.Tables[12];


                /* ================= HEADER ================= */
                PdfPTable header = new PdfPTable(2);
                header.WidthPercentage = 100;
                header.SetWidths(new float[] { 20, 80 });

                string logoPath = Path.Combine(_env.WebRootPath, "assets2", "img",
                    dtCompany.Rows[0]["CompanyLogo"].ToString());

                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(60, 60);

                header.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER });

                PdfPCell compCell = new PdfPCell
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER
                };
                compCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyName"].ToString(),
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));
                compCell.AddElement(new Paragraph(dtCompany.Rows[0]["CompanyAddress"].ToString(),
                    FontFactory.GetFont(FontFactory.HELVETICA, 9)));

                header.AddCell(compCell);
                document.Add(header);

                document.Add(new Paragraph("\n"));

                /* ================= EVENT FORM TITLE ================= */
                PdfPTable titleTable = new PdfPTable(2);
                titleTable.WidthPercentage = 100;
                titleTable.SetWidths(new float[] { 70, 30 });

                PdfPCell titleCell = CreateCell(
                    dtFormTitle.Rows[0]["FormTitle"].ToString(),
                    titleFont,
                    Element.ALIGN_CENTER);
                titleCell.BackgroundColor = new BaseColor(230, 230, 230); // light gray
                titleTable.AddCell(titleCell);

                PdfPCell dateCell = CreateCell(
                    dtFormTitle.Rows[0]["DateTitle"].ToString() + " : ",
                    titleFont);
                dateCell.BackgroundColor = new BaseColor(230, 230, 230);
                titleTable.AddCell(dateCell);

                document.Add(titleTable);


                /* ================= GUEST DETAILS ================= */
                PdfPTable guestTable = new PdfPTable(2);
                guestTable.WidthPercentage = 100;
                guestTable.SetWidths(new float[] { 30, 70 });

                guestTable.AddCell(CreateCell(dtBasic.Rows[0]["GuestNameTitle"].ToString(), boldFont));
                guestTable.AddCell(CreateCell("", normalFont));

                guestTable.AddCell(CreateCell(dtBasic.Rows[0]["EventDetailsTitle"].ToString(), boldFont));
                guestTable.AddCell(CreateCell("", normalFont));

                document.Add(guestTable);

                /* ================= PLATE TABLE ================= */
                PdfPTable plateTable = new PdfPTable(4);
                plateTable.WidthPercentage = 100;
                plateTable.SetWidths(new float[] { 30, 20, 25, 25 });

                plateTable.AddCell(CreateCell("", boldFont));
                plateTable.AddCell(CreateCell(dtHeaders.Rows[0]["Col_NoOfPlates"].ToString(), boldFont, Element.ALIGN_CENTER));
                plateTable.AddCell(CreateCell(dtHeaders.Rows[0]["Col_AuthorisedSignature"].ToString(), boldFont, Element.ALIGN_CENTER));
                plateTable.AddCell(CreateCell(dtHeaders.Rows[0]["Col_GuestSignature"].ToString(), boldFont, Element.ALIGN_CENTER));

                foreach (DataRow r in dtPlateRows.Rows)
                {
                    plateTable.AddCell(CreateCell(r["RowTitle"].ToString(), normalFont));
                    plateTable.AddCell(CreateCell("", normalFont));
                    plateTable.AddCell(CreateCell("", normalFont));
                    plateTable.AddCell(CreateCell("", normalFont));
                }

                document.Add(plateTable);

                /* ================= SUMMARY ================= */
                PdfPTable summaryTable = new PdfPTable(4);
                summaryTable.WidthPercentage = 100;

                summaryTable.AddCell(CreateCell(
                    dtSummary.Rows[0]["SummaryTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    4,
                    gray));

                document.Add(summaryTable);

                /* ================= PAYMENT ================= */
                PdfPTable paymentTable = new PdfPTable(3);
                paymentTable.WidthPercentage = 100;
                paymentTable.SetWidths(new float[] { 35, 30, 35 });

                /* ---------- Row 1 ---------- */
                paymentTable.AddCell(CreateCell(
                    dtPayment.Rows[0]["RatePerPlateTitle"].ToString(),
                    boldFont));

                paymentTable.AddCell(CreateCell("", normalFont));

                PdfPCell extraAmountCell = CreateTitleWithSpaceCell(
                    dtPayment.Rows[0]["ExtraAmountTitle"].ToString(),
                    boldFont,
                    55f   // 👈 space height
                );
                extraAmountCell.Rowspan = 2;
                paymentTable.AddCell(extraAmountCell);

                /* ---------- Row 2 ---------- */
                paymentTable.AddCell(CreateCell(
                    dtPayment.Rows[0]["TotalPayableTitle"].ToString(),
                    boldFont));
                paymentTable.AddCell(CreateCell("", normalFont));

                /* ---------- Row 3 ---------- */
                paymentTable.AddCell(CreateCell(
                    dtPayment.Rows[0]["AdvanceAmountTitle"].ToString(),
                    boldFont));
                paymentTable.AddCell(CreateCell("", normalFont));

                PdfPCell advanceModeCell = CreateTitleWithSpaceCell(
                    dtPayment.Rows[0]["AdvancePaymentModeTitle"].ToString(),
                    boldFont,
                    35f
                );
                paymentTable.AddCell(advanceModeCell);

                /* ---------- Row 4 ---------- */
                paymentTable.AddCell(CreateCell(
                    dtPayment.Rows[0]["FinalPaymentTitle"].ToString(),
                    boldFont));
                paymentTable.AddCell(CreateCell("", normalFont));

                PdfPCell finalModeCell = CreateTitleWithSpaceCell(
                    dtPayment.Rows[0]["FinalPaymentModeTitle"].ToString(),
                    boldFont,
                    35f
                );
                paymentTable.AddCell(finalModeCell);

                document.Add(paymentTable);

                /* ================= FOOTER ================= */
                PdfPTable footer = new PdfPTable(1);
                footer.WidthPercentage = 100;

                footer.AddCell(CreateCell(
                    dtFooter.Rows[0]["FeedbackTitle"].ToString(),
                    titleFont,
                    Element.ALIGN_CENTER,
                    1,
                    gray));

                document.Add(footer);

                PdfPTable mainTable = new PdfPTable(2);
                mainTable.WidthPercentage = 100;
                mainTable.SetWidths(new float[] { 65, 35 });

                PdfPTable leftTable = new PdfPTable(1);
                leftTable.WidthPercentage = 100;

                // Feedback image (emoji + stars combined PNG)
                string imgPath = Path.Combine(
                                    _env.WebRootPath,
                                    "assets2",
                                    "img",
                                    "feedback-stars.png"
                                );

                Image feedbackImg = Image.GetInstance(imgPath);
                feedbackImg.ScaleToFit(300f, 200f);
                feedbackImg.Alignment = Image.ALIGN_LEFT;

                PdfPCell imgCell = new PdfPCell(feedbackImg)
                {
                    Border = Rectangle.BOX,
                    Padding = 8,
                    FixedHeight = 100
                };

                leftTable.AddCell(imgCell);


                PdfPTable rightTable = new PdfPTable(1);
                rightTable.WidthPercentage = 100;

                /* COMMENTS */
                PdfPCell commentTitle = new PdfPCell(new Phrase("Comments", boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 4
                };

                PdfPCell commentBox = new PdfPCell(new Phrase(" "))
                {
                    FixedHeight = 100
                };

                rightTable.AddCell(commentTitle);
                rightTable.AddCell(commentBox);

                /* SIGNATURE */
                PdfPCell signTitle = new PdfPCell(new Phrase("Guest Signature", boldFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 4
                };

                PdfPCell signBox = new PdfPCell(new Phrase(" "))
                {
                    FixedHeight = 100
                };

                rightTable.AddCell(signTitle);
                rightTable.AddCell(signBox);

                mainTable.AddCell(new PdfPCell(leftTable));
                mainTable.AddCell(new PdfPCell(rightTable));

                document.Add(mainTable);

                document.NewPage();

                PdfPTable titleTbl = new PdfPTable(1);
                titleTbl.WidthPercentage = 100;
                titleTbl.AddCell(CreateCell2(dtStaffTitle.Rows[0]["StaffUseOnlyTitle"].ToString(), titleFont, Element.ALIGN_CENTER, 1, 30));
                document.Add(titleTbl);
                document.Add(new Paragraph("\n")); // Space

                // ------------------- FOOD WASTAGE TABLE -------------------
                PdfPTable fwTable = new PdfPTable(2); // Left + Right
                fwTable.WidthPercentage = 100;
                fwTable.SetWidths(new float[] { 30f, 70f });

                // Left box: FoodWastageTitle
                PdfPTable leftBox = new PdfPTable(1);
                leftBox.WidthPercentage = 100;

                PdfPCell leftTitleCell = new PdfPCell(
                    new Phrase(dtFoodTitles.Rows[0]["FoodWastageTitle"].ToString(), boldFont)
                );
                leftTitleCell.HorizontalAlignment = Element.ALIGN_CENTER;
                leftTitleCell.VerticalAlignment = Element.ALIGN_TOP;
                leftTitleCell.FixedHeight = 60f;
                leftTitleCell.Border = Rectangle.NO_BORDER;   // ✅ IMPORTANT

                leftBox.AddCell(leftTitleCell);


                // Right box: 3 rows (LocationTime, WeightDoneBy, WeightCrossCheck)
                PdfPTable rightBox = new PdfPTable(2);
                rightBox.WidthPercentage = 100;
                rightBox.SetWidths(new float[] { 50f, 50f });

                // Row 1
                rightBox.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["LocationTimeTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));

                // Row 2
                rightBox.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["WeightDoneByTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));

                // Row 3
                rightBox.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["WeightCrossCheckByTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 0, Rectangle.BOX));

                rightBox.AddCell(CreateCell2(
                    "", normalFont, Element.ALIGN_LEFT, 1, 20, Rectangle.BOX));


                // Add left and right boxes to main table
                fwTable.AddCell(new PdfPCell(leftBox) { Border = Rectangle.BOX, Padding = 5 });
                fwTable.AddCell(new PdfPCell(rightBox) { Border = Rectangle.BOX, Padding = 5 });

                document.Add(fwTable);
                document.Add(new Paragraph("\n"));

                // ================= MAIN CONTAINER TABLE =================
                PdfPTable mainTable2 = new PdfPTable(2);
                mainTable2.WidthPercentage = 100;
                mainTable2.SetWidths(new float[] { 60f, 40f }); // Left (Item) | Right (Staff)

                // =======================================================
                // LEFT SIDE : ITEM & WEIGHT TABLE
                // =======================================================
                PdfPTable itemTable = new PdfPTable(2);
                itemTable.WidthPercentage = 100;
                itemTable.SetWidths(new float[] { 60f, 40f });

                // Header
                itemTable.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["ItemNameTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 1, 25));

                itemTable.AddCell(CreateCell2(
                    dtFoodTitles.Rows[0]["WeightKgTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 1, 25));

                // 7 blank rows
                for (int i = 0; i < 7; i++)
                {
                    itemTable.AddCell(CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 25));
                    itemTable.AddCell(CreateCell2("", normalFont, Element.ALIGN_CENTER, 1, 25));
                }

                // Add LEFT table to main table
                PdfPCell leftCell = new PdfPCell(itemTable);
                leftCell.Border = Rectangle.NO_BORDER;
                leftCell.Padding = 3;
                mainTable2.AddCell(leftCell);

               
                // RIGHT SIDE : STAFF TABLE
                // =======================================================
                PdfPTable staffTbl = new PdfPTable(2);
                staffTbl.WidthPercentage = 100;
                staffTbl.SetWidths(new float[] { 45f, 45f });

                // Staff Header
                staffTbl.AddCell(CreateCell2(
                    dtStaffDuty.Rows[0]["StaffDutyTitle"].ToString(),
                    boldFont, Element.ALIGN_CENTER, 2, 25));

                // --- PRESENT STAFF 
                staffTbl.AddCell(CreateCell2(
                    dtStaffDuty.Rows[0]["StaffPresentTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 50)); // Label height: 50

                staffTbl.AddCell(CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 80)); 

                // --- ABSENT STAFF 
                staffTbl.AddCell(CreateCell2(
                    dtStaffDuty.Rows[0]["StaffAbsentTitle"].ToString(),
                    boldFont, Element.ALIGN_LEFT, 1, 30)); // Label height: 30

                staffTbl.AddCell(CreateCell2("", normalFont, Element.ALIGN_LEFT, 1, 30)); 

                // Add RIGHT table to main table
                PdfPCell rightCell = new PdfPCell(staffTbl);
                rightCell.Border = Rectangle.NO_BORDER;
                rightCell.Padding = 3;
                mainTable2.AddCell(rightCell);

                // =======================================================
                // ADD TO DOCUMENT
                // =======================================================
                document.Add(mainTable2);
                document.Add(new Paragraph("\n"));

                PdfPTable chkTbl = new PdfPTable(2);
                chkTbl.WidthPercentage = 100;

                chkTbl.AddCell(Cell(
                    dtChkTitle.Rows[0]["ChecklistTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    2,
                    30
                ));

                foreach (DataRow r in dtChkItems.Rows)
                {
                    chkTbl.AddCell(Cell(r["ChecklistItem"].ToString(), normalFont));
                    chkTbl.AddCell(Cell("", normalFont));
                }

                document.Add(chkTbl);

                PdfPTable notesTbl = new PdfPTable(1);
                notesTbl.WidthPercentage = 100;

                notesTbl.AddCell(Cell(
                    dtNotes.Rows[0]["EventNotesTitle"].ToString(),
                    boldFont,
                    Element.ALIGN_CENTER,
                    1,
                    30
                ));

                notesTbl.AddCell(Cell("", normalFont, Element.ALIGN_LEFT, 1, 120));
                document.Add(notesTbl);

                document.Close();

                /* ================= SAVE ================= */
                string folder = Path.Combine(_env.WebRootPath, "reports");
                Directory.CreateDirectory(folder);

                string fileName = $"BookingEvent_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                System.IO.File.WriteAllBytes(Path.Combine(folder, fileName), stream.ToArray());

                response.status = "Success";
                response.message = "PDF Generated Successfully";
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

        private PdfPCell Cell(
                         string text,
                         Font font,
                         int align = Element.ALIGN_LEFT,
                         int colspan = 1,
                         float height = 25)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? " ", font));
            cell.Colspan = colspan;
            cell.FixedHeight = height;
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 6;
            cell.Border = Rectangle.BOX;
            return cell;
        }

        PdfPCell CreateTitleWithSpaceCell(string title, Font titleFont, float height)
        {
            PdfPTable inner = new PdfPTable(1);
            inner.WidthPercentage = 100;

            // 🔸 Title (TOP)
            PdfPCell titleCell = new PdfPCell(new Phrase(title, titleFont))
            {
                Border = Rectangle.BOTTOM_BORDER,
                Padding = 4,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            inner.AddCell(titleCell);

            // 🔸 Empty Space for VALUE
            PdfPCell valueCell = new PdfPCell(new Phrase(" "))
            {
                MinimumHeight = height,
                Border = Rectangle.NO_BORDER
            };
            inner.AddCell(valueCell);

            return new PdfPCell(inner)
            {
                Padding = 0
            };
        }


        private PdfPCell CreateCell(
                         string text,
                         Font font,
                         int align = Element.ALIGN_LEFT,
                         int colspan = 1,
                         BaseColor bg = null)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text ?? " ", font));
            cell.Colspan = colspan;
            cell.HorizontalAlignment = align;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Padding = 4;
            cell.Border = Rectangle.BOX;

            if (bg != null)
                cell.BackgroundColor = bg;

            return cell;
        }

        public static PdfPCell CreateCell2(
                                string text,
                                Font font,
                                int hAlign = Element.ALIGN_LEFT,
                                int colspan = 1,
                                float fixedHeight = 0,
                                int border = Rectangle.BOX)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.HorizontalAlignment = hAlign;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.Colspan = colspan;
            cell.Padding = 5;
            cell.Border = border;

            if (fixedHeight > 0)
                cell.FixedHeight = fixedHeight;

            return cell;
        }


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
        [Route("Report_DailySalesDetails")]
        public IActionResult Report_DailySalesDetails()
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

        [HttpPost]
        [Route("Report_MonthlySalesDetails")]
        public IActionResult Report_MonthlySalesDetails()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "MonthlySales_Report_*.pdf");

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
        [Route("Report_RevenueLoss")]
        public IActionResult Report_RevenueLoss()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "RevenueLoss_Report_*.pdf");

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
        [Route("Report_GSTDetails")]
        public IActionResult Report_GSTDetails()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "GST_Report_*.pdf");

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

        [HttpPost]
        [Route("Report_Purchase")]
        public IActionResult Report_Purchase()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Purchase_Report_*.pdf");

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
        [Route("Report_FoodWastage")]
        public IActionResult Report_FoodWastage()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "FoodWastage_Report_*.pdf");

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
        [Route("Report_Expense")]
        public IActionResult Report_Expense()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Expense_Report_*.pdf");

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
        [Route("Report_ProfitLoss")]
        public IActionResult Report_ProfitLoss()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "ProfitLoss_Report_*.pdf");

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
        [Route("Report_Cashflow")]
        public IActionResult Report_Cashflow()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Cashflow_Report_*.pdf");

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
        [Route("Report_Modification")]
        public IActionResult Report_Modification()
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
                string filePath = Path.Combine(folder, fileName);

                Utils.deleteOldFiles(folder, "Modification_Report_*.pdf");

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
