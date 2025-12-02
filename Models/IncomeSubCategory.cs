namespace ERPAdmin.Models
{
    public class IncomeSubCategoryModel
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
    }
    public class GetIncomeSubCategoryRequest
    {
        public long Id { get; set; } = 0;
        public long CategoryId { get; set; } = 0;
    }

    public class SaveIncomeSubCategoryRequest : BaseClass
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteIncomeSubCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }
}
