namespace ERPAdmin.Models
{
    public class LeaveTypeMaster
    {
        public long Id { get; set; }
        public string? LeaveTypeName { get; set; }
        public bool? PaidFlag { get; set; }
        public int PriorityNo { get; set; }
    }
    public class GetLeaveTypeMasterRequest
    {
        public long Id { get; set; }
    }

    public class SaveLeaveTypeMasterRequest
    {
        public long Id { get; set; }
        public string? LeaveTypeName { get; set; }
        public bool? PaidFlag { get; set; }
        public int PriorityNo { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteLeaveTypeMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class LeaveTypeMasterModel
    {
        public long Id { get; set; }
        public string? LeaveTypeName { get; set; }
        public bool? PaidFlag { get; set; }
        public int PriorityNo { get; set; }
    }

}
