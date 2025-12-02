namespace ERPAdmin.Models
{
    public class Process
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 IndexNo { get; set; }
    }
    public class SaveProcess : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 IndexNo { get; set; }
    }
    public class DeleteProcess : BaseClass
    {
        public Int64 Id { get; set; }
    }
}
