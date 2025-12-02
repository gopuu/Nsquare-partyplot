namespace ERPAdmin.Models
{
    //public class CompanyImage
    //{
    //    public long ImageId { get; set; }
    //    public string? ImageType { get; set; }  // "FLOW" or "PROJECT"
    //    public string? ImagePath { get; set; }
    //    public string? ImageInfo { get; set; }
    //}
    public class Company
    {
        public Int64 Company_Id { get; set; }
        public string? Name { get; set; }
        public string? ContactName { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? GSTNo { get; set; }
        public string? PANNo { get; set; }
        public string? StateCode { get; set; }
        public string? Logo { get; set; }
        public string? Esign { get; set; }
        public string? OwnerName { get; set; }
        public string? FirmType { get; set; }
        public string? ContactNo2 { get; set; }
        public string? Description { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? Declarations { get; set; }
        //public List<CompanyImage> FlowAndProjectImages { get; set; } = new List<CompanyImage>();
    }
    //public class CompanySave_FlowProject
    //{
    //    public Int64 ImageId { get; set; }
    //    public string? ImageType { get; set; }
    //    public string? Image { get; set; }
    //    public string? ImageInfo { get; set; }
    //}
    public class CompanySave : BaseClass
    {
        public Int64 Company_Id { get; set; }
        public string? Name { get; set; }
        public string? ContactName { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? GSTNo { get; set; }
        public string? PANNo { get; set; }
        public string? StateCode { get; set; }
        public string? Logo { get; set; }
        public string? Esign { get; set; }
        public string? OwnerName { get; set; }
        public string? FirmType { get; set; }
        public string? ContactNo2 { get; set; }
        public string? Description { get; set; }
        public string? TermsAndConditions { get; set; }
        public string? Declarations { get; set; }


    }
}
