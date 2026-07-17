namespace ERPAdmin.Models
{
    //GRID
    public class CashflowLineItem
    {
        public long SrNo { get; set; }
        public string TxnDate { get; set; }
        public string TxnDate_Format { get; set; }

        public string FlowType { get; set; }
        public string TxnType { get; set; }

        public string Description { get; set; }
        public string VoucherNo { get; set; }
        public string InvoiceNo { get; set; }

        public string SourceName { get; set; }
        public string ReceiverName { get; set; }
        public string PaymentModeName { get; set; }
        public string Notes { get; set; }

        public decimal Amount { get; set; }
    }

    public class CashflowReportRequest : BaseClass
    {
        public bool? WithHeader { get; set; }
        // 🔹 Date Filters
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }

        // 🔹 Transaction Filters
        public int TxnType { get; set; } = 0;
        // 0 = All, 1 = Income, 2 = Expense
        public int PaymentModeId { get; set; } = 0;
        public int AccountId { get; set; } = 0;

        // 🔹 Search Filters
        public string? VoucherNo_InvoiceNo { get; set; }
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
