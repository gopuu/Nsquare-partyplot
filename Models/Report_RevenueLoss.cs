namespace ERPAdmin.Models
{
    public class RevenueLossGridRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
    }

    public class RevenueLossReportRequest
    {
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? FilterDuration { get; set; }
        public List<string>? Columns { get; set; }
        public bool? WithHeader { get; set; }
        public long ActionBy { get; set; }
    }

    public class RevenueLossGridModel
    {
        public long SrNo { get; set; }
        public long EnquiryDealId { get; set; }
        public string? EventDate { get; set; }
        public string? GuestName { get; set; }
        public string? EventType { get; set; }
        public string? EventSubType { get; set; }
        public string? EventDisplay { get; set; }
        public decimal EstimatedAmount { get; set; }
    }
}
