namespace ERPAdmin.Models
{
    public class ExpenseSubCategoryModel
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
    }
    public class GetExpenseSubCategoryRequest
    {
        public long Id { get; set; } = 0;
        public long CategoryId { get; set; } = 0;
    }

    public class SaveExpenseSubCategoryRequest : BaseClass
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteExpenseSubCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
