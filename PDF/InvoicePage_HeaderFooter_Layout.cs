using iTextSharp.text;
using iTextSharp.text.pdf;

namespace ERPAdmin.PDF
{
    public class InvoicePage_HeaderFooter_Layout : PdfPageEventHelper
    {
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string LogoPath { get; set; }

        Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        Font normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.TotalWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;
            headerTable.SetWidths(new float[] { 20f, 80f });

            PdfPCell logoCell = new PdfPCell();

            // DEBUG
            Console.WriteLine("=== DEBUG LOGO PATH ===");
            Console.WriteLine("LogoPath Raw => " + LogoPath);
            Console.WriteLine("File.Exists => " + File.Exists(LogoPath));
            Console.WriteLine("========================");

            if (!string.IsNullOrEmpty(LogoPath) && File.Exists(LogoPath))
            {
                Image logo = Image.GetInstance(LogoPath);
                logo.ScaleToFit(60, 60);
                logoCell.AddElement(logo);
            }
            logoCell.Border = Rectangle.NO_BORDER;

            // COMPANY INFO
            PdfPCell infoCell = new PdfPCell();
            infoCell.AddElement(new Paragraph(CompanyName, headerFont));
            infoCell.AddElement(new Paragraph(Address, normalFont));
            infoCell.Border = Rectangle.NO_BORDER;

            headerTable.AddCell(logoCell);
            headerTable.AddCell(infoCell);

            headerTable.WriteSelectedRows(
                0, -1,
                document.LeftMargin,
                document.PageSize.Height - 20,
                writer.DirectContent
            );

            // FOOTER
            PdfPTable footerTable = new PdfPTable(1);
            footerTable.TotalWidth = headerTable.TotalWidth;

            PdfPCell footerCell = new PdfPCell(
                new Phrase("Page " + writer.PageNumber, normalFont)
            );
            footerCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            footerCell.Border = Rectangle.NO_BORDER;

            footerTable.AddCell(footerCell);

            footerTable.WriteSelectedRows(
                0, -1,
                document.LeftMargin,
                document.BottomMargin - 10,
                writer.DirectContent
            );
        }
    }
}
