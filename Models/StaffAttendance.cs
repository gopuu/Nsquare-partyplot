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
        public long? Id { get; set; }
        public long? StaffId { get; set; }
        public long? StatusId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    public class SaveStaffAttendanceRequest : BaseClass
    {
        public List<StaffAttendanceItem> AttendanceList { get; set; }
    }
    public class StaffAttendanceItem
    {
        public long? Id { get; set; }
        public long StaffId { get; set; }
        public string? AttendanceDate { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public long? StatusId { get; set; }
        
    }
    public class DeleteStaffAttendanceRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class StaffAttendanceModel
    {
        public long AttendanceId { get; set; }
        public long StaffId { get; set; }
        public string? AttendanceDate { get; set; }
        public string? CheckInTime { get; set; }
        public string? CheckOutTime { get; set; }
        public long StatusId { get; set; }
        public string? StaffName { get; set; }
        public string? StatusName { get; set; }
        public string? AttendanceDateFormat { get; set; }
        public string? DayName { get; set; }

    }

}
