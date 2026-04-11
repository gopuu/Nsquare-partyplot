namespace ERPAdmin.Models
{
    public class WhatsAppRequest
    {
        public int CompanyId { get; set; }
        public string Mobile { get; set; }
        public string TemplateName { get; set; }
        public bool IsPromotional { get; set; }
        public List<string> MessageParams { get; set; } // For template parameters
        public string RawMessage { get; set; } // Alternative: raw message with & separator
        public string LanguageCode { get; set; }
        public string UserId { get; set; }
        public string ReferenceId { get; set; }
        public string MessageType { get; set; } = "WHATSAPP";
        public int MessageCount { get; set; } = 1;
    }

    public class WhatsAppResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public decimal Balance { get; set; }
        public decimal Amount { get; set; }
        public string TransactionType { get; set; }
    }
}
