namespace ERPAdmin.Models
{

    //GRID request
    public class EnquiryDealsReportRequestGRID 
    {
        public string? EnquiryFromDate { get; set; }
        public string? EnquiryToDate { get; set; }
        public string? EnquiryMonthDuration { get; set; }

        public string? EventFromDate { get; set; }
        public string? EventToDate { get; set; }

        public string? NameEmailContactNo { get; set; }

        public long EventTypeId { get; set; }
        public long EventSubTypeId { get; set; }
        public long HallId { get; set; }

        public long? FromPax { get; set; }
        public long? ToPax { get; set; }

        public long LeadSourceId { get; set; }
        public long ProcessId { get; set; }

        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
    }
    //GRID response
    public class EnquiryDealsReportGridModel
    {
        public long EnquiryId { get; set; }

        public string EnquiryDate { get; set; }
        public string EventDate { get; set; }

        public string ClientName { get; set; }

        public string EventType_SubType { get; set; }

        public string Hall { get; set; }

        public int Pax { get; set; }

        public string LeadSource { get; set; }

        public decimal Amount { get; set; }

        public int FollowUps { get; set; }

        public string Status { get; set; }
    }

    //PDF request
    public class EnquiryDealsReportRequest : BaseClass
    {
        public bool? WithHeader { get; set; }
        public List<string>? Columns { get; set; }

        public string? EnquiryFromDate { get; set; }
        public string? EnquiryToDate { get; set; }
        public string? EnquiryMonthDuration { get; set; }

        public string? EventFromDate { get; set; }
        public string? EventToDate { get; set; }

        public string? NameEmailContactNo { get; set; }

        public long EventTypeId { get; set; }
        public long EventSubTypeId { get; set; }
        public long HallId { get; set; }

        public long? FromPax { get; set; }
        public long? ToPax { get; set; }

        public long LeadSourceId { get; set; }
        public long ProcessId { get; set; }

        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
    }

}
