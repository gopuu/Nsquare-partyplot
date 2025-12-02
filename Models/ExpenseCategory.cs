namespace ERPAdmin.Models
{
    public class ExpenseCategoryModel
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
    public class GetExpenseCategoryRequest
    {
        public long Id { get; set; } = 0;
    }

    public class SaveExpenseCategoryRequest : BaseClass
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteExpenseCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }


}
