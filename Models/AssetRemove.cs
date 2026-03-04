namespace ERPAdmin.Models
{
    //Get response
    public class AssetRemoveModel
    {
        public long Id { get; set; }

        public long? BookingId { get; set; }

        public string? RemovedDate { get; set; }

        public string Remarks { get; set; }

        public List<AssetRemoveItemModel> Items { get; set; }
    }
    public class AssetRemoveItemModel
    {
        public long Id { get; set; }

        public long AssetRemove_Id { get; set; }

        public long AssetMaster_Id { get; set; }

        public decimal Qty { get; set; }

        public long? AssetRemove_ReasonMaster_Id { get; set; }

    }
    //Get request
    public class GetAssetRemoveRequest
    {
        public long Id { get; set; }
    }

    //Save
    public class SaveAssetRemoveRequest
    {
        public long Id { get; set; }

        public long? BookingId { get; set; }

        public string? RemovedDate { get; set; }

        public string Remarks { get; set; }

        public List<AssetRemoveItemModel> Items { get; set; }

        public long ActionBy { get; set; }
    }

    //Delete
    public class DeleteAssetRemoveRequest
    {
        public long Id { get; set; }

        public long ActionBy { get; set; }
    }

}
