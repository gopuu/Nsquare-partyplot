namespace ERPAdmin.Models
{
    #region WHATSAPP TEMPLATE LIST
    public class WhatsappTemplateModel_Request
    {
        public long EventId { get; set; }
    }

    public class WhatsappTemplateModel
    {
        public long EventId { get; set; }
        public string TempName_Title { get; set; }
    }
    #endregion

    #region WHATSAPP GRID REPORT
    public class WhatsAppMessageLogReportGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }

        public string? Status { get; set; }
        public long CompanyId { get; set; }
        public string? MobileNo { get; set; }
        public long? EventId { get; set; }
        public string? Search { get; set; }
    }
    public class WhatsAppMessageLogReportGridModel
    {
        public long Id { get; set; }
        public long CompanyId { get; set; }

        public string MobileNo { get; set; }
        public string MsgTemplate { get; set; }
        public string VariablesJson { get; set; }
        public string MsgSample { get; set; }

        public string Status { get; set; }

        public string ProviderMessageId { get; set; }
        public string ProviderResponse { get; set; }

        public string ErrorMessage { get; set; }

        public string ReferenceId { get; set; }
        public string MessageType { get; set; }
        public bool IsPromotional { get; set; }

        public decimal DeductedAmount { get; set; }
        public decimal RefundAmount { get; set; }

        public string CreatedAt { get; set; }
        public string SentAt { get; set; }
        public string FailedAt { get; set; }
        public string RefundedAt { get; set; }

        public string ActionByName { get; set; }

        public string TempName_Title { get; set; }
    }

    public class WhatsAppMessageLogReportSummaryModel
    {
        public int TotalRows { get; set; }
        public decimal TotalDeductedAmount { get; set; }
        public decimal TotalRefundAmount { get; set; }
    }
    #endregion

    #region WHATSAPP PDF/EXCEL REPORT
    public class WhatsAppMessageLogReportRequest : WhatsAppMessageLogReportGridRequest
    {
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }
    #endregion
}


