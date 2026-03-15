namespace ERPAdmin.Models
{
    public class SupportTicketAssignment
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public long AssignedTo { get; set; }
        public long AssignedBy { get; set; }
        public string? AssignedOn { get; set; }
        public bool IsActive { get; set; }
    }
    public class GetSupportTicketAssignmentRequest
    {
        public long Id { get; set; } = 0;
        public long TicketId { get; set; } = 0;
    }

    public class SaveSupportTicketAssignmentRequest :BaseClass
    {
        public long Id { get; set; } = 0;
        public long TicketId { get; set; }
        public long AssignedTo { get; set; }
        public long AssignedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class DeleteSupportTicketAssignmentRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class SupportTicketAssignmentModel
    {
        public long Id { get; set; }
        public long TicketId { get; set; }
        public string? TicketDisplay { get; set; }
        public long AssignedTo { get; set; }
        public long AssignedBy { get; set; }
        public string? Title {get; set; }
        public string? AssignedToName { get; set; }
        public string? AssignedByName { get; set; }
        public string? TicketNumber { get; set; }
        public string? AssignedOn { get; set; }
        public bool IsActive { get; set; }
    }

}
