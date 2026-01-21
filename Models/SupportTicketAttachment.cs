namespace ERPAdmin.Models
{
    public class SupportTicketAttachment
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string? FileName { get; set; }

        public string? FilePath { get; set; }
        public string? TicketDisplay { get; set; }
        public long ActionBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GetSupportTicketAttachmentRequest
    {
        public long TicketId { get; set; }
    }

    public class SaveSupportTicketAttachmentRequest : BaseClass
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
    }

    public class DeleteSupportTicketAttachmentRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

}
