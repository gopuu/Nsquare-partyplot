namespace ERPAdmin.Models
{
    // GRID REQUEST
    public class DailySalesGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class DailySalesDateDetailsRequest: BaseClass
    {
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public string? InvoiceDate { get; set; }
    }

    // REPORT REQUEST
    public class DailySalesReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class DailySalesDateDetailsReportRequest
    {
        public string? InvoiceDate { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    // GRID RESPONSE
    public class DailySalesGridModel
    {
        public long SrNo { get; set; }
        public string? Date { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }
        public int TotalInvoices { get; set; }
        public decimal GrossSales { get; set; }
        public decimal Taxes { get; set; }
        public decimal NetSales { get; set; }
        public decimal PaymentAmount { get; set; }
        public string? PaymentMode { get; set; }
        public decimal DuePayments { get; set; }
    }
    public class DailySalesDateDetailsTotalsModel
    {
        public int TotalInvoices { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal Taxes { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
    }

    public class DailySalesTotalsModel
    {
        public int TotalInvoices { get; set; }
        public decimal GrossSales { get; set; }
        public decimal Taxes { get; set; }
        public decimal NetSales { get; set; }
        public decimal TotalPayments { get; set; }
        public decimal DuePayments { get; set; }
    }

    public class DailySalesDateDetailsModel
    {
        public long SrNo { get; set; }
        public string? InvoiceNo { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }
        public string? ClientName { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal Taxes { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string? PaymentModes { get; set; }
    }
}