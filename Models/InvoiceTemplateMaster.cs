namespace ERPAdmin.Models
{
    //GET
    public class GetInvoiceTemplateMasterRequest
    {
        public long Id { get; set; } = 0;
        public long? CategoryId { get; set; }
    }
    public class InvoiceTemplateMasterModel
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? ImageName { get; set; }
        public string? PdfName { get; set; }
        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? MethodName { get; set; }
        public bool Is_Selected { get; set; }
    }

    //SAVE
    public class UpdateTemplateSelectedRequest
    {
        public long TemplateId { get; set; }
        public long CategoryId { get; set; }
        public long? ActionBy { get; set; }
    }

}
