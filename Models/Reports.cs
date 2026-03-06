namespace ERPAdmin.Models
{
    public class EmergencyContactReportRequest : BaseClass
    {
        public long Id { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public bool? IsActive { get; set; }
        public string? Columns { get; set; }
        public bool? WithHeader { get; set; }
    }

}
