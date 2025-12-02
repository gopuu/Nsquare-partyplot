namespace ERPAdmin.Models
{
    public class EventSubType_Request
    {
        public Int64 EventTypeId { get; set; }
    }
    public class EventSubType
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 EventTypeId { get; set; }
        public string? EventTypeName { get; set; }
    }
    public class EventSubTypeById
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
    }
    public class SaveEventSubType : BaseClass
    {
        public Int64 Id { get; set; }
        public string? Name { get; set; }
        public Int64 EventTypeId { get; set; }
    }
    public class DeleteEventSubType : BaseClass
    {
        public Int64 Id { get; set; }
    }
}
