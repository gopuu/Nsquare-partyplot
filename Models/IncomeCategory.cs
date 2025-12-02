namespace ERPAdmin.Models
{
    public class IncomeCategoryModel
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
    public class GetIncomeCategoryRequest
    {
        public long Id { get; set; } = 0;
    }

    public class SaveIncomeCategoryRequest : BaseClass
    {
        public long Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }

    public class DeleteIncomeCategoryRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
