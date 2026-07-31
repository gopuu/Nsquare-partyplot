namespace ERPAdmin.Models
{
    public class InfinityGrowth_SOPDocumentRequest : BaseClass
    {
    }
    public class InfinityGrowth_SOPLogRequest : BaseClass
    {
        public long DocumentId { get; set; }
    }

    public class InfinityGrowth_SOPDocumentModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public long ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; } = "";
        public int SortOrder { get; set; }
    }

    public class InfinityGrowth_SOPDocumentViewModel
    {
        public long Id { get; set; }
        public string Title { get; set; } = "";
        public string FileName { get; set; } = "";
        public string RelativeFilePath { get; set; } = "";
    }

}


