namespace ERPAdmin.Models
{
    public class ClientMasterModel_Request
    {
        public long Id { get; set; } = 0;
    }
    public class ClientMasterModel
    {
        public long Id { get; set; }
        public string? ClientCode { get; set; }
        public string? Name { get; set; }
        public string? Contact { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? GSTIN { get; set; }
        public long StateId { get; set; }
    }
}
