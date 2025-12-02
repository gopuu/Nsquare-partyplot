using System.Data;

namespace ERPAdmin.PDF
{
    public static class TemplateDataProvider
    {
        public static DataTable GetCompanyInfo()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("CompanyName");
            dt.Columns.Add("Address");
            dt.Columns.Add("LogoPath");

            dt.Rows.Add(
                "NSquare Software Solutions",
                "Surat, Gujarat, India",
                "image_icon.png"
            );

            return dt;
        }

        public static DataSet GetInvoiceData()
        {
            DataSet ds = new DataSet();

            // CLIENT
            DataTable client = new DataTable();
            client.Columns.Add("ClientName");
            client.Columns.Add("InvoiceNo");
            client.Columns.Add("Date");

            client.Rows.Add(
                "Demo Client",
                "INV-1001",
                DateTime.Now.ToString("dd/MM/yyyy")
            );

            ds.Tables.Add(client);

            // ITEMS
            DataTable items = new DataTable();
            items.Columns.Add("Item");
            items.Columns.Add("Qty");
            items.Columns.Add("Price");
            items.Columns.Add("Total");

            items.Rows.Add("Laptop", "1", "50000", "50000");
            items.Rows.Add("Mouse", "2", "500", "1000");

            ds.Tables.Add(items);

            return ds;
        }
    }
}
