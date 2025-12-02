namespace ERPAdmin.Models
{
    public class GetItemCategoryRequest
    {
        public long Id { get; set; }
    }

    public class GetItemCategoryList
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveItemCategory : BaseClass
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
    public class DeleteItemCategory : BaseClass
    {
        public long Id { get; set; }
    }
}
