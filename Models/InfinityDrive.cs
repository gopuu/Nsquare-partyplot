namespace ERPAdmin.Models
{
    //GET REQUEST
    public class InfinityDriveFileRequest
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
    }

    // GET RESPONSE
    public class InfinityDriveFileModel
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public string DocumentName { get; set; } = "";
        public string DocumentFileName { get; set; } = "";
        public int SortOrder { get; set; }
    }

    // SAVE
    public class InfinityDriveFileSaveRequest
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string DocumentName { get; set; } = "";
        public int SortOrder { get; set; }
        public string? FileBase64 { get; set; }
        public long ActionBy { get; set; }
    }

    //DELETE
    public class InfinityDriveFileDeleteRequest : BaseClass
    {
        public long Id { get; set; }
    }
}
