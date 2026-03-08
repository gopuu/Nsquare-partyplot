namespace ERPAdmin.Models
{
    public class PurchaseReportGridRequest
    {
        public long SupplierId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class PurchaseReportRequest
    {
        public long SupplierId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class PurchaseReportGridModel
    {
        public long SrNo { get; set; }
        public long PurchaseId { get; set; }
        public string? InvoiceDate { get; set; }
        public string? InvoiceNo { get; set; }
        public string? SupplierName { get; set; }
        public long SupplierId { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal Outstanding { get; set; }
    }
}
