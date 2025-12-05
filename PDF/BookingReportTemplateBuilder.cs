using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Data;
using System.IO;

namespace ERPAdmin.PDF
{
    public class BookingReportTemplateBuilder
    {
        private readonly IWebHostEnvironment _env;

        public BookingReportTemplateBuilder(IWebHostEnvironment env)
        {
            _env = env;
        }

        // ===== Booking Report A4 Portrait =====
        public void Generate_BookingReport_A4_Portrait(string pdfPath)
        {
            var company = BookingReportDataProvider.GetCompanyInfo().Rows[0];
            var bookingData = BookingReportDataProvider.GetBookingData();
            var itemsData = BookingReportDataProvider.GetBookingItems();

            Document doc = new Document(PageSize.A4, 30, 30, 60, 40);

            using FileStream fs = new FileStream(pdfPath, FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            // Optional: add header/footer if needed
            writer.PageEvent = new BookingReportHeaderFooter
            {
                CompanyName = company["CompanyName"].ToString(),
                Address = company["Address"].ToString(),
                LogoPath = Path.Combine(_env.WebRootPath, "assets2", "img", company["LogoPath"].ToString())
            };

            doc.Open();

            // 1️⃣ Add Booking Details
            AddBookingDetailsSection(doc, bookingData);

            // 2️⃣ Add Package Items
            AddBookingItemsSection(doc, itemsData);

            // 3️⃣ Add Summary
            AddBookingSummarySection(doc, bookingData);

            doc.Close();
        }

        // -------------------- Sections --------------------

        private void AddBookingDetailsSection(Document doc, DataTable bookingData)
        {
            Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11);
            Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 1f, 1f });

            foreach (DataRow row in bookingData.Rows)
            {
                table.AddCell(new PdfPCell(new Phrase("Booking Ref#", labelFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(row["BookingRef"].ToString(), valueFont)) { Border = Rectangle.NO_BORDER });

                table.AddCell(new PdfPCell(new Phrase("Client Name", labelFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(row["ClientName"].ToString(), valueFont)) { Border = Rectangle.NO_BORDER });

                table.AddCell(new PdfPCell(new Phrase("Event Date", labelFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(row["EventDate"].ToString(), valueFont)) { Border = Rectangle.NO_BORDER });

                table.AddCell(new PdfPCell(new Phrase("Property", labelFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(row["Property"].ToString(), valueFont)) { Border = Rectangle.NO_BORDER });

                table.AddCell(new PdfPCell(new Phrase("Hall", labelFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(row["Hall"].ToString(), valueFont)) { Border = Rectangle.NO_BORDER });

                table.AddCell(new PdfPCell(new Phrase("Slot", labelFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(row["Slot"].ToString(), valueFont)) { Border = Rectangle.NO_BORDER });

                table.AddCell(new PdfPCell(new Phrase("Timing", labelFont)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(row["Timing"].ToString(), valueFont)) { Border = Rectangle.NO_BORDER });
            }

            doc.Add(table);
            doc.Add(new Paragraph("\n"));
        }

        private void AddBookingItemsSection(Document doc, DataTable itemsData)
        {
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);
            Font rowFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);

            PdfPTable table = new PdfPTable(itemsData.Columns.Count);
            table.WidthPercentage = 100;

            // Header
            foreach (DataColumn col in itemsData.Columns)
            {
                table.AddCell(new PdfPCell(new Phrase(col.ColumnName, headerFont))
                {
                    BackgroundColor = new BaseColor(82, 86, 88),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                });
            }

            // Rows
            foreach (DataRow dr in itemsData.Rows)
            {
                foreach (var v in dr.ItemArray)
                {
                    table.AddCell(new PdfPCell(new Phrase(v.ToString(), rowFont))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        PaddingTop = 4,
                        PaddingBottom = 4,
                        PaddingLeft = 6,
                        PaddingRight = 6
                    });
                }
            }

            doc.Add(table);
            doc.Add(new Paragraph("\n"));
        }

        private void AddBookingSummarySection(Document doc, DataTable bookingData)
        {
            Font summaryFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.WHITE);

            PdfPTable summaryTable = new PdfPTable(3);
            summaryTable.WidthPercentage = 100;
            summaryTable.SetWidths(new float[] { 0.33f, 0.33f, 0.33f });

            var row = bookingData.Rows[0];

            summaryTable.AddCell(new PdfPCell(new Phrase("Deal Amount: " + row["DealAmount"], summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase("Booking Discount: " + row["BookingDiscount"], summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase("Payable Amount: " + row["PayableAmount"], summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), Padding = 6 });

            summaryTable.AddCell(new PdfPCell(new Phrase("Total Paid: " + row["TotalPaid"], summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase("Cash Discount: " + row["CashDiscount"], summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), Padding = 6 });
            summaryTable.AddCell(new PdfPCell(new Phrase("Outstanding Amount: " + row["OutstandingAmount"], summaryFont)) { Border = Rectangle.NO_BORDER, BackgroundColor = new BaseColor(82, 86, 88), Padding = 6 });

            doc.Add(summaryTable);
        }
    }
}
