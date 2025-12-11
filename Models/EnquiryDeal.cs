namespace ERPAdmin.Models
{
    public class DeleteEnquiryDeal : BaseClass
    {
        public long Id { get; set; }
    }

    public class EnquiryDeal
    {
        public long Id { get; set; }
        public long ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public long EventTypeId { get; set; }
        public string? EventTypeName { get; set; }
        public long EventSubTypeId { get; set; }
        public string? EventSubTypeName { get; set; }
        public long BanquetHallId { get; set; }
        public string? BanquetHallName { get; set; }
        public string? EventDate { get; set; }
        public string? EventDate_Format { get; set; }
        public string? EventStartTime { get; set; }
        public string? EventEndTime { get; set; }
        public long Pax { get; set; }
        public string? FollowUpDate { get; set; }
        public string? FollowUpDate_Format { get; set; }
        public decimal EstimatedAmount { get; set; }
        public long LeadSourceId { get; set; }
        public string? LeadSourceName { get; set; }
        public string? Remarks { get; set; }
        public long FollowUpCount { get; set; }
    }
    public class SaveEnquiryDeal : BaseClass
    {
        public long Id { get; set; }
        public long ProcessId { get; set; }
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public long EventTypeId { get; set; }
        public long EventSubTypeId { get; set; }
        public long BanquetHallId { get; set; }
        public string? EventDate { get; set; }
        public string? EventStartTime { get; set; }
        public string? EventEndTime { get; set; }
        public long Pax { get; set; }
        public string? FollowUpDate { get; set; }
        public decimal EstimatedAmount { get; set; }
        public long LeadSourceId { get; set; }
        public string? Remarks { get; set; }
    }

    public class EnquiryDeal_forDragNDrop
    {
        public long ProcessId { get; set; }
        public string? Duration { get; set; }
        public decimal FromAmount { get; set; }
        public decimal ToAmount { get; set; }
    }

    public class SaveEnquiryDeal_forDragNDrop : BaseClass
    {
        public long EnquiryDealId { get; set; }
        public long ProcessId { get; set; }
        public string? FollowUpDate { get; set; }
        public string? Remarks { get; set; }
        public string? FileUpload { get; set; }
    }
    //for Drag n Drop
    public class EnquiryProcessDto
    {
        public long ProcessId { get; set; }
        public string? ProcessName { get; set; }
        public int TotalEnquiryDeals { get; set; }
        public decimal TotalAmount { get; set; }
        public List<dynamic> EnquiryDealsJson { get; set; } = new List<dynamic>();
    }

}
