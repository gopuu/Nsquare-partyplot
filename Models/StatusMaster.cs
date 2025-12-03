namespace ERPAdmin.Models
{
    public class StatusMaster
    {
        public long Id { get; set; }
        public string? StatusType { get; set; }
        public string? StatusName { get; set; }
        public int PriorityNo { get; set; }
    }

    // StatusMaster Models

    public class GetStatusMasterRequest
    {
        public long Id { get; set; }
    }

    public class SaveStatusMasterRequest
    {
        public long Id { get; set; }
        public string? StatusType { get; set; }
        public string? StatusName { get; set; }
        public int PriorityNo { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteStatusMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class StatusMasterModel
    {
        public long Id { get; set; }
        public string? StatusType { get; set; }
        public string? StatusName { get; set; }
        public int PriorityNo { get; set; }
    }

}
