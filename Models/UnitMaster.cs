namespace ERPAdmin.Models
{
    public class GetUnitMasterRequest
    {
        public long Id { get; set; }
    }

    public class GetUnitMaster
    {
        public long Id { get; set; }
        public string? UnitName { get; set; }
        public string? ShortName { get; set; }
        public bool IsActive { get; set; }
    }
    public class SaveUnitMaster : BaseClass
    {
        public long Id { get; set; }
        public string? UnitName { get; set; }
        public string? ShortName { get; set; }
        public bool IsActive { get; set; }
    }
    public class DeleteUnitMaster : BaseClass
    {
        public long Id { get; set; }
    }

}
