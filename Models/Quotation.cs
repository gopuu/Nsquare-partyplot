namespace ERPAdmin.Models
{
    //GET
    public class GetQuotationRequest
    {
        public long? Id { get; set; }
    }

    public class GetQuotationResponse
    {
        public long Id { get; set; }
        public long? BookingId { get; set; }
        public long BanquetId { get; set; }
        public string? BanquetName { get; set; }
        public long SlotId { get; set; }
        public string? SlotName { get; set; }

        public string? FromDate { get; set; }
        public string? TillDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }

        public long? ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? ContactNo { get; set; }
        public string? GSTIN { get; set; }
        public long? StateId { get; set; }
        public string? StateName { get; set; }

        public string? EventSubTypeId { get; set; }
        public string? EventSubTypeName { get; set; }

        public decimal BanquetRent { get; set; }
        public int Pax { get; set; }
        public string? ReferenceTypeId { get; set; }
        public string? ReferenceTypeName { get; set; }

        public decimal DecorationChargesAmount { get; set; }
        public decimal SoundSystemChargesAmount { get; set; }
        public decimal ProjectorChargesAmount { get; set; }

        public bool IsPrintPriceExclusive { get; set; }
        public bool IsPrintFoodItems { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string? Notes { get; set; }

        public List<QuotationPackageModel>? Packages { get; set; } = new();
        public List<QuotationOtherServiceModel>? OtherServices { get; set; } = new();
    }
    public class QuotationPackageModel
    {
        public long Id { get; set; }
        public long PackageId { get; set; }
    }
    public class QuotationOtherServiceModel
    {
        public long Id { get; set; }
        public int? SrNo { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public decimal Amount { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    //SAVE
    public class SaveQuotationRequest : BaseClass
    {
        public long Id { get; set; }

        public long? BookingId { get; set; }
        public long BanquetId { get; set; }
        public long SlotId { get; set; }
        public string FromDate { get; set; }
        public string TillDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public long? ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? ContactNo { get; set; }
        public string? GSTIN { get; set; }
        public long? StateId { get; set; }

        public string? EventSubTypeId { get; set; }
        public decimal BanquetRent { get; set; }
        public int Pax { get; set; }
        public string? ReferenceTypeId { get; set; }

        public decimal DecorationChargesAmount { get; set; }
        public decimal SoundSystemChargesAmount { get; set; }
        public decimal ProjectorChargesAmount { get; set; }

        public bool IsPrintPriceExclusive { get; set; }
        public bool IsPrintFoodItems { get; set; }

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal GrandTotal { get; set; }

        public string? Notes { get; set; }

        // JSON strings
        public string? PackagesJson { get; set; }
        public string? OtherServicesJson { get; set; }

        public long ActionBy { get; set; }
    }

    //DELETE
    public class DeleteQuotationRequest : BaseClass
    {
        public long Id { get; set; }
    }
}
