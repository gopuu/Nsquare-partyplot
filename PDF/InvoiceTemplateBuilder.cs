using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;

namespace ERPAdmin.PDF
{
    public class InvoiceTemplateBuilder
    {
        private readonly IWebHostEnvironment _env;

        public InvoiceTemplateBuilder(IWebHostEnvironment env)
        {
            _env = env;
        }

        //INVOICE TEMPLATE
        public void Generate_PurchaseInvoiceDefault1_A4_Portrait(string pdfPath)
        {
            var company = TemplateDataProvider.GetCompanyInfo().Rows[0];
            var data = TemplateDataProvider.GetInvoiceData();

            Document doc = new Document(PageSize.A4, 25, 25, 100, 60);

            using FileStream fs = new FileStream(pdfPath, FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            writer.PageEvent = new InvoicePage_HeaderFooter_Layout
            {
                CompanyName = company["CompanyName"].ToString(),
                Address = company["Address"].ToString(),
                //LogoPath = Path.Combine(_env.WebRootPath, company["LogoPath"].ToString())
                LogoPath = Path.Combine(_env.WebRootPath, "assets2", "img", company["LogoPath"].ToString().TrimStart('/'))
            };

            doc.Open();

            AddClientSection(doc, data.Tables[0]);
            AddItemSection(doc, data.Tables[1]);

            doc.Close();
        }

        public void Generate_PurchaseInvoiceDefault1_A4_Landscape(string pdfPath)
        {
            var company = TemplateDataProvider.GetCompanyInfo().Rows[0];
            var data = TemplateDataProvider.GetInvoiceData();

            Document doc = new Document(PageSize.A4.Rotate(), 25, 25, 100, 60);

            using FileStream fs = new FileStream(pdfPath, FileMode.Create);
            PdfWriter writer = PdfWriter.GetInstance(doc, fs);

            writer.PageEvent = new InvoicePage_HeaderFooter_Layout
            {
                CompanyName = company["CompanyName"].ToString(),
                Address = company["Address"].ToString(),
                LogoPath = Path.Combine(_env.WebRootPath, company["LogoPath"].ToString())
            };

            doc.Open();

            AddClientSection(doc, data.Tables[0]);
            AddItemSection(doc, data.Tables[1]);

            doc.Close();
        }

        // -------------------- HELPERS --------------------

        private void AddClientSection(Document doc, DataTable client)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;

            table.AddCell("Client Name:");
            table.AddCell(client.Rows[0]["ClientName"].ToString());

            table.AddCell("Invoice No:");
            table.AddCell(client.Rows[0]["InvoiceNo"].ToString());

            table.AddCell("Date:");
            table.AddCell(client.Rows[0]["Date"].ToString());

            doc.Add(table);
            doc.Add(new Paragraph("\n"));
        }

        private void AddItemSection(Document doc, DataTable items)
        {
            PdfPTable table = new PdfPTable(items.Columns.Count);
            table.WidthPercentage = 100;

            // headers
            foreach (DataColumn col in items.Columns)
            {
                table.AddCell(new PdfPCell(new Phrase(col.ColumnName))
                {
                    BackgroundColor = BaseColor.LIGHT_GRAY
                });
            }

            // rows
            foreach (DataRow dr in items.Rows)
            {
                foreach (var v in dr.ItemArray)
                    table.AddCell(v.ToString());
            }

            doc.Add(table);
        }

        //INVOICE TEMPLATE
    }
}
