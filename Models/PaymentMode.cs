namespace ERPAdmin.Models
{
    //GET
    public class GetPaymentMode_Request 
    {
        public long Id { get; set; }
    }
    public class GetPaymentMode
    {
        public long Id { get; set; }
        public string? PaymentModeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
    //SAVE
    public class SavePaymentMode : BaseClass
    {
        public long Id { get; set; }
        public string? PaymentModeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    //Delete
    public class DeletePaymentMode : BaseClass
    {
        public long Id { get; set; }
    }


}
