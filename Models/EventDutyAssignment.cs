namespace ERPAdmin.Models
{
    public class EventDutyAssignment
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
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
        public long BookingId { get; set; }    // 0 = All
        public long StaffId { get; set; }    // 0 = All
    }

    public class SaveEventDutyAssignmentRequest
    {
        public long Id { get; set; }         // 0 = Insert, >0 = Update
        public long BookingId { get; set; }
        public long StaffId { get; set; }
        public long DutyRoleId { get; set; }

        public string? DutyStartDate { get; set; }
        public string? DutyEndDate { get; set; }

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
        public long BookingId { get; set; }
        public long StaffId { get; set; }
        public long DutyRoleId { get; set; }

        public string? DutyStartDate { get; set; }
        public string? DutyEndDate { get; set; }
        public string? DutyStartDate2 { get; set; }
        public string? DutyEndDate2 { get; set; }

        public string? DutyStartTime { get; set; }
        public string? DutyEndTime { get; set; }
        public string? DutyStartTime2 { get; set; }
        public string? DutyEndTime2 { get; set; }

        public string? Remarks { get; set; }
        public string? StaffName { get; set; }
        public string? RoleName { get; set; }
        public string? StatusName { get; set; }
        public string? EventName { get; set; }
        public long? StatusId { get; set; }
    }

}
