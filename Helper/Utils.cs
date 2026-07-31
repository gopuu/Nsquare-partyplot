using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;

namespace ERPAdmin.Helper
{
    public static class Utils
    {

        public static void deleteOldFiles(string dirPath, string filePattern, int olderThanMinutes=60)
        {
            if (!Directory.Exists(dirPath))
                return;

            var files = Directory.GetFiles(dirPath, filePattern);
            //var cutoffTime = DateTime.Now.AddHours(-1); // 🔹 1 hour ago
            var cutoffTime = DateTime.Now.AddMinutes(-olderThanMinutes);

            foreach (string file in files)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);

                    // Use LastWriteTime (or CreationTime if you prefer)
                    if (fileInfo.LastWriteTime < cutoffTime)
                    {
                        fileInfo.Delete();
                    }
                }
                catch
                {
                    // optional: log exception
                }
            }
        }
        public static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }



        public static PdfPCell CreateHeaderCell(string text, Font font, BaseColor bgColor)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                BackgroundColor = bgColor,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 6,
                BorderColor = new BaseColor(220, 220, 220), // light gray
                BorderWidth = 0.5f
            };
        }
        public static PdfPCell CreateBodyCell(string text, Font font, int align = Element.ALIGN_LEFT, float padding = 4f)
        {
            return new PdfPCell(new Phrase(text, font))
            {
                HorizontalAlignment = align,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = padding,
                BorderColor = new BaseColor(220, 220, 220), // light gray
                BorderWidth = 0.5f
            };
        }
        public static PdfPCell Cell(string text, Font font, int align = Element.ALIGN_LEFT, int colspan = 1, float height = 25)
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
        public static PdfPCell CreateTitleWithSpaceCell(string title, Font titleFont, float height)
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
        public static PdfPCell CreateTitleWithValueCell(string title, string value, Font titleFont, Font valueFont, float minHeight)
        {
            PdfPTable inner = new PdfPTable(1);
            inner.WidthPercentage = 100;

            // 🔹 Title
            PdfPCell titleCell = new PdfPCell(new Phrase(title, titleFont))
            {
                Border = Rectangle.BOTTOM_BORDER,
                Padding = 4,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            inner.AddCell(titleCell);

            // 🔹 Value (directly below title)
            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont))
            {
                MinimumHeight = minHeight,
                Padding = 6,
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Border = Rectangle.NO_BORDER
            };
            inner.AddCell(valueCell);

            return new PdfPCell(inner)
            {
                Padding = 0
            };
        }

        public static PdfPCell CreateCell(string text, Font font, int align = Element.ALIGN_LEFT, int colspan = 1, BaseColor bg = null)
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
        public static PdfPCell CreateCell2(string text, Font font, int hAlign = Element.ALIGN_LEFT, int colspan = 1, float fixedHeight = 0, int border = Rectangle.BOX)
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
        public static PdfPCell CreateInfoCell(string label, object? value, bool bold = false)
        {
            Font font = FontFactory.GetFont(FontFactory.HELVETICA, bold ? 10 : 9, bold ? Font.BOLD : Font.NORMAL);
            return new PdfPCell(new Phrase($"{label} : {value}", font))
            {
                Border = Rectangle.NO_BORDER,
                Padding = 4
            };
        }
        public static PdfPCell CreateTotalLabelCell(string text, bool isBold = false)
        {
            return new PdfPCell(new Phrase(
                text,
                FontFactory.GetFont(FontFactory.HELVETICA, 9, isBold ? Font.BOLD : Font.NORMAL)
            ))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 4
            };
        }
        public static PdfPCell CreateTotalValueCell(object value, bool isBold = false)
        {
            return new PdfPCell(new Phrase(
                Convert.ToDecimal(value).ToString("N2"),
                FontFactory.GetFont(FontFactory.HELVETICA, 9, isBold ? Font.BOLD : Font.NORMAL)
            ))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 4
            };
        }


    }
}
