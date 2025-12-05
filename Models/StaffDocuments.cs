namespace ERPAdmin.Models
{
    public class StaffDocuments
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentName { get; set; }
        public string? FilePath { get; set; }
    }
    public class GetStaffDocumentsRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
    }

    public class SaveStaffDocumentsRequest
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? DocumentType { get; set; }
        public string? DocumentName { get; set; }
        public string? FilePath { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteStaffDocumentsRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class StaffDocumentsModel
    {
        public long Id { get; set; }
        public long StaffId { get; set; }
        public string? StaffName { get; set; }

        public string? DocumentType { get; set; }
        public string? DocumentName { get; set; }
        public string? FilePath { get; set; }
    }

}
