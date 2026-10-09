namespace ERPAdmin.Models
{
    public class SaveMuhuratModel : BaseClass
    {
        public int Id { get; set; }
        public string MuhuratDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Title { get; set; }
    }
    public class DeleteMuhuratModel : BaseClass
    {
        public int Id { get; set; }
    }
    public class GetMuhuratModelRequest
    {
        public int Id { get; set; } = 0;
        public string? MuhuratDate { get; set; } = null;
    }
    public class GetMuhuratModelResponse
    {
        public int Id { get; set; }
        public string MuhuratDate { get; set; }
        public string MuhuratDate1 { get; set; }
        public string StartTime { get; set; }
        public string StartTime1 { get; set; }
        public string EndTime { get; set; }
        public string EndTime1 { get; set; }
        public string Title { get; set; }
    }
}
