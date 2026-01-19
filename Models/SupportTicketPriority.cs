namespace ERPAdmin.Models
{
    public class SupportTicketPriority
    {
        public long Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long SortOrder { get; set; }
    }

    public class GetSupportTicketPriorityRequest
    {
        public long Id { get; set; } = 0;
    }

    public class SaveSupportTicketPriorityRequest :BaseClass
    {
        public long Id { get; set; } = 0;
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long SortOrder { get; set; }
    }

    public class DeleteSupportTicketPriorityRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class SupportTicketPriorityModel
    {
        public long Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public long SortOrder { get; set; }
    }

}
