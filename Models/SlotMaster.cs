namespace ERPAdmin.Models
{
    public class GetSlotRequest
    {
        public long Id { get; set; } = 0;
        public bool IsActive { get; set; } = false;
    }
    public class GetSlotResponse
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public string? StartTime2 { get; set; }
        public string? EndTime2 { get; set; }
        public string? StartTimeHH { get; set; }
        public string? StartTimeMM { get; set; }
        public string? StartTimeAMPM { get; set; }
        public string? EndTimeHH { get; set; }
        public string? EndTimeMM { get; set; }
        public string? EndTimeAMPM { get; set; }
        public bool IsActive { get; set; }
    }

    public class SaveSlotBulkRequest
    {
        public List<SaveSlotItem> Slots { get; set; }
        public long ActionBy { get; set; }
    }

    public class SaveSlotItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
