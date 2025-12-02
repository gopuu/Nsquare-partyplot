namespace ERPAdmin.Models
{
    //Finance Audit Model
    public class AccountBalanceAuditModel
    {
        public long Id { get; set; }

        public long AccountId { get; set; }
        public string? AccountName { get; set; }

        public decimal PreviousBalance { get; set; }
        public decimal AmountChanged { get; set; }
        public decimal NewBalance { get; set; }

        public string? ActionType { get; set; }   // FundTransfer-Debit, Expense, Income, etc.
        public long? ReferenceId { get; set; }

        public string? Created_At { get; set; }
        public long? Created_By { get; set; }
        public string? Created_By_Name { get; set; }
    }
    public class GetAccountBalanceAuditRequest
    {
        public long Id { get; set; } = 0;
        public long AccountId { get; set; } = 0;
        public string ActionType { get; set; } = null;
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

}
