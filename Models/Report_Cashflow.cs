namespace ERPAdmin.Models
{
    public class CashflowReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class CashflowLineItem
    {
        public long SrNo { get; set; }
        public string? TxnDate { get; set; }
        public string? FlowType { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class CashflowSummary
    {
        public decimal TotalInflow { get; set; }
        public decimal TotalOutflow { get; set; }
        public decimal NetCashflow { get; set; }
    }

    public class CashflowGridResponse
    {
        public List<CashflowLineItem> LineItems { get; set; } = new();
        public CashflowSummary Summary { get; set; } = new();
    }
}
