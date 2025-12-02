namespace ERPAdmin.Models
{
    public class AccountTypeModel_Request
    {
        public long Id { get; set; }
    }
    public class AccountTypeModel
    {
        public long Id { get; set; }
        public string? TypeName { get; set; }
        public string? TypeCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
    public class SaveAccountTypeRequest : BaseClass
    {
        public long Id { get; set; }
        public string? TypeName { get; set; }
        public string? TypeCode { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class DeleteAccountTypeRequest : BaseClass
    {
        public long Id { get; set; }
    }

}
