namespace ERPAdmin.Models
{
    public class GetEmergencyContactRequest
    {
        public long? Id { get; set; }
        public long? CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }

    public class SaveEmergencyContactRequest
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string ContactType { get; set; }
        public string ContactName { get; set; }
        public string MobileNo { get; set; }
        public string? MobileNo2 { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public long ActionBy { get; set; }
    }

    public class DeleteEmergencyContactRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }

    public class GetEmergencyContactResponse
    {
        public long Id { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string ContactType { get; set; }
        public string ContactName { get; set; }
        public string MobileNo { get; set; }
        public string? MobileNo2 { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
    }


}
