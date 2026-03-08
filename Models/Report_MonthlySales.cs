namespace ERPAdmin.Models
{
    public class MonthlySalesGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class MonthlySalesReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class MonthlySalesGridModel
    {
        public long SrNo { get; set; }
        public int Yr { get; set; }
        public int Mn { get; set; }
        public string? Month { get; set; }
        public string? InvoiceNos { get; set; }
        public int TotalInvoices { get; set; }
        public decimal GrossSales { get; set; }
        public decimal CGST { get; set; }
        public decimal NetSales { get; set; }
        public decimal Cash { get; set; }
        public decimal Online { get; set; }
        public decimal Due { get; set; }
        public decimal Total { get; set; }
    }

    public class MonthlySalesTotalsModel
    {
        public int TotalInvoices { get; set; }
        public decimal GrossSales { get; set; }
        public decimal CGST { get; set; }
        public decimal NetSales { get; set; }
        public decimal Cash { get; set; }
        public decimal Online { get; set; }
        public decimal Due { get; set; }
        public decimal Total { get; set; }
    }
}
