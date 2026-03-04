namespace ERPAdmin.Models
{
    //Get response
    public class AssetPurchaseModel
    {
        public long Id { get; set; }
        public string? PurchaseDate { get; set; }
        public string? PurchaseDate1 { get; set; }
        public string? VendorName { get; set; }
        public string? InvoiceNo { get; set; }
        public string? Remarks { get; set; }
        public decimal NetTotalAmount { get; set; }

        public List<AssetPurchaseItemModel>? Items { get; set; }
    }
    public class AssetPurchaseItemModel
    {
        public long Id { get; set; }
        public long AssetMaster_Id { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal GrossTotalAmount { get; set; }
    }
    //Get request
    public class GetAssetPurchaseRequest
    {
        public long Id { get; set; }
    }
    //Save
    public class SaveAssetPurchaseRequest : BaseClass
    {
        public long Id { get; set; }

        public string? PurchaseDate { get; set; }
        public string? VendorName { get; set; }
        public string? InvoiceNo { get; set; }
        public string? Remarks { get; set; }
        public decimal NetTotalAmount { get; set; }

        public List<AssetPurchaseItemModel>? Items { get; set; }

    }
    //Delete
    public class DeleteAssetPurchaseRequest : BaseClass
    {
        public long Id { get; set; }
    }
}
