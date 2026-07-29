namespace ERPAdmin.Models
{
    //All invoice download
    public class GSTSalesAllInvoiceReportRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public long? InvoiceNo { get; set; }
        public string? NameEmailContactNo { get; set; }
        public bool? IsWithGST { get; set; }
    }

    // ── Request Models ──────────────────────────────────────────────

    public class GSTSalesGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public long? InvoiceNo { get; set; }
        public string? NameEmailContactNo { get; set; }
        public bool? IsWithGST { get; set; }
    }

    public class GSTSalesReportRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public long? InvoiceNo { get; set; }
        public string? NameEmailContactNo { get; set; }
        public bool? IsWithGST { get; set; }

        public bool? WithHeader { get; set; }
        public List<string>? Columns { get; set; }
    }

    // ── Grid Model ──────────────────────────────────────────────────
    public class GSTSalesGridModel
    {
        public long InvoiceNo { get; set; }
        public string? GSTInvoiceNo { get; set; }
        public string? DisplayInvoiceNo { get; set; }

        public long? BookingId { get; set; }

        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }

        public string? Name { get; set; }
        public string? GSTIN { get; set; }
        public string? EventType { get; set; }

        public decimal GrossSale { get; set; }
        public decimal Discount { get; set; }
        public decimal NetSale { get; set; }
        public decimal TaxableAmount { get; set; }

        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }

        public decimal TaxAmount { get; set; }
        public decimal InvoiceAmount { get; set; }

        public string? PaymentMode { get; set; }

        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
    }
    public class GSTSalesTotalsModel
    {
        public decimal TotalGrossSale { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalNetSale { get; set; }
        public decimal TotalTaxableAmount { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalDueAmount { get; set; }
    }

}
