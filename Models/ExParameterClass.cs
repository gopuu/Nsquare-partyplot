namespace ERPAdmin.Models
{
    public class ExParameterClass
    {
        public string? Ex1 { get; set; }
        public string? Ex2 { get; set; }
        public string? Ex3 { get; set; }

    }
    public class InquiryExParameterClass : ExParameterClass
    {
        public string? Status { get; set; }
        public string? Date { get; set; }
        public string? ToDate { get; set; }
        public Int64? ReasonId { get; set; }
        public Int64? EmployeeId { get; set; }
        public Int64? CategoryId { get; set; }
        public Int64? ProcessForInquiryId { get; set; }
    }
    public class SupportExParameterClass : ExParameterClass
    {
        public Boolean? Status { get; set; }

    }
}
