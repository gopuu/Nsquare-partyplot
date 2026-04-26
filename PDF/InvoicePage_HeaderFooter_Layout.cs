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
    public class PdfHeaderFooter_StandardReport_A4_Rotate : PdfPageEventHelper
    {
        private string propertyName, companyName, companyAddress, contactNo, gstNo, emailId,
            stateCode, logoPath, generatedDate, username;
        private bool? withHeader;

        public PdfHeaderFooter_StandardReport_A4_Rotate(
            string propertyName, string companyName, string companyAddress, string contactNo,
            string gstNo, string emailId, string stateCode, string logoPath, string generatedDate, string username, bool? withHeader=true)
        {
            this.propertyName = propertyName;
            this.companyName = companyName;
            this.companyAddress = companyAddress;
            this.contactNo = contactNo;
            this.gstNo = gstNo;
            this.emailId = emailId;
            this.stateCode = stateCode;
            this.logoPath = logoPath;
            this.generatedDate = generatedDate;
            this.username = username;
            this.withHeader = withHeader;
        }
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            PdfContentByte cb = writer.DirectContent;

            // Border settings -- START
            float borderMargin = 10f; // distance from page edge
            float lineWidth = 0.5f;

            Rectangle rect = new Rectangle(
                document.PageSize.Left + borderMargin,
                document.PageSize.Bottom + borderMargin,
                document.PageSize.Right - borderMargin,
                document.PageSize.Top - borderMargin
            );

            rect.Border = Rectangle.BOX;
            rect.BorderWidth = lineWidth;
            rect.BorderColor = BaseColor.GRAY;

            cb.Rectangle(rect);
            // Border settings -- END

            float pageWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;

            PdfPTable headerTable = new PdfPTable(2);
            headerTable.TotalWidth = pageWidth;
            headerTable.SetWidths(new float[] { 14f, 86f });

            // Logo
            PdfPCell logoCell;
            if (!string.IsNullOrEmpty(logoPath))
            {
                try
                {
                    var logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(60f, 60f);
                    logoCell = new PdfPCell(logo);
                }
                catch
                {
                    logoCell = new PdfPCell(new Phrase(""));
                }
            }
            else
            {
                logoCell = new PdfPCell(new Phrase(""));
            }

            logoCell.Border = Rectangle.BOTTOM_BORDER;
            logoCell.Padding = 5;
            logoCell.BorderColor = BaseColor.LIGHT_GRAY;
            logoCell.BorderWidth = 0.5f;
            headerTable.AddCell(logoCell);

            // Company Info
            Font bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font normal = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);

            PdfPCell infoCell = new PdfPCell
            {
                Border = Rectangle.BOTTOM_BORDER,
                Padding = 5,
                PaddingTop = 2,
                HorizontalAlignment = Element.ALIGN_CENTER,
                BorderWidth = 0.5f,
                BorderColor = BaseColor.LIGHT_GRAY
            };

            infoCell.AddElement(new Paragraph(propertyName, bold));
            infoCell.AddElement(new Paragraph(companyName, normal));
            infoCell.AddElement(new Paragraph(companyAddress, normal));
            infoCell.AddElement(new Paragraph($"Contact: {contactNo} | Email: {emailId} | GSTIN: {gstNo}", normal));

            if (withHeader == true) {
                headerTable.AddCell(infoCell);
            }

            headerTable.WriteSelectedRows(
                0, -1,
                document.LeftMargin,
                document.PageSize.Height - 10,
                cb
            );

            /* ================= FOOTER ================= */
            PdfPTable footer = new PdfPTable(3);
            footer.TotalWidth = pageWidth;
            footer.SetWidths(new float[] { 33, 34, 33 });

            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            //footer.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });

            footer.AddCell(new PdfPCell(new Phrase($"Generated by: {username}", footerFont))
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            footer.AddCell(new PdfPCell(new Phrase($"Page {writer.PageNumber}", footerFont))
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            footer.AddCell(new PdfPCell(new Phrase($"Generated: {generatedDate}", footerFont))
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            footer.WriteSelectedRows(
                0, -1,
                document.LeftMargin,
                document.BottomMargin - 10,
                cb
            );
        }
    }

    public class PdfHeaderFooter_StandardReport_A4 : PdfPageEventHelper
    {
        private string propertyName, companyName, companyAddress, contactNo, gstNo, emailId,
            stateCode, logoPath, generatedDate, username;
        private bool? withHeader;

        public PdfHeaderFooter_StandardReport_A4(
            string propertyName, string companyName, string companyAddress, string contactNo,
            string gstNo, string emailId, string stateCode, string logoPath, 
            string generatedDate , string username, bool? withHeader)
        {
            this.propertyName = propertyName;
            this.companyName = companyName;
            this.companyAddress = companyAddress;
            this.contactNo = contactNo;
            this.gstNo = gstNo;
            this.emailId = emailId;
            this.stateCode = stateCode;
            this.logoPath = logoPath;
            this.generatedDate = generatedDate;
            this.username = username;
            this.withHeader = withHeader;
        }
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            #region PAGE BORDER
            // Border settings -- START
            PdfContentByte cb = writer.DirectContent;
            float borderMargin = 10f; // distance from page edge
            float lineWidth = 0.5f;

            Rectangle rect = new Rectangle(
                document.PageSize.Left + borderMargin,
                document.PageSize.Bottom + borderMargin,
                document.PageSize.Right - borderMargin,
                document.PageSize.Top - borderMargin
            );

            rect.Border = Rectangle.BOX;
            rect.BorderWidth = lineWidth;
            rect.BorderColor = BaseColor.GRAY;
            cb.Rectangle(rect);
            // Border settings -- END
            #endregion

            #region PAGE HEADER
            float pageWidth = document.PageSize.Width - document.LeftMargin - document.RightMargin;

            PdfPTable headerTable = new PdfPTable(2);
            headerTable.TotalWidth = pageWidth;
            headerTable.SetWidths(new float[] { 20f, 80f });

            // Logo
            PdfPCell logoCell;
            if (!string.IsNullOrEmpty(logoPath))
            {
                try
                {
                    var logo = Image.GetInstance(logoPath);
                    logo.ScaleToFit(60f, 60f);
                    logoCell = new PdfPCell(logo);
                }
                catch
                {
                    logoCell = new PdfPCell(new Phrase(""));
                }
            }
            else
            {
                logoCell = new PdfPCell(new Phrase(""));
            }

            logoCell.Border = Rectangle.BOTTOM_BORDER;
            logoCell.Padding = 5;
            logoCell.BorderColor = BaseColor.LIGHT_GRAY;
            logoCell.BorderWidth = 0.5f;
            headerTable.AddCell(logoCell);

            // Company Info
            Font bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            Font normal = FontFactory.GetFont(FontFactory.HELVETICA, 9, BaseColor.DARK_GRAY);

            PdfPCell infoCell = new PdfPCell
            {
                Border = Rectangle.BOTTOM_BORDER,
                Padding = 5,
                PaddingTop = 2,
                HorizontalAlignment = Element.ALIGN_CENTER,
                BorderWidth = 0.5f,
                BorderColor = BaseColor.LIGHT_GRAY
            };

            infoCell.AddElement(new Paragraph(propertyName, bold));
            infoCell.AddElement(new Paragraph(companyName, normal));
            infoCell.AddElement(new Paragraph(companyAddress, normal));
            infoCell.AddElement(new Paragraph($"Contact: {contactNo} | Email: {emailId} | GSTIN: {gstNo}", normal));

            if (withHeader == true) {
                headerTable.AddCell(infoCell);
            }

            headerTable.WriteSelectedRows(
                0, -1,
                document.LeftMargin,
                document.PageSize.Height - 10,
                cb
            );
            #endregion

            /* ================= FOOTER ================= */
            #region FOOTER
            PdfPTable footer = new PdfPTable(3);
            footer.TotalWidth = pageWidth;
            footer.SetWidths(new float[] { 33, 34, 33 });

            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);

            //footer.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });

            footer.AddCell(new PdfPCell(new Phrase($"Generated by: {username}", footerFont))
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_LEFT
            });

            footer.AddCell(new PdfPCell(new Phrase($"Page {writer.PageNumber}", footerFont))
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_CENTER
            });

            footer.AddCell(new PdfPCell(new Phrase($"Generated: {generatedDate}", footerFont))
            {
                Border = 0,
                HorizontalAlignment = Element.ALIGN_RIGHT
            });

            footer.WriteSelectedRows(
                0, -1,
                document.LeftMargin,
                document.BottomMargin - 10,
                cb
            );
            #endregion
        }
    }


}
