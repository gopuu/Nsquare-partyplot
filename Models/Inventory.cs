namespace ERPAdmin.Models
{
    //SAVE
    public class SaveStockOutRequest : BaseClass
    {
        public List<SaveStockOutModel>? StockOutList { get; set; }
    }

    // Per Row Model
    public class SaveStockOutModel
    {
        public long RawMaterialId { get; set; }
        public decimal Qty { get; set; }
        public long UnitId { get; set; }
        public string? Remarks { get; set; }
    }

    //GET
    public class GetStockListModel
    {
        public long RawMaterialId { get; set; }
        public string? RawMaterialName { get; set; }
        public long ItemCategoryId { get; set; }
        public string? ItemCategoryName { get; set; }
        public long UnitId { get; set; }
        public string? UnitName { get; set; }
        public decimal CurrentQty { get; set; }
    }

    public class GetStockListRequest
    {
        public long? ItemCategoryId { get; set; }
        public string? ItemCategoryName { get; set; }
        public long? RawMaterialId { get; set; }
        public string? RawMaterialName { get; set; }
        public long? UnitId { get; set; }
        public string? UnitName { get; set; }

    }

}
