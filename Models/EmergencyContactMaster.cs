namespace ERPAdmin.Models
{
    //GET
    public class GetEmergencyContactRequest
    {
        public long? Id { get; set; }
        public long? CategoryId { get; set; }
        public long? SubCategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
    public class GetEmergencyContactResponse
    {
        public long Id { get; set; }
        public long SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }

        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ContactType { get; set; }
        public string? ContactName { get; set; }
        public string? MobileNo { get; set; }
        public string? MobileNo2 { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
    }

    //SAVE
    public class SaveEmergencyContactRequest : BaseClass
    {
        public long Id { get; set; }
        public long SubCategoryId { get; set; }   // FIXED
        public string ContactType { get; set; }
        public string ContactName { get; set; }
        public string MobileNo { get; set; }
        public string? MobileNo2 { get; set; }
        public string? Location { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
    }


    //DELETE
    public class DeleteEmergencyContactRequest : BaseClass
    {
        public long Id { get; set; }
    }


}
