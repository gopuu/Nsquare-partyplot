namespace ERPAdmin.Models
{

    public class AssetRepairModel
    {
        public long Id { get; set; }

        public long? BookingId { get; set; }

        public string? RepairVendor { get; set; }

        public decimal RepairTotalCost { get; set; }

        public string? Remarks { get; set; }

        public List<AssetRepairItemModel>? Items { get; set; }
    }
    public class AssetRepairItemModel
    {
        public long Id { get; set; }

        public long AssetRepair_Id { get; set; }

        public long AssetMaster_Id { get; set; }

        public decimal CostPerItem { get; set; }

        public decimal Qty { get; set; }

        public decimal RepairCost { get; set; }

        public long? AssetRepair_StatusMaster_Id { get; set; }

        public string? SentDate { get; set; }

        public string? ReceivedDate { get; set; }
    }
    public class GetAssetRepairRequest
    {
        public long Id { get; set; }
    }
    //Save
    public class SaveAssetRepairRequest : BaseClass
    {
        public long Id { get; set; }

        public long? BookingId { get; set; }

        public string? RepairVendor { get; set; }

        public decimal RepairTotalCost { get; set; }

        public string? Remarks { get; set; }

        public List<AssetRepairItemModel> Items { get; set; }

    }
    //Delete
    public class DeleteAssetRepairRequest : BaseClass
    {
        public long Id { get; set; }

    }
}
