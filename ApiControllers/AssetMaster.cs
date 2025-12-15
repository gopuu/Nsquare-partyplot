namespace ERPAdmin.ApiControllers
{
    public class GetAssetRequest
    {
        public long? Id { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class GetAssetResponse
    {
        public long Id { get; set; }
        public string AssetName { get; set; }
        public string AssetUniqueCode { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public long? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal ClosingQty { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveAssetRequest
    {
        public long Id { get; set; }
        public string AssetName { get; set; }
        public string AssetUniqueCode { get; set; }
        public long CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal ClosingQty { get; set; }
        public bool IsActive { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteAssetRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

}
