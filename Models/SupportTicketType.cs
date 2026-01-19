namespace ERPAdmin.Models
{
    public class SupportTicketType
    {
        public long Id { get; set; } = 0; // 0 for insert
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public long ActionBy { get; set; }
    }

    public class GetSupportTicketTypeRequest
    {
        public long Id { get; set; } = 0; // 0 for all
    }

    public class SaveSupportTicketTypeRequest : BaseClass
    {
        public long Id { get; set; } = 0; // 0 for insert
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class DeleteSupportTicketTypeRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class SupportTicketTypeModel
    {
        public long Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
    }

}
