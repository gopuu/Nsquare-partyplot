namespace ERPAdmin.Models
{
    //GET
    public class GetVendorWorkOrderRequest
    {
        public long? Id { get; set; }
        public long? BookingId { get; set; }
    }
    public class GetVendorWorkOrderResponse
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public long ServiceId { get; set; }
        public long VendorId { get; set; }
        public string? ConcernPerson { get; set; }
        public string? PODate { get; set; }
        public string? EventDate { get; set; }
        public string? EventTime { get; set; }
        public long? CustomerId { get; set; }
        public long? SlotId { get; set; }
        public string? Note { get; set; }

        public List<GetVendorWorkOrderParticular>? Particulars { get; set; }
    }
    public class GetVendorWorkOrderParticular
    {
        public string? Service { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public string? Description { get; set; }
    }

    //SAVE
    public class SaveVendorWorkOrderRequest : BaseClass
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public long ServiceId { get; set; }
        public long VendorId { get; set; }
        public string? ConcernPerson { get; set; }
        public string? PODate { get; set; }
        public string? EventDate { get; set; }
        public string? EventTime { get; set; }
        public long? CustomerId { get; set; }
        public long? SlotId { get; set; }
        public string Note { get; set; }

        public List<VendorWorkOrderParticular> Particulars { get; set; }

    }
    public class VendorWorkOrderParticular
    {
        public string? Service { get; set; }
        public decimal Qty { get; set; }
        public decimal Amount { get; set; }
        public decimal Total { get; set; }
        public string? Description { get; set; }
    }
    
    //DELETE
    public class DeleteVendorWorkOrderRequest : BaseClass
    {
        public long Id { get; set; }
    }

    //SLOT MASTER
    public class GetSlotMasterRequest
    {
        public long? Id { get; set; }
    }
    public class SlotMasterResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

}
