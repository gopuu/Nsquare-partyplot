namespace ERPAdmin.Models
{
    public class GSTSalesGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class GSTSalesReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class GSTSalesGridModel
    {
        public long SrNo { get; set; }
        public int Yr { get; set; }
        public int Mn { get; set; }
        public string? Month { get; set; }
        public string? GSTType { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal CGST { get; set; }
        public decimal SGST { get; set; }
        public decimal IGST { get; set; }
        public decimal TotalGST { get; set; }
        public decimal GrandTotal { get; set; }
    }
}
