namespace ERPAdmin.Models
{
    public class Company_FoodItemClass
    {
    }
    public class CompanyFoodItem
    {
        public long Id { get; set; }
        public string HeaderName { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
    }
    public class GetCompanyFoodItemsRequest
    {
    }
    public class GetCompanyFoodItemsRequestById
    {
        public long? Id { get; set; }
    }
    public class DeleteCompanyFoodItemRequest
    {
        public long Id { get; set; }
        public long DeletedBy { get; set; }
    }
}
