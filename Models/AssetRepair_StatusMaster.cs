namespace ERPAdmin.Models
{
    public class GetAssetRepairStatusRequest
    {
        public long Id { get; set; }
    }
    public class SaveAssetRepairStatusRequest : BaseClass
    {
        public long Id { get; set; }

        public string? StatusTitle { get; set; }

        public string? StatusDescription { get; set; }
    }
    public class DeleteAssetRepairStatusRequest : BaseClass
    {
        public long Id { get; set; }
    }
    public class AssetRepairStatusModel
    {
        public long Id { get; set; }

        public string? StatusTitle { get; set; }

        public string? StatusDescription { get; set; }
    }
}
