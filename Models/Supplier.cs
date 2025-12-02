namespace ERPAdmin.Models
{
    //GET SUPPLIER REQUEST
    public class GetSupplierRequest
    {
        public long? Id { get; set; }
    }
    public class GetSupplier
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? GSTNo { get; set; }

        public string? RegisterAddress { get; set; }
        public long? RegisterStateId { get; set; }
        public long? RegisterCity { get; set; }
        public string? RegisterPincode { get; set; }

        public string? ShippingAddress { get; set; }

        public string? FSSAI_LicNo { get; set; }
        public string? PAN_No { get; set; }
        public string? MSME_No { get; set; }
        public string? TAN_No { get; set; }
        public string? CIN_No { get; set; }

        public double? TCS_Percent { get; set; }
        public long? SupplierType { get; set; }

        public string? Documents { get; set; }

        public bool IsActive { get; set; }
        public long ActionBy { get; set; }
    }

    // DELETE SUPPLIER
    public class DeleteSupplier : BaseClass
    {
        public long Id { get; set; }
    }

    // SAVE SUPPLIER
    public class SaveSupplier : BaseClass
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? GSTNo { get; set; }

        public string? RegisterAddress { get; set; }
        public long? RegisterStateId { get; set; }
        public long? RegisterCity { get; set; }
        public string? RegisterPincode { get; set; }

        public string? ShippingAddress { get; set; }

        public string? FSSAI_LicNo { get; set; }
        public string? PAN_No { get; set; }
        public string? MSME_No { get; set; }
        public string? TAN_No { get; set; }
        public string? CIN_No { get; set; }

        public double? TCS_Percent { get; set; }
        public long? SupplierType { get; set; }

        public string? Documents { get; set; }

        public bool IsActive { get; set; }
    }


}
