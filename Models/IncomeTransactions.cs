namespace ERPAdmin.Models
{
    public class IncomeTransactionModel
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string AccountName { get; set; }

        public long CategoryId { get; set; }
        public string CategoryName { get; set; }

        public long? SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }

        public decimal Amount { get; set; }
        public DateOnly TransactionDate { get; set; }    // or DateTime if you prefer
        public string PaymentMode { get; set; }
        public string ReferenceNo { get; set; }
        public string Notes { get; set; }

        public string Created_At { get; set; }
        public long? Created_By { get; set; }
        public string Updated_At { get; set; }
        public long? Updated_By { get; set; }
    }

    public class GetIncomeTransactionRequest
    {
        public long Id { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public long CategoryId { get; set; } = 0;
        public string? FromDate { get; set; } = null;
        public string? ToDate { get; set; } = null;
    }

    public class SaveIncomeTransactionRequest : BaseClass
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public long CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public decimal Amount { get; set; }
        public string TransactionDate { get; set; }
        public string PaymentMode { get; set; }
        public string ReferenceNo { get; set; }
        public string Notes { get; set; }
    }

    public class DeleteIncomeTransactionRequest : BaseClass
    { 
        public long Id { get; set; }
    }

}
