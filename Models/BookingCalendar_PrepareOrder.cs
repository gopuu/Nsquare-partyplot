namespace ERPAdmin.Models
{
    //SAVE
    public class SavePrepareOrderRequest : BaseClass
    {
        public long Id { get; set; } = 0;
        public long BookingId { get; set; }
        public long EnquiryDealId { get; set; }
        public int PAX { get; set; }
        public string? PONumber { get; set; }
        public string? PODate { get; set; }       // dd-MM-yyyy
        public string? EventDate { get; set; }    // dd-MM-yyyy
        public long SlotId { get; set; }
        public string? Notes { get; set; }

        public List<PrepareOrderFoodItemDto>? FoodItems { get; set; } = new();
    }
    public class PrepareOrderFoodItemDto
    {
        public long FoodItemId { get; set; }
        public bool IsAdded { get; set; } = true;

        public FoodFormulaDto? FoodFormula { get; set; }
    }
    public class FoodFormulaDto
    {
        public int PeopleCount { get; set; } = 1;
        public bool IsFormulaRequired { get; set; } = true;

        public List<FoodFormulaIngredientDto>? Ingredients { get; set; } = new();
    }
    public class FoodFormulaIngredientDto
    {
        public string? IngredientName { get; set; }
        public decimal Quantity { get; set; }
        public long UnitMasterId { get; set; }
    }

    //GET
    public class GetPrepareOrderRequest
    {
        public long BookingId { get; set; }
        public long PrepareOrderId { get; set; }
    }
    public class PrepareOrderResponseModel
    {
        public long Id { get; set; }
        public long BookingId { get; set; }

        public long EnquiryDealId { get; set; }
        public string? EnquiryDealName { get; set; }

        public int PAX { get; set; }
        public string? PONumber { get; set; }
        public string? PODate { get; set; }
        public string? EventDate { get; set; }
        public long SlotId { get; set; }
        public string? Notes { get; set; }

        public List<PrepareOrderFoodItemModel> FoodItems { get; set; } = new();
    }
    public class PrepareOrderFoodItemModel
    {
        public long PrepareOrderFoodItemId { get; set; }
        public long PrepareOrderId { get; set; }
        public long FoodItemId { get; set; }
        public string? FoodItemName { get; set; }
        public long HeaderId { get; set; }
        public string? HeaderName { get; set; }
        public bool IsAdded { get; set; }

        public List<FoodFormulaModel>? FoodFormula { get; set; }
    }
    public class FoodFormulaModel
    {
        public long Id { get; set; }
        public int PeopleCount { get; set; }
        public bool IsFormulaRequired { get; set; }

        public List<FoodFormulaIngredientModel>? Ingredients { get; set; }
    }
    public class FoodFormulaIngredientModel
    {
        public long Id { get; set; }
        public string? IngredientName { get; set; }
        public decimal Quantity { get; set; }
        public long UnitMaster_Id { get; set; }
    }

    //DELETE
    public class DeletePrepareOrderRequest
    {
        public long PrepareOrderId { get; set; }
        public long Action_By { get; set; }
    }

}
