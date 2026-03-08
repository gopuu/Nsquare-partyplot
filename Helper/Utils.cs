using iTextSharp.text;
using iTextSharp.text.pdf;

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

    }
}
