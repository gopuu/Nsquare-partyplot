namespace ERPAdmin.Models
{
    public class DutyRoleMaster
    {
        public long Id { get; set; }   // 0 = Insert, >0 = Update
        public string? RoleName { get; set; }
        public int PriorityNo { get; set; }
    }

    public class GetDutyRoleMasterRequest
    {
        public long Id { get; set; }   // 0 = Fetch All
    }

    public class SaveDutyRoleMasterRequest
    {
        public long Id { get; set; }   // 0 = Insert, >0 = Update
        public string? RoleName { get; set; }
        public int PriorityNo { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteDutyRoleMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class DutyRoleMasterModel
    {
        public long Id { get; set; }
        public string RoleName { get; set; }
        public int PriorityNo { get; set; }
    }
}
