namespace ERPAdmin.Models
{
    //GET
    public class GetVendorWorkOrderServiceRequest
    {
        public long? Id { get; set; }
    }

    public class VendorWorkOrderServiceResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    //SAVE
    public class SaveVendorWorkOrderServiceRequest
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long ActionBy { get; set; }
    }


    //DELETE
    public class DeleteVendorWorkOrderServiceRequest
    {
        public long Id { get; set; }
        public long ActionBy { get; set; }
    }
}
