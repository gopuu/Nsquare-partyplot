namespace ERPAdmin.Models
{
    public class DesignationMaster
    {
        public long Id { get; set; }
        public string? DesignationName { get; set; }
        public long LevelId { get; set; }
    }

    public class GetDesignationMasterRequest
    {
        public long Id { get; set; }
    }

    public class SaveDesignationMasterRequest
    {
        public long Id { get; set; }
        public string? DesignationName { get; set; }
        public long LevelId { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteDesignationMasterRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class DesignationMasterModel
    {
        public long Id { get; set; }
        public string? DesignationName { get; set; }
        public long LevelId { get; set; }
        public string? LevelName { get; set; }

    }

}
