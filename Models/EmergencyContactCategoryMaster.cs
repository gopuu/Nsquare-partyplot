namespace ERPAdmin.Models
{
    public class GetEmergencyContactCategoryRequest
    {
        public long? Id { get; set; }
    }
    public class GetEmergencyContactCategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }

    public class SaveEmergencyContactCategoryRequest : BaseClass
    {
        public long? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
    }
    public class DeleteEmergencyContactCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
