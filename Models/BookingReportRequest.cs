namespace ERPAdmin.Models
{
    public class BookingReportRequest
    {
        public long EventTypeId { get; set; } = 0;
        public long EventSubTypeId { get; set; } = 0;
        public long HallId { get; set; } = 0;
        public string? EventDate { get; set; }
    }
}
