namespace ERPAdmin.Models
{
    public class ExpenseTransactionModel
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string? AccountName { get; set; }

        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public long? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }

        public decimal Amount { get; set; }
        public DateOnly TransactionDate { get; set; }
        public string? PaymentMode { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }
    }
    public class GetExpenseTransactionRequest
    {
        public long Id { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long CategoryId { get; set; } = 0;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    public class SaveExpenseTransactionRequest : BaseClass
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long CategoryId { get; set; }
        public long? SubCategoryId { get; set; }

        public decimal Amount { get; set; }
        public string? TransactionDate { get; set; }
        public string? PaymentMode { get; set; }
        public string? ReferenceNo { get; set; }
        public string? Notes { get; set; }

    }

    public class DeleteExpenseTransactionRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
