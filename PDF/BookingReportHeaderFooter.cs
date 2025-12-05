using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace ERPAdmin.PDF
{
    public class BookingReportHeaderFooter : PdfPageEventHelper
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string LogoPath { get; set; }

        private PdfContentByte _cb;
        private PdfTemplate _template;

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            _cb = writer.DirectContent;
            _template = _cb.CreateTemplate(50, 50);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            float pageWidth = document.PageSize.Width;
            float pageHeight = document.PageSize.Height;

            // ========== HEADER ==========
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.TotalWidth = pageWidth - document.LeftMargin - document.RightMargin;
            headerTable.LockedWidth = true;
            headerTable.SetWidths(new float[] { 1f, 3f });

            // Logo
            if (!string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(LogoPath);
                logo.ScaleAbsolute(60, 60);

                PdfPCell logoCell = new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                headerTable.AddCell(logoCell);
            }
            else
            {
                PdfPCell emptyCell = new PdfPCell(new Phrase(""))
                {
                    Border = Rectangle.NO_BORDER
                };
                headerTable.AddCell(emptyCell);
            }

            // Company Details
            Font headerFontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable companyTable = new PdfPTable(1);
            companyTable.AddCell(new PdfPCell(new Phrase(CompanyName, headerFontBold)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
            companyTable.AddCell(new PdfPCell(new Phrase(Address, headerFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            PdfPCell companyCell = new PdfPCell(companyTable)
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            headerTable.AddCell(companyCell);

            // Draw header table
            headerTable.WriteSelectedRows(0, -1, document.LeftMargin, pageHeight - 10, writer.DirectContent);

            // ========== FOOTER ==========
            PdfPTable footerTable = new PdfPTable(1);
            footerTable.TotalWidth = pageWidth - document.LeftMargin - document.RightMargin;

            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.GRAY);
            PdfPCell footerCell = new PdfPCell(new Phrase($"Page {writer.PageNumber}", footerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_CENTER
            };

            footerTable.AddCell(footerCell);
            footerTable.WriteSelectedRows(0, -1, document.LeftMargin, document.BottomMargin - 5, writer.DirectContent);
        }
    }
}
