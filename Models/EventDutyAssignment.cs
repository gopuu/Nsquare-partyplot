namespace ERPAdmin.Models
{
    public class EventDutyAssignment
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long StaffId { get; set; }
        public long DutyRoleId { get; set; }

        public string? DutyStartTime { get; set; }
        public string? DutyEndTime { get; set; }

        public string? Remarks { get; set; }
        public long? StatusId { get; set; }
    }

    public class GetEventDutyAssignmentRequest
    {
        public long Id { get; set; }         // 0 = All
        public long EventId { get; set; }    // 0 = All
        public long StaffId { get; set; }    // 0 = All
    }

    public class SaveEventDutyAssignmentRequest
    {
        public long Id { get; set; }         // 0 = Insert, >0 = Update
        public long EventId { get; set; }
        public long StaffId { get; set; }
        public long DutyRoleId { get; set; }

        public string? DutyStartTime { get; set; }
        public string? DutyEndTime { get; set; }

        public string? Remarks { get; set; }
        public long? StatusId { get; set; }

        public long ActionBy { get; set; }
    }

    public class DeleteEventDutyAssignmentRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class EventDutyAssignmentModel
    {
        public long Id { get; set; }
        public long EventId { get; set; }
        public long StaffId { get; set; }
        public long DutyRoleId { get; set; }

        public string? DutyStartTime { get; set; }
        public string? DutyEndTime { get; set; }

        public string? Remarks { get; set; }
        public long? StatusId { get; set; }
    }

}
