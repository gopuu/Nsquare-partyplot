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

    //STANDARD HEADER-FOOTER FORMAT
    public class PdfHeaderFooter_StandardReport : PdfPageEventHelper
    {
        private string companyName, companyAddress, contactNo, gstNo, panNo, stateCode, logoPath, generatedDate;

        public PdfHeaderFooter_StandardReport(
            string companyName, string companyAddress, string contactNo,
            string gstNo, string panNo, string stateCode, string logoPath, string generatedDate)
        {
            this.companyName = companyName;
            this.companyAddress = companyAddress;
            this.contactNo = contactNo;
            this.gstNo = gstNo;
            this.panNo = panNo;
            this.stateCode = stateCode;
            this.logoPath = logoPath;
            this.generatedDate = generatedDate;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfContentByte cb = writer.DirectContent;

            /* ================= HEADER ================= */
            PdfPTable headerTable = new PdfPTable(2);
            headerTable.TotalWidth = document.PageSize.Width - 50;
            headerTable.SetWidths(new float[] { 20, 80 });

            // Logo
            if (!string.IsNullOrEmpty(logoPath))
            {
                try
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleToFit(60f, 60f);
                    PdfPCell logoCell = new PdfPCell(logo)
                    {
                        Border = 0,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    headerTable.AddCell(logoCell);
                }
                catch
                {
                    headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });
                }
            }
            else
            {
                headerTable.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });
            }

            // Company Info
            Font bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            Font normal = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            PdfPCell infoCell = new PdfPCell
            {
                Border = 0, //iTextSharp.text.Rectangle.BOX,
                HorizontalAlignment = Element.ALIGN_LEFT
            };

            infoCell.AddElement(new Phrase(companyName, bold));
            infoCell.AddElement(new Phrase(companyAddress, normal));
            infoCell.AddElement(new Phrase($"Contact: {contactNo}", normal));
            infoCell.AddElement(new Phrase($"GST: {gstNo} | PAN: {panNo}", normal));

            headerTable.AddCell(infoCell);

            headerTable.WriteSelectedRows(
                0, -1,
                25,
                document.PageSize.Height - 25,
                cb
            );

            /* ================= FOOTER ================= */
            PdfPTable footer = new PdfPTable(3);
            footer.TotalWidth = document.PageSize.Width - 50;
            footer.SetWidths(new float[] { 33, 34, 33 });

            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            // Left (empty or future use)
            footer.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });

            // Center → Page Number
            PdfPCell pageCell = new PdfPCell(
                new Phrase($"Page {writer.PageNumber}", footerFont)
            )
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            footer.AddCell(pageCell);

            // Right → Generated Date
            PdfPCell dateCell = new PdfPCell(
                new Phrase($"Generated: {generatedDate:dd-MM-yyyy HH:mm}", footerFont)
            )
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            footer.AddCell(dateCell);

            footer.WriteSelectedRows(
                0, -1,
                25,
                30,
                cb
            );

            /* ================= PAGE BORDER ================= */
            cb.Rectangle(
                document.LeftMargin - 5,
                document.BottomMargin - 5,
                document.PageSize.Width - document.LeftMargin - document.RightMargin + 10,
                document.PageSize.Height - document.TopMargin - document.BottomMargin + 10
            );

            cb.SetLineWidth(0.5f);
            cb.Stroke();
        }
    }


}
