namespace ERPAdmin.Models
{
    public class RevenueLossGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public long? EventTypeId { get; set; }
        public long? EventSubTypeId { get; set; }
        public long? LeadSourceId { get; set; }
    }

    public class RevenueLossReportRequest : BaseClass
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public long? EventTypeId { get; set; }
        public long? EventSubTypeId { get; set; }
        public long? LeadSourceId { get; set; }

        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
    }

    public class RevenueLossGridModel
    {
        public long SrNo { get; set; }
        public long EnquiryDealId { get; set; }
        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }
        public string? GuestName { get; set; }
        public string? EventType { get; set; }
        public string? EventSubType { get; set; }
        public string? EventDisplay { get; set; }
        public string? LeadSource { get; set; }
        public string? EnquiryDateTime { get; set; }
        public decimal LossAmount { get; set; }
    }
}
