namespace ERPAdmin.Models
{
    public class SupportTicketStatus
    {
        public long Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool IsFinal { get; set; }
        public bool IsActive { get; set; }
    }
    public class GetSupportTicketStatusRequest
    {
        public long Id { get; set; } = 0;
    }

    public class SaveSupportTicketStatusRequest : BaseClass
    {
        public long Id { get; set; } = 0;
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool IsFinal { get; set; }
        public bool IsActive { get; set; }
    }

    public class DeleteSupportTicketStatusRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class SupportTicketStatusModel
    {
        public long Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool IsFinal { get; set; }
        public bool IsActive { get; set; }
    }

}
