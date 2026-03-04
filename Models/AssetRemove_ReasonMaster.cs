namespace ERPAdmin.Models
{

    //Get
    public class AssetRemoveReasonModel
    {
        public long Id { get; set; }

        public string? ReasonTitle { get; set; }

        public string? ReasonDescription { get; set; }
    }
    public class GetAssetRemoveReasonRequest
    {
        public long Id { get; set; }
    }
    //Save
    public class SaveAssetRemoveReasonRequest : BaseClass
    {
        public long Id { get; set; }

        public string? ReasonTitle { get; set; }

        public string? ReasonDescription { get; set; }
    }
    //Delete
    public class DeleteAssetRemoveReasonRequest : BaseClass
    {
        public long Id { get; set; }

    }
}
