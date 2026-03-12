namespace ERPAdmin.Models
{
    // ── Request Models ──────────────────────────────────────────────

    public class GSTSalesGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public long? InvoiceNo { get; set; }
        public string? NameEmailContactNo { get; set; }
    }

    public class GSTSalesReportRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public long? InvoiceNo { get; set; }
        public string? NameEmailContactNo { get; set; }

        public bool? WithHeader { get; set; }
        public List<string>? Columns { get; set; }
    }

    // ── Grid Model ──────────────────────────────────────────────────

    public class GSTSalesGridModel
    {
        public long InvoiceNo { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceDate_Format { get; set; }
        public string? Name { get; set; }
        public string? GSTIN { get; set; }
        public string? EventType { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SGST { get; set; }
        public decimal CGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public string? PaymentMode { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
    }

    //public class GSTSalesTotalsModel
    //{
    //    public decimal TotalTaxableAmount { get; set; }
    //    public decimal TotalCGST { get; set; }
    //    public decimal TotalSGST { get; set; }
    //    public decimal TotalIGST { get; set; }
    //    public decimal TotalGST { get; set; }
    //    public decimal TotalGrandTotal { get; set; }
    //}

    public class GSTSalesTotalsModel
    {
        public decimal TotalSubTotal { get; set; }
        public decimal TotalCGST { get; set; }
        public decimal TotalSGST { get; set; }
        public decimal TotalIGST { get; set; }
        public decimal TotalTax { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalDueAmount { get; set; }
    }

}
