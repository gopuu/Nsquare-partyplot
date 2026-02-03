namespace ERPAdmin.Models
{
    //GET
    public class GetEmergencyContactSubCategoryRequest
    {
        public long? Id { get; set; }
        public long? CategoryId { get; set; }
    }
    public class GetEmergencyContactSubCategoryResponse
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string? Description { get; set; }
    }

    //SAVE
    public class SaveEmergencyContactSubCategoryRequest : BaseClass // contains ActionBy
    {
        public long? Id { get; set; }
        public long CategoryId { get; set; }   // REQUIRED
        public string SubCategoryName { get; set; }
        public string? Description { get; set; }
    }


    //DELETE
    public class DeleteEmergencyContactSubCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
