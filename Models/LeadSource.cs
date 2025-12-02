namespace ERPAdmin.Models
{
    public class LeadSource
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class SaveLeadSource : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class DeleteLeadSource : BaseClass
    {
        public Int64 Id { get; set; }
    }
}
