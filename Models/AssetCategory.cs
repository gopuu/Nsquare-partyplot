namespace ERPAdmin.Models
{
    public class GetAssetCategoryRequest
    {
        public long? Id { get; set; }
    }
    public class GetAssetCategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
    }

    public class SaveAssetCategoryRequest : BaseClass
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteAssetCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
