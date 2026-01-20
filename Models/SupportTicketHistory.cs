namespace ERPAdmin.Models
{
    public class SupportTicketHistory
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public long ActionBy { get; set; }
        public DateTime ActionDate
        {
            get; set;
        }
    }

    public class GetSupportTicketHistoryRequest
    {
        public long TicketId { get; set; }
    }

    public class SaveSupportTicketHistoryRequest : BaseClass
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    public class DeleteSupportTicketHistoryRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

}
