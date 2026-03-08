namespace ERPAdmin.Models
{
    //GET
    public class GetRawMaterialsRequest
    {
        public long Id { get; set; }
        public long ItemCategoryId { get; set; }
    }
    public class GetRawMaterialsList
    {
        public long Id { get; set; }
        public string? Name { get; set; }

        public long ConsumptionUnitId { get; set; }
        public decimal? OnePurchaseUnitEquals_ConsumptionUnitQty { get; set; }

        public long ItemCategoryId { get; set; }
        public string? ItemCategoryName { get; set; }

        public decimal? PurchasePrice { get; set; }
        public decimal? TransferPrice { get; set; }
        public decimal? ReconciliationPrice { get; set; }

        public long? TaxTypeId { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal? ExciseDutyAmount { get; set; }

        public long? MinStockUnitId { get; set; }
        public decimal? MinStockLevel { get; set; }
        public long? AtParStockUnitId { get; set; }
        public decimal? AtParStockLevel { get; set; }

        public bool AllowReStockLevel { get; set; }

        public string? Barcode { get; set; }
        public string? HSNCode { get; set; }
        public string? GTIN { get; set; }
        public string? Brand { get; set; }

        public bool ExclusiveToRestaurant { get; set; }
        public bool IsExpiry { get; set; }
        public bool AllowDecimalQty { get; set; }
        public bool IsActive { get; set; }
        public bool IsFavourite { get; set; }

        public string? Description { get; set; }
        public decimal? NormalLossPercent { get; set; }
        public decimal? QuantityInGmMl { get; set; }
        public decimal? CurrentQty { get; set; }

        public List<RM_PurchaseUnit> JSON_PurchaseUnits { get; set; }
        public List<RM_ClosingStockUpdatedOn> JSON_ClosingStockUpdatedOn { get; set; }
        public List<RM_RestockLevel> JSON_RestockLevel { get; set; }
    }

    public class RM_PurchaseUnit
    {
        public long UnitId { get; set; }
        public decimal? ConversionValue { get; set; }
    }

    public class RM_ClosingStockUpdatedOn
    {
        public long FrequencyTitleId { get; set; }
    }

    public class RM_RestockLevel
    {
        public decimal RestockQty { get; set; }
        public long RestockUnitId { get; set; }
    }

    // SAVE MODEL FOR RAW MATERIALS
    public class SaveRawMaterials : BaseClass
    {
        public long Id { get; set; }
        public string? Name { get; set; }

        // JSON ARRAYS
        public List<PurchaseUnitModel>? JSON_PurchaseUnits { get; set; }
        // Units
        public long ConsumptionUnitId { get; set; }

        public long ItemCategoryId { get; set; }

        public decimal? CurrentQty { get; set; }

        // Pricing
        public decimal? PurchasePrice { get; set; }
        public decimal? TransferPrice { get; set; }
        public decimal? ReconciliationPrice { get; set; }

        // Taxes
        public long? TaxTypeId { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal? ExciseDutyAmount { get; set; }


        // Stock Levels
        public long? MinStockUnitId { get; set; }
        public decimal? MinStockLevel { get; set; }
        public long? AtParStockUnitId { get; set; }
        public decimal? AtParStockLevel { get; set; }

        public List<ClosingStockUpdatedOnModel>? JSON_ClosingStockUpdatedOn { get; set; }

        public bool AllowReStockLevel { get; set; }
        public List<RestockLevelModel>? JSON_RestockLevel { get; set; }

        // Identification
        public string? Barcode { get; set; }
        public string? HSNCode { get; set; }
        public bool ExclusiveToRestaurant { get; set; }
        public bool IsExpiry { get; set; }

        public bool AllowDecimalQty { get; set; }

        public string? Description { get; set; }
        // Extra Info
        public decimal? NormalLossPercent { get; set; }
        public decimal? QuantityInGmMl { get; set; }

        public string? GTIN { get; set; }
        public string? Brand { get; set; }

        // Flags
        public bool IsActive { get; set; }

    }
    public class PurchaseUnitModel
    {
        public long UnitId { get; set; }
        public decimal? ConversionValue { get; set; }
    }

    public class ClosingStockUpdatedOnModel
    {
        public long FrequencyTitleId { get; set; }
    }

    public class RestockLevelModel
    {
        public decimal RestockQty { get; set; }
        public long RestockUnitId { get; set; }
    }


    //DELETE
    public class DeleteRawMaterialsRequest : BaseClass
    {
        public long Id { get; set; }
    }

    //Save Is favourite
    public class SaveFavouriteModel : BaseClass
    {
        public long Id { get; set; }
        public bool IsFavourite { get; set; }
    }

    //Save IsActive
    public class SaveActiveModel : BaseClass
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
    }
    //Get Category Summary
    public class GetCategorySummaryRequest
    {
        public long CategoryId { get; set; } = 0;   // 0 = fetch all
    }

    public class RawMaterialCategorySummary
    {
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public long RawMaterialsCount { get; set; }
    }
    //Stock Out History
    public class GetStockOutHistoryRequest
    {
        public long RawMaterialId { get; set; } = 0;
    }
    public class GetStockOutHistoryList
    {
        public long Id { get; set; }
        public long RawMaterialId { get; set; }
        public string? RawMaterialName { get; set; }

        public long UnitId { get; set; }
        public string? UnitName { get; set; }

        public decimal Quantity { get; set; }
        public string? Remarks { get; set; }

        public string? ModifiedOn { get; set; }
        public string? ModifiedOn2 { get; set; }

        public string? ModifiedBy { get; set; }
    }
}
