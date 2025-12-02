namespace ERPAdmin.Models
{
    public class EventType
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class SaveEventType : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class DeleteEventType : BaseClass
    {
        public Int64 Id { get; set; }
    }
}
