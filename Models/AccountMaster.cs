namespace ERPAdmin.Models
{

    public class AccountMasterModel
    {
        public long Id { get; set; }
        public string? AccountName { get; set; }
        public string? AccountType { get; set; }

        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }

        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? IFSC { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? UPI_Id { get; set; }

        public string? Description { get; set; }
        public bool IsDefaultAccount { get; set; }
    }

    public class GetAccountRequest : BaseClass
    {
        public long Id { get; set; } = 0;
    }

    public class SaveAccountRequest : BaseClass
    {
        public long Id { get; set; }
        public string? AccountName { get; set; }
        public string? AccountType { get; set; }

        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }

        public string? BankName { get; set; }
        public string? BranchName { get; set; }
        public string? IFSC { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolderName { get; set; }
        public string? UPI_Id { get; set; }

        public string? Description { get; set; }
        public bool IsDefaultAccount { get; set; }
    }

    public class DeleteAccountRequest : BaseClass
    {
        public long Id { get; set; }
    }


}
