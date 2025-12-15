namespace ERPAdmin.Models
{
    //GET
    public class GetAssetSubCategoryRequest
    {
        public long? Id { get; set; }
        public long? CategoryId { get; set; }
    }

    public class GetAssetSubCategoryResponse
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
    }

    //SAVE
    public class SaveAssetSubCategoryRequest : BaseClass
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
    }
    //DELETE

    public class DeleteAssetSubCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
