namespace ERPAdmin.Models
{
    public class StaffAttendance
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? AttendanceDate { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public long? StatusId { get; set; }
    }

    public class GetStaffAttendanceRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
    }

    public class SaveStaffAttendanceRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? AttendanceDate { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public long? StatusId { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteStaffAttendanceRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class StaffAttendanceModel
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? AttendanceDate { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public long? StatusId { get; set; }
        public string? StaffName { get; set; }
        public string? StatusName { get; set; }

    }

}
