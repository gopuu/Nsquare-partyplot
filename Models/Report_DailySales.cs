namespace ERPAdmin.Models
{
    public class DailySalesGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class DailySalesReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }
    public class DailySalesGridModel
    {
        public long SrNo { get; set; }
        public string? Date { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceNos { get; set; }
        public int TotalInvoices { get; set; }
        public decimal GrossSales { get; set; }
        public decimal GST { get; set; }
        public decimal NetSales { get; set; }
        public decimal Cash { get; set; }
        public decimal Online { get; set; }
        public decimal Due { get; set; }
        public decimal Total { get; set; }
    }
    public class DailySalesTotalsModel
    {
        public int TotalInvoices { get; set; }
        public decimal GrossSales { get; set; }
        public decimal GST { get; set; }
        public decimal NetSales { get; set; }
        public decimal Cash { get; set; }
        public decimal Online { get; set; }
        public decimal Due { get; set; }
        public decimal Total { get; set; }
    }
}
