using Newtonsoft.Json;

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
        public string ReferenceId { get; set; }
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

    public class WalletResponse
    {
        public int Status { get; set; } // 1 for Success, 0 for Fail
        public string Message { get; set; }
        public decimal Balance { get; set; }
        public decimal DeductedAmount { get; set; }
        public decimal RefundedAmount { get; set; }
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

    #region WHATSAPP
    public class WhatsAppSendRequest
    {

        public string MsgTemplate { get; set; }
        public string MsgSample { get; set; }

        public long CompanyId { get; set; }
        public string MobileNo { get; set; }
        public string MessageType { get; set; }   // TEXT / TEMPLATE
        public int MessageCount { get; set; } = 1;
        public string? ReferenceId { get; set; }   // UNIQUE (GUID recommended)
        public long? TemplateId { get; set; }
        public bool IsPromotional { get; set; }
        public long ActionBy { get; set; }

        // Template Data
        public string TemplateName { get; set; }
        public string Message { get; set; } // param1&param2 format
    }
    public class WhatsAppApiResponse_OLD
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class WhatsAppApiResponse
    {
        [JsonProperty("messaging_product")]
        public string MessagingProduct { get; set; }


        [JsonProperty("contacts")]
        public List<WhatsAppContact> Contacts { get; set; }


        [JsonProperty("messages")]
        public List<WhatsAppMessage> Messages { get; set; }


        [JsonProperty("success")]
        public bool Success { get; set; }


        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class WhatsAppContact
    {
        [JsonProperty("input")]
        public string Input { get; set; }


        [JsonProperty("wa_id")]
        public string WaId { get; set; }
    }

    public class WhatsAppMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
    #endregion

}
