namespace ERPAdmin.Models
{
    public class ExpenseReportGridRequest
    {
        public long CategoryId { get; set; } = 0;
        public long SubCategoryId { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class ExpenseReportRequest
    {
        public long CategoryId { get; set; } = 0;
        public long SubCategoryId { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long PaymentModeId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class ExpenseReportGridModel
    {
        public long SrNo { get; set; }
        public long TransactionId { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountName { get; set; }
        public string? CategoryName { get; set; }
        public long CategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public long SubCategoryId { get; set; }
        public string? PaymentModeName { get; set; }
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }
}
