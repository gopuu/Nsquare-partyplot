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
        public string? TransactionDate { get; set; }
        public long? PaymentModeId { get; set; }
        public string? PaymentModeName { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public string? VoucherNo { get; set; }
        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }
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
        public string? TransactionDate { get; set; }
        public long? PaymentModeId { get; set; }
        public string? TransactionId { get; set; }
        public string? Notes { get; set; }
        public string? VoucherNo { get; set; }
        public string? ChequeBank { get; set; }
        public string? ChequeNo { get; set; }
        public string? ChequeDate { get; set; }
    }

    public class DeleteIncomeTransactionRequest : BaseClass
    { 
        public long Id { get; set; }
    }

    //NEXT VOUCHER NO
    public class NextVoucherNoResponse
    {
        public string LastVoucherNo_Income { get; set; }
        public string LastVoucherNo_Expense { get; set; }
    }

}
