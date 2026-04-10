namespace ERPAdmin.Models
{
    #region DTO Models (Request / Response)
    public class WalletRequest_Balance 
    {
        public long CompanyId { get; set; }
    }
    public class WalletRequest_Create : BaseClass
    {
        public long CompanyId { get; set; }
    }
    public class WalletRequest_Ledger //: BaseClass
    {
        public long CompanyId { get; set; }
    }
    public class WalletResponse_Ledger //: BaseClass
    {
        public long Id { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string ReferenceType { get; set; }
        public string Description { get; set; }
        public string CreatedAt { get; set; }
    }

    public class WalletCreditRequest : BaseClass
    {
        public long CompanyId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceType { get; set; } // RAZORPAY
        public long? ReferenceId { get; set; }
    }

    public class WalletDebitRequest: BaseClass
    {
        public long CompanyId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceType { get; set; } // WHATSAPP / SMS
        public long? ReferenceId { get; set; }
    }
    #endregion

    #region RECHARGE
    public class CreateOrderRequest
    {
        public long CompanyId { get; set; }
        public decimal Amount { get; set; } // in INR
    }

    public class VerifyPaymentRequest : BaseClass
    {
        public long CompanyId { get; set; }
        public decimal Amount { get; set; }

        public string RazorpayOrderId { get; set; }
        public string RazorpayPaymentId { get; set; }
        public string RazorpaySignature { get; set; }
    }
    #endregion
}
