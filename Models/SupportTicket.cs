namespace ERPAdmin.Models
{
    public class SupportTicket
    {
        public long Id { get; set; }

        public long TicketTypeId { get; set; }
        public long PriorityId { get; set; }
        public long StatusId { get; set; }

        public string? TicketNumber { get; set; }
        public string? TicketTypeName { get; set; }
        public string? PriorityName { get; set; }
        public string? StatusName   { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsClosed { get; set; }
        public string? CreatedAt { get; set; }

        public List<SupportTicketAttachment> AttachmentDetails { get; set; } = new();
    }

    // ================= GET =================
    public class GetSupportTicketRequest
    {
        public long Id { get; set; }       // 0 = all
        public long StatusId { get; set; } // optional filter
    }

    // ================= SAVE =================
    public class SaveSupportTicketRequest : BaseClass
    {
        public long Id { get; set; }
        public string? TicketNumber { get; set; }
        public long TicketTypeId { get; set; }
        public long PriorityId { get; set; }
        public long StatusId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsClosed { get; set; }
        public List<SupportTicketAttachmentDto> AttachmentDetails { get; set; }

    }

    public class SupportTicketAttachmentDto
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }

    // ================= DELETE =================
    public class DeleteSupportTicketRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }
}


