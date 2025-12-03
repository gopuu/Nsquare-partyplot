namespace ERPAdmin.Models
{
    public class LevelMaster
    {
        public long Id { get; set; }
        public string? LevelName { get; set; }
        public int PriorityNo { get; set; }
    }

    public class GetLevelMasterRequest
    {
        public long Id { get; set; }
    }

    public class SaveLevelMasterRequest
    {
        public long Id { get; set; }
        public string? LevelName { get; set; }
        public int PriorityNo { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteLevelMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class LevelMasterModel
    {
        public long Id { get; set; }
        public string? LevelName { get; set; }
        public int PriorityNo { get; set; }
    }

}
