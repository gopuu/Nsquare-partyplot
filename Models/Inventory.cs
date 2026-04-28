namespace ERPAdmin.Models
{
    //SAVE
    public class SaveStockInOutRequest : BaseClass
    {
        public List<SaveStockInOutModel>? StockInOutList { get; set; }
    }

    // Per Row Model
    public class SaveStockInOutModel
    {
        public long RawMaterialId { get; set; }
        public decimal Qty_In { get; set; }
        public decimal Qty_Out { get; set; }
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
        public long? RawMaterialId { get; set; }
        public long? UnitId { get; set; }
    }

    #region REPORT
    public class StockReportRequest : BaseClass
    {
        public List<string>? Columns { get; set; }
        public long ItemCategoryId { get; set; }
        public long RawMaterialId { get; set; }
        public long UnitId { get; set; }
        public bool? WithHeader { get; set; }
    }

    #endregion

}
