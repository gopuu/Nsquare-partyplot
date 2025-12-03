namespace ERPAdmin.Models
{
    public class StaffLeave
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public long LeaveTypeId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Reason { get; set; }
        public long? StatusId { get; set; }
    }

    // StaffLeave Models

    public class GetStaffLeaveRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
    }

    public class SaveStaffLeaveRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public long LeaveTypeId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Reason { get; set; }
        public long? StatusId { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteStaffLeaveRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class StaffLeaveModel
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public long LeaveTypeId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
        public string? Reason { get; set; }
        public long? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? StaffName { get; set; }

        public string? LeaveTypeName { get; set; }

    }

}
