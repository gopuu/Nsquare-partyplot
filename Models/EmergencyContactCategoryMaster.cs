namespace ERPAdmin.Models
{
    public class GetEmergencyContactCategoryRequest
    {
        public long? Id { get; set; }
    }
    public class GetEmergencyContactCategoryResponse
    {
        public long Id { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class SaveEmergencyContactCategoryRequest
    {
        public long? Id { get; set; }
        public string CategoryName { get; set; }
        public string? Description { get; set; }
        public long ActionBy { get; set; }
    }
    public class DeleteEmergencyContactCategoryRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

}
